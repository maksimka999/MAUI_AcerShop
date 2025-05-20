using AcerShop.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AcerShop.Services;
public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://172.20.10.8:7205";
    private readonly JsonSerializerOptions _serializerOptions = new
    JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition =
System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
    public ApiService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    public async Task<User?> EnsureUserCreated(string firebaseUid,
 string? email = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(firebaseUid))
            {
                Console.WriteLine("API Service Error: Firebase UID is required for EnsureUserCreated.");
                return null;
            }

            var requestUrl =
$"/api/users/ensure-created?firebaseUid={Uri.EscapeDataString(firebaseUid)}";
            if (!string.IsNullOrWhiteSpace(email))
            {
                requestUrl +=
$"&email={Uri.EscapeDataString(email)}";
            }

            var response = await _httpClient.PostAsync(requestUrl,
 null);

            if (response.IsSuccessStatusCode)
            {
                var user = await
response.Content.ReadFromJsonAsync<User>();
                if (user == null)
                {
                    Console.WriteLine("API Service Error: Received null user object from ensure-created.");
                }
                return user;
            }
            else
            {
                Console.WriteLine($"API Service Error (EnsureUserCreated): {response.StatusCode} - {await
response.Content.ReadAsStringAsync()}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Exception (EnsureUserCreated): {ex}");
            return null;
        }
    }

    public async Task<ObservableCollection<Product>>
GetProductsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/products");
            if (response.IsSuccessStatusCode)
            {
                var productsList = await
response.Content.ReadFromJsonAsync<List<Product>>();
                return new
ObservableCollection<Product>(productsList ?? new
List<Product>());
            }
            else
            {
                Console.WriteLine($"Error getting products: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return new ObservableCollection<Product>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Exception (GetProductsAsync): {ex}");
            return new ObservableCollection<Product>();
        }
    }
    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            var response = await
_httpClient.PutAsJsonAsync("/api/users/update", user);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Exception (UpdateUserAsync): {ex}");
            return false;
        }
    }
    public async Task<bool> DeleteProductAsync(int productId)
    {
        try
        {
            var response = await
_httpClient.DeleteAsync($"/api/products/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await
response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error (DeleteProductAsync): {response.StatusCode} - {errorContent}");
                return false;
            }

            Console.WriteLine($"Successfully deleted product ID {productId}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Exception (DeleteProductAsync): {ex}");
            return false;
        }
    }

    public async Task<bool> UpdateCartItemAsync(int userId, int
productId, int quantity)
    {
        try
        {
            var itemDto = new CartItemDto
            {
                ProductId = productId,
                Quantity = quantity
            };
            var response = await
_httpClient.PutAsJsonAsync($"/api/cart/update?userId={userId}",
itemDto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Exception (UpdateCartItemAsync): {ex}");
            return false;
        }
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int
productId)
    {
        try
        {
            var response = await
_httpClient.DeleteAsync($"/api/cart/remove?userId={userId}&productId={productId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Exception (RemoveFromCartAsync): {ex}");
            return false;
        }
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        try
        {
            var response = await
_httpClient.DeleteAsync($"/api/cart/clear?userId={userId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Exception (ClearCartAsync): {ex}");
            return false;
        }
    }
    public async Task<bool> CheckoutAsync(int userId)
    {
        try
        {
            var response = await
_httpClient.PostAsync($"/api/cart/checkout?userId={userId}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Exception (CheckoutAsync): {ex}");
            return false;
        }
    }
    public async Task<bool> AddToCartAsync(int userId, int
productId, int quantity = 1)
    {
        try
        {
            var itemDto = new CartItemDto
            {
                ProductId = productId,
                Quantity = quantity
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"/api/cart/add?userId={userId}",
                itemDto);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Exception (AddToCartAsync): {ex}");
            return false;
        }
    }

    public async Task<ProductDtoForApi?> GetProductByIdAsync(int
productId)
    {
        try
        {
            var response = await
_httpClient.GetAsync($"/api/products/{productId}");
            if (response.IsSuccessStatusCode)
            {
                return await
response.Content.ReadFromJsonAsync<ProductDtoForApi>();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Error (GetProductByIdAsync): {ex}");
            return null;
        }
    }

    public async Task<bool> UpdateProductAsync(Product
productToUpdate)
    {
        if (productToUpdate == null) return false;

        try
        {
            var productDto = new ProductDtoForApi
            {
                ProductId = productToUpdate.ProductId,
                Name = productToUpdate.Name,
                Description = productToUpdate.Description,
                Price = productToUpdate.Price,
                Quantity = productToUpdate.Quantity,
                TypeName = productToUpdate.TypeName,
                PhotoUrl = productToUpdate.PhotoUrl,
                LaptopDetails = productToUpdate.LaptopDetails,
                ChairDetails = productToUpdate.ChairDetails,
                MouseDetails = productToUpdate.MouseDetails
            };

            string url = $"/api/products/{productToUpdate.ProductId}";
            Console.WriteLine($"ApiService: Sending PUT request to {url}");

            var response = await _httpClient.PutAsJsonAsync(url,
productDto, _serializerOptions);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await
response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error (UpdateProductAsync): {response.StatusCode} - {errorContent}");
                return false;
            }

            Console.WriteLine($"ApiService: Successfully updated product ID {productToUpdate.ProductId}. Status: {response.StatusCode}");
            return true;
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"API HTTP Exception (UpdateProductAsync): {httpEx.Message}");
            if (httpEx.StatusCode.HasValue) Console.WriteLine($"Status Code: {httpEx.StatusCode}");
            return false;
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"API JSON Exception (UpdateProductAsync): {jsonEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API General Exception (UpdateProductAsync): {ex}");
            return false;
        }
    }

    public async Task<CartResponseDto> GetCartAsync(int userId)
    {
        try
        {
            var response = await
_httpClient.GetAsync($"/api/cart?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                return await
response.Content.ReadFromJsonAsync<CartResponseDto>();
            }

            Console.WriteLine($"Error getting cart: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Exception (GetCartAsync): {ex}");
            return null;
        }
    }


    public async Task<bool> AddProductAsync(Product productToAdd)
    {
        string url = "/api/products";
        Debug.WriteLine($"[API SERVICE - AddProductAsync] Отправка POST-запроса на {url}");

        try
        {
            var productDto = new ProductDtoForApi
            {
                Name = productToAdd.Name,
                Description = productToAdd.Description,
                Price = productToAdd.Price,
                Quantity = productToAdd.Quantity,
                TypeName = productToAdd.TypeName,
                PhotoUrl = productToAdd.PhotoUrl,
                LaptopDetails = productToAdd.LaptopDetails,
                ChairDetails = productToAdd.ChairDetails,
                MouseDetails = productToAdd.MouseDetails
            };

            var response = await _httpClient.PostAsJsonAsync(url,
    productDto, _serializerOptions);

            Debug.WriteLine($"[API SERVICE - AddProductAsync] Статус ответа: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[API SERVICE - AddProductAsync] Продукт успешно добавлен. Статус: {response.StatusCode}");
                return true;
            }
            else
            {
                string errorContent = await
    response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[API SERVICE - AddProductAsync] Ошибка: {response.StatusCode}, Содержимое: {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[API SERVICE - AddProductAsync] Ошибка: {ex.Message}");
            return false;
        }
    }

}