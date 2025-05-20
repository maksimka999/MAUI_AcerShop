using Microsoft.EntityFrameworkCore;
using AcerShopBackend.Data;
using AcerShopBackend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(
    "http://172.20.10.8:5079",
    "https://172.20.10.8:7205"
);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена в конфигурации.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseHttpsRedirection();

app.UseCors("AllowAll");


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        Console.WriteLine("Attempting to apply database migrations...");
        dbContext.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully (if any were pending).");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FATAL: An error occurred while migrating the database: {ex.Message}");
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred migrating the DB.");
    }
}


app.MapPost("/api/users/ensure-created", async (AppDbContext db, [FromQuery] string firebaseUid, [FromQuery] string? email) =>
{
    Console.WriteLine($"POST /api/users/ensure-created?firebaseUid={firebaseUid}&email={email}");
    try
    {
        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            Console.WriteLine("Error: Firebase UID is required.");
            return Results.BadRequest("Firebase UID is required");
        }

        Console.WriteLine($"Received UID: {firebaseUid}, Email: {email}");

        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        if (existingUser != null)
        {
            Console.WriteLine($"User already exists: ID={existingUser.Id}, Role={existingUser.CustomRole}, RegDate={existingUser.RegistrationDate}");
            return Results.Ok(existingUser);
        }

        Console.WriteLine($"User not found. Creating new user for FirebaseUID: {firebaseUid}");

        string initialName;
        if (!string.IsNullOrWhiteSpace(email) && email.Contains("@"))
        {
            var parts = email.Split('@');
            if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
            {
                initialName = parts[0];
            }
            else
            {
                initialName = "Новый пользователь";
            }
        }
        else
        {
            initialName = "Новый пользователь";
        }

        var nameMaxLengthAttribute = typeof(User).GetProperty(nameof(User.Name))
                                               ?.GetCustomAttributes(typeof(MaxLengthAttribute), true)
                                               .FirstOrDefault() as MaxLengthAttribute;

        if (nameMaxLengthAttribute != null && initialName.Length > nameMaxLengthAttribute.Length)
        {
            initialName = initialName.Substring(0, nameMaxLengthAttribute.Length);
            Console.WriteLine($"Warning: Initial name truncated to {nameMaxLengthAttribute.Length} characters.");
        }
        if (initialName == null) initialName = "Новый пользователь";

        DateTime defaultDateOfBirth = DateTime.SpecifyKind(new DateTime(2015, 12, 25), DateTimeKind.Utc);
        var newUser = new User
        {
            FirebaseUid = firebaseUid,
            Name = initialName,
            CustomRole = "user",
            RegistrationDate = DateTime.UtcNow,
            Gender = "Не указан",
            DateOfBirth = defaultDateOfBirth,
            PhoneNumber = null,
            Photo = null
        };

        await db.Users.AddAsync(newUser);
        await db.SaveChangesAsync();

        Console.WriteLine($"Created new user: ID={newUser.Id} with role '{newUser.CustomRole}' and Name '{newUser.Name}' for FirebaseUID={newUser.FirebaseUid}");
        return Results.Created($"/api/users/{newUser.FirebaseUid}", newUser);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FATAL Error in ensure-created: {ex}");
        return Results.Problem($"Internal server error occurred.");
    }
});


app.MapGet("/api/products", async (AppDbContext db) =>
{
    Console.WriteLine("GET /api/products");

    try
    {
        var products = await db.Products
            .Include(p => p.LaptopDetails)
            .Include(p => p.ChairDetails)
            .Include(p => p.MouseDetails)
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                TypeName = p.TypeName,
                PhotoUrl = p.Photo,
                LaptopDetails = p.LaptopDetails,
                ChairDetails = p.ChairDetails,
                MouseDetails = p.MouseDetails
            })
            .ToListAsync();
        return Results.Ok(products);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in getting products: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        }
        return Results.Problem($"Internal server error: {ex.Message}");
    }
});

app.MapPut("/api/users/update", async (AppDbContext db, [FromBody] User updatedUser) =>
{
    Console.WriteLine($"PUT /api/users/update for user ID: {updatedUser?.Id}");

    try
    {
        if (updatedUser == null)
        {
            Console.WriteLine("Error: User data is null");
            return Results.BadRequest("User data is required");
        }

        var existingUser = await db.Users.FindAsync(updatedUser.Id);
        if (existingUser == null)
        {
            Console.WriteLine($"Error: User with ID {updatedUser.Id} not found");
            return Results.NotFound("User not found");
        }

        existingUser.Name = updatedUser.Name;
        existingUser.Gender = updatedUser.Gender;
        existingUser.DateOfBirth = updatedUser.DateOfBirth.Value.ToUniversalTime();
        existingUser.PhoneNumber = updatedUser.PhoneNumber;

        if (updatedUser.Photo != null && updatedUser.Photo.Length > 0)
        {
            existingUser.Photo = updatedUser.Photo;
        }

        db.Users.Update(existingUser);
        await db.SaveChangesAsync();

        Console.WriteLine($"Successfully updated user ID: {existingUser.Id}");
        return Results.Ok(existingUser);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating user: {ex}");
        return Results.Problem("Internal server error");
    }
});



app.MapDelete("/api/products/{id}", async (int id, AppDbContext db) =>
{
    Console.WriteLine($"DELETE /api/products/{id}");

    try
    {
        var product = await db.Products
            .Include(p => p.LaptopDetails)
            .Include(p => p.ChairDetails)
            .Include(p => p.MouseDetails)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
        {
            Console.WriteLine($"Product with ID {id} not found.");
            return Results.NotFound($"Product with ID {id} not found.");
        }

        if (product.LaptopDetails != null)
        {
            db.LaptopDetails.Remove(product.LaptopDetails);
            Console.WriteLine($"Removed LaptopDetails for product ID {id}");
        }
        else if (product.ChairDetails != null)
        {
            db.ChairDetails.Remove(product.ChairDetails);
            Console.WriteLine($"Removed ChairDetails for product ID {id}");
        }
        else if (product.MouseDetails != null)
        {
            db.MouseDetails.Remove(product.MouseDetails);
            Console.WriteLine($"Removed MouseDetails for product ID {id}");
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync();

        Console.WriteLine($"Successfully deleted product with ID {id}");
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting product ID {id}: {ex}");
        return Results.Problem($"Internal server error while deleting product ID {id}: {ex.Message}");
    }
});

app.MapGet("/api/cart", async (AppDbContext db, [FromQuery] int userId) =>
{
    Console.WriteLine($"GET /api/cart?userId={userId}");

    try
    {
        var cartItems = await db.UserCarts
            .Where(uc => uc.UserId == userId)
            .Include(uc => uc.Product)
            .ToListAsync();

        if (!cartItems.Any())
        {
            Console.WriteLine($"No cart items found for user ID {userId}");
            return Results.Ok(new CartResponseDto
            {
                UserId = userId,
                Items = new List<CartItemDetailDto>(),
                TotalPrice = 0
            });
        }

        var response = new CartResponseDto
        {
            CartId = cartItems.First().CartId,
            UserId = userId,
            Items = cartItems.Select(ci => new CartItemDetailDto
            {
                ProductId = ci.ProductId,
                Name = ci.Product.Name,
                Price = ci.Product.Price,
                Quantity = ci.Quantity,
                Photo = ci.Product.Photo
            }).ToList(),
            TotalPrice = cartItems.Sum(ci => ci.Product.Price * ci.Quantity)
        };

        Console.WriteLine($"Returning {response.Items.Count} cart items for user ID {userId}");
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting cart for user ID {userId}: {ex}");
        return Results.Problem($"Internal server error: {ex.Message}");
    }
});

app.MapGet("/api/products/{id}", async (AppDbContext db, int id) =>
{
    var product = await db.Products
        .Include(p => p.LaptopDetails)
        .Include(p => p.ChairDetails)
        .Include(p => p.MouseDetails)
        .FirstOrDefaultAsync(p => p.ProductId == id);

    if (product == null)
    {
        return Results.NotFound("Product not found");
    }

    return Results.Json(product, new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    });
}).WithName("GetProductById");

app.MapPost("/api/cart/add", async (AppDbContext db, [FromBody] CartItemDto itemDto, [FromQuery] int userId) =>
{
    Console.WriteLine($"POST /api/cart/add?userId={userId} for product ID {itemDto.ProductId}");

    try
    {
        var user = await db.Users.FindAsync(userId);
        if (user == null)
        {
            Console.WriteLine($"User with ID {userId} not found");
            return Results.NotFound("User not found");
        }

        var product = await db.Products.FindAsync(itemDto.ProductId);
        if (product == null)
        {
            Console.WriteLine($"Product with ID {itemDto.ProductId} not found");
            return Results.NotFound("Product not found");
        }

        if (itemDto.Quantity <= 0)
        {
            Console.WriteLine($"Invalid quantity: {itemDto.Quantity}");
            return Results.BadRequest("Quantity must be positive");
        }

        if (product.Quantity < itemDto.Quantity)
        {
            Console.WriteLine($"Not enough stock. Requested: {itemDto.Quantity}, Available: {product.Quantity}");
            return Results.BadRequest("Not enough stock");
        }

        var existingItem = await db.UserCarts
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ProductId == itemDto.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += itemDto.Quantity;
            Console.WriteLine($"Updated quantity for product ID {itemDto.ProductId} in cart. New quantity: {existingItem.Quantity}");
        }
        else
        {
            var newCartItem = new UserCart
            {
                UserId = userId,
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity
            };
            db.UserCarts.Add(newCartItem);
            Console.WriteLine($"Added new product ID {itemDto.ProductId} to cart with quantity {itemDto.Quantity}");
        }

        await db.SaveChangesAsync();
        return Results.Ok("Product added to cart successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error adding to cart: {ex}");
        return Results.Problem($"Internal server error: {ex.Message}");
    }
});

app.MapPut("/api/cart/update", async (AppDbContext db, [FromBody] CartItemDto itemDto, [FromQuery] int userId) =>
{
    Console.WriteLine($"PUT /api/cart/update?userId={userId} for product ID {itemDto.ProductId}");

    try
    {
        var cartItem = await db.UserCarts
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ProductId == itemDto.ProductId);

        if (cartItem == null)
        {
            Console.WriteLine($"Product ID {itemDto.ProductId} not found in cart for user ID {userId}");
            return Results.NotFound("Product not found in cart");
        }

        var product = await db.Products.FindAsync(itemDto.ProductId);
        if (product == null)
        {
            Console.WriteLine($"Product with ID {itemDto.ProductId} not found");
            return Results.NotFound("Product not found");
        }

        if (itemDto.Quantity <= 0)
        {
            db.UserCarts.Remove(cartItem);
            Console.WriteLine($"Removed product ID {itemDto.ProductId} from cart due to zero quantity");
        }
        else if (product.Quantity < itemDto.Quantity)
        {
            Console.WriteLine($"Not enough stock. Requested: {itemDto.Quantity}, Available: {product.Quantity}");
            return Results.BadRequest("Not enough stock");
        }
        else
        {
            cartItem.Quantity = itemDto.Quantity;
            Console.WriteLine($"Updated quantity for product ID {itemDto.ProductId} to {itemDto.Quantity}");
        }

        await db.SaveChangesAsync();
        return Results.Ok("Cart updated successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating cart: {ex}");
        return Results.Problem($"Internal server error: {ex.Message}");
    }
});

app.MapDelete("/api/cart/remove", async (AppDbContext db, [FromQuery] int userId, [FromQuery] int productId) =>
{
    Console.WriteLine($"DELETE /api/cart/remove?userId={userId}&productId={productId}");

    try
    {
        var cartItem = await db.UserCarts
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ProductId == productId);

        if (cartItem == null)
        {
            Console.WriteLine($"Product ID {productId} not found in cart for user ID {userId}");
            return Results.NotFound("Product not found in cart");
        }

        db.UserCarts.Remove(cartItem);
        await db.SaveChangesAsync();

        Console.WriteLine($"Removed product ID {productId} from cart for user ID {userId}");
        return Results.Ok("Product removed from cart");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error removing from cart: {ex}");
        return Results.Problem($"Internal server error: {ex.Message}");
    }
});

app.MapDelete("/api/cart/clear", async (AppDbContext db, [FromQuery] int userId) =>
{
    Console.WriteLine($"DELETE /api/cart/clear?userId={userId}");

    try
    {
        var cartItems = await db.UserCarts
            .Where(uc => uc.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            Console.WriteLine($"No items found in cart for user ID {userId}");
            return Results.Ok("Cart is already empty");
        }

        db.UserCarts.RemoveRange(cartItems);
        await db.SaveChangesAsync();

        Console.WriteLine($"Cleared cart for user ID {userId}. Removed {cartItems.Count} items.");
        return Results.Ok("Cart cleared successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error clearing cart: {ex}");
        return Results.Problem($"Internal server error: {ex.Message}");
    }
});

app.MapPut("/api/products/{id}", async (int id, [FromBody] ProductDto updatedProductDto, AppDbContext db) =>
{
    Console.WriteLine($"PUT /api/products/{id}");
    try
    {
        var existingProduct = await db.Products
            .Include(p => p.LaptopDetails)
            .Include(p => p.ChairDetails)
            .Include(p => p.MouseDetails)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (existingProduct == null)
        {
            Console.WriteLine($"Error: Product with ID {id} not found.");
            return Results.NotFound($"Product with ID {id} not found.");
        }

        existingProduct.Name = updatedProductDto.Name;
        existingProduct.Description = updatedProductDto.Description;
        existingProduct.Price = updatedProductDto.Price;
        existingProduct.Quantity = updatedProductDto.Quantity;
        existingProduct.TypeName = updatedProductDto.TypeName;

        if (updatedProductDto.PhotoUrl != null && updatedProductDto.PhotoUrl.Length > 0)
        {
            existingProduct.Photo = updatedProductDto.PhotoUrl;
            Console.WriteLine($"Photo updated for product ID {id}. Size: {updatedProductDto.PhotoUrl.Length} bytes.");
        }
        else
        {
            Console.WriteLine($"No new photo provided for product ID {id}. Keeping existing photo.");
        }


        if (existingProduct.TypeName.Equals("Laptop", StringComparison.OrdinalIgnoreCase) && updatedProductDto.LaptopDetails != null)
        {
            if (existingProduct.LaptopDetails == null)
            {
                existingProduct.LaptopDetails = new LaptopDetails { ProductId = id };
                db.LaptopDetails.Add(existingProduct.LaptopDetails);
            }
            existingProduct.LaptopDetails.Processor = updatedProductDto.LaptopDetails.Processor;
            existingProduct.LaptopDetails.Ram = updatedProductDto.LaptopDetails.Ram;
            existingProduct.LaptopDetails.StorageSize = updatedProductDto.LaptopDetails.StorageSize;
            existingProduct.LaptopDetails.ScreenSize = updatedProductDto.LaptopDetails.ScreenSize;
            existingProduct.LaptopDetails.GraphicsCard = updatedProductDto.LaptopDetails.GraphicsCard;
            existingProduct.LaptopDetails.OperatingSystem = updatedProductDto.LaptopDetails.OperatingSystem;
            existingProduct.LaptopDetails.BatteryLife = updatedProductDto.LaptopDetails.BatteryLife;
            existingProduct.LaptopDetails.Weight = updatedProductDto.LaptopDetails.Weight;
        }
        else if (existingProduct.TypeName.Equals("Chair", StringComparison.OrdinalIgnoreCase) && updatedProductDto.ChairDetails != null)
        {
            if (existingProduct.ChairDetails == null)
            {
                existingProduct.ChairDetails = new ChairDetails { ProductId = id };
                db.ChairDetails.Add(existingProduct.ChairDetails);
            }
            existingProduct.ChairDetails.Material = updatedProductDto.ChairDetails.Material;
            existingProduct.ChairDetails.Color = updatedProductDto.ChairDetails.Color;
            existingProduct.ChairDetails.WeightCapacity = updatedProductDto.ChairDetails.WeightCapacity;
            existingProduct.ChairDetails.AdjustableFeatures = updatedProductDto.ChairDetails.AdjustableFeatures;
            existingProduct.ChairDetails.Dimensions = updatedProductDto.ChairDetails.Dimensions;
            existingProduct.ChairDetails.WarrantyYears = updatedProductDto.ChairDetails.WarrantyYears;
            existingProduct.ChairDetails.ComfortRating = updatedProductDto.ChairDetails.ComfortRating;
        }
        else if (existingProduct.TypeName.Equals("Mouse", StringComparison.OrdinalIgnoreCase) && updatedProductDto.MouseDetails != null)
        {
            if (existingProduct.MouseDetails == null)
            {
                existingProduct.MouseDetails = new MouseDetails { ProductId = id };
                db.MouseDetails.Add(existingProduct.MouseDetails);
            }
            existingProduct.MouseDetails.Dpi = updatedProductDto.MouseDetails.Dpi;
            existingProduct.MouseDetails.Buttons = updatedProductDto.MouseDetails.Buttons;
            existingProduct.MouseDetails.ConnectionType = updatedProductDto.MouseDetails.ConnectionType;
            existingProduct.MouseDetails.ErgonomicDesign = updatedProductDto.MouseDetails.ErgonomicDesign;
            existingProduct.MouseDetails.SensitivityAdjustment = updatedProductDto.MouseDetails.SensitivityAdjustment;
            existingProduct.MouseDetails.WirelessRange = updatedProductDto.MouseDetails.WirelessRange;
            existingProduct.MouseDetails.CompatibilityPlatforms = updatedProductDto.MouseDetails.CompatibilityPlatforms;
        }

        db.Products.Update(existingProduct);
        await db.SaveChangesAsync();

        Console.WriteLine($"Successfully updated product ID: {existingProduct.ProductId}");
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating product ID {id}: {ex}");
        return Results.Problem($"Internal server error while updating product ID {id}: {ex.Message}");
    }
});

app.MapPost("/api/products", async (AppDbContext db, [FromBody] ProductDto productDto) => // РЕКОМЕНДАЦИЯ: Используйте CreateProductDto здесь
{
    Console.WriteLine("POST /api/products");
    // Базовая валидация
    if (productDto == null ||
        string.IsNullOrWhiteSpace(productDto.Name) ||
        string.IsNullOrWhiteSpace(productDto.TypeName) ||
        productDto.Price <= 0 ||
        productDto.Quantity < 0)
    {
        Console.WriteLine("Error: Invalid product data. Name, TypeName, Price (>0), and Quantity (>=0) are required.");
        return Results.BadRequest("Invalid product data. Name, TypeName, Price (>0), and Quantity (>=0) are required.");
    }


    try
    {
        var product = new Product
        {
            // ProductId НЕ должен устанавливаться из productDto при создании,
            // он генерируется базой данных.
            // ProductId = productDto.ProductId, // <--- ЭТУ СТРОКУ СЛЕДУЕТ УДАЛИТЬ ИЛИ ИСПОЛЬЗОВАТЬ CreateProductDto

            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            Quantity = productDto.Quantity,
            TypeName = productDto.TypeName,
            Photo = productDto.PhotoUrl
        };

        Console.WriteLine($"Creating product of type: {product.TypeName}");

        // Добавление деталей в зависимости от типа продукта
        if (product.TypeName.Equals("Laptop", StringComparison.OrdinalIgnoreCase) && productDto.LaptopDetails != null)
        {
            Console.WriteLine("Adding LaptopDetails.");
            product.LaptopDetails = new LaptopDetails
            {
                Processor = productDto.LaptopDetails.Processor,
                Ram = productDto.LaptopDetails.Ram,
                StorageSize = productDto.LaptopDetails.StorageSize,
                ScreenSize = productDto.LaptopDetails.ScreenSize,
                GraphicsCard = productDto.LaptopDetails.GraphicsCard,
                OperatingSystem = productDto.LaptopDetails.OperatingSystem,
                BatteryLife = productDto.LaptopDetails.BatteryLife,
                Weight = productDto.LaptopDetails.Weight
            };
        }
        else if (product.TypeName.Equals("Chair", StringComparison.OrdinalIgnoreCase) && productDto.ChairDetails != null)
        {
            Console.WriteLine("Adding ChairDetails.");
            product.ChairDetails = new ChairDetails
            {
                Material = productDto.ChairDetails.Material,
                Color = productDto.ChairDetails.Color,
                WeightCapacity = productDto.ChairDetails.WeightCapacity,
                AdjustableFeatures = productDto.ChairDetails.AdjustableFeatures,
                Dimensions = productDto.ChairDetails.Dimensions,
                WarrantyYears = productDto.ChairDetails.WarrantyYears,
                ComfortRating = productDto.ChairDetails.ComfortRating
            };
        }
        else if (product.TypeName.Equals("Mouse", StringComparison.OrdinalIgnoreCase) && productDto.MouseDetails != null)
        {
            Console.WriteLine("Adding MouseDetails.");
            product.MouseDetails = new MouseDetails
            {
                Dpi = productDto.MouseDetails.Dpi,
                Buttons = productDto.MouseDetails.Buttons,
                ConnectionType = productDto.MouseDetails.ConnectionType,
                ErgonomicDesign = productDto.MouseDetails.ErgonomicDesign,
                SensitivityAdjustment = productDto.MouseDetails.SensitivityAdjustment,
                WirelessRange = productDto.MouseDetails.WirelessRange,
                CompatibilityPlatforms = productDto.MouseDetails.CompatibilityPlatforms
            };
        }

        await db.Products.AddAsync(product);
        await db.SaveChangesAsync();

        Console.WriteLine($"Successfully created product ID: {product.ProductId}");

        // ИЗМЕНЕНИЕ: Возвращаем 201 Created с Location header, но без тела ответа
        string resourceUri = $"/api/products/{product.ProductId}"; // Путь к созданному ресурсу
        return Results.Created(resourceUri, null); // null в качестве второго аргумента означает пустое тело
    }
    catch (DbUpdateException dbEx)
    {
        Console.WriteLine($"Error creating product (DbUpdateException): {dbEx.Message} - Inner: {dbEx.InnerException?.Message}");
        return Results.Problem($"Database error: {dbEx.Message} - Inner: {dbEx.InnerException?.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating product (Exception): {ex.ToString()}");
        return Results.Problem($"Internal server error while creating product: {ex.Message}");
    }
});

app.MapPost("/api/cart/checkout", async (AppDbContext db, [FromQuery] int userId) =>
{
    Console.WriteLine($"POST /api/cart/checkout?userId={userId}");

    using var transaction = await db.Database.BeginTransactionAsync();

    try
    {
        var cartItems = await db.UserCarts
            .Where(uc => uc.UserId == userId)
            .Include(uc => uc.Product)
            .ToListAsync();

        if (!cartItems.Any())
        {
            Console.WriteLine($"Cart is empty for user ID {userId}");
            return Results.BadRequest("Cart is empty");
        }

        var validationErrors = new List<string>();
        foreach (var item in cartItems)
        {
            if (item.Product.Quantity < item.Quantity)
            {
                validationErrors.Add(
                    $"{item.Product.Name}: запрошено {item.Quantity}, доступно {item.Product.Quantity}"
                );
            }
        }

        if (validationErrors.Any())
        {
            Console.WriteLine($"Not enough stock: {string.Join(", ", validationErrors)}");
            return Results.BadRequest(new
            {
                Message = "Not enough stock for some products",
                Errors = validationErrors
            });
        }

        var order = new UserCart
        {
            UserId = userId,
            AddedAt = DateTime.UtcNow,
        };

        foreach (var item in cartItems)
        {
            item.Product.Quantity -= item.Quantity;
            db.Products.Update(item.Product);
        }

        db.UserCarts.RemoveRange(cartItems);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        Console.WriteLine($"Checkout successful for user ID {userId}. Removed {cartItems.Count} items.");
        return Results.Ok(new
        {
            Message = "Order processed successfully",
            TotalItems = cartItems.Count,
            TotalAmount = cartItems.Sum(x => x.Product.Price * x.Quantity)
        });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        Console.WriteLine($"Checkout failed for user {userId}: {ex}");
        return Results.Problem("Order processing failed. Please try again.");
    }

   
});
Console.WriteLine("Starting application...");
app.Run();