using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcerShopBackend.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int Id { get; set; }

        [Required]
        [Column("firebase_uid")]
        public string FirebaseUid { get; set; }

        [Required]
        [Column("registration_date")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [Column("name")]
        public string? Name { get; set; }

        [Required]
        [Column("custom_role")]
        public string CustomRole { get; set; }

        [Column("gender")]
        [MaxLength(20)]
        public string? Gender { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("phone_number")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Column("photo")]

        public byte[]? Photo { get; set; }

        public virtual ICollection<UserCart> UserCarts { get; set; } = new List<UserCart>();


    }

    public class Product
    {
        [Key]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("price", TypeName = "numeric(10, 2)")]
        public decimal Price { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("type_name")]
        public string TypeName { get; set; }

        [Required]
        [Column("photo")]
        public byte[] Photo { get; set; }

        public virtual ICollection<UserCart> UserCarts { get; set; } = new List<UserCart>();

        public virtual LaptopDetails? LaptopDetails { get; set; }
        public virtual ChairDetails? ChairDetails { get; set; }
        public virtual MouseDetails? MouseDetails { get; set; }
    }

    /// <summary>
    /// Представляет товар в корзине пользователя (связующая таблица).
    /// </summary>
    [Table("user_cart")]
    public class UserCart
    {
        [Key]
        [Column("cart_id")]
        public int CartId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("added_at")]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }

    /// <summary>
    /// Детали для продукта типа "Ноутбук". Связь один-к-одному с Product.
    /// </summary>
    [Table("laptop_details")]
    public class LaptopDetails
    {

        [Key, ForeignKey("Product")]
        [Column("product_id")]
        public int ProductId { get; set; }

        [MaxLength(50)]
        [Column("processor")]
        public string? Processor { get; set; }

        [MaxLength(20)]
        [Column("ram")]
        public string? Ram { get; set; }

        [MaxLength(20)]
        [Column("storage_size")]
        public string? StorageSize { get; set; }

        [MaxLength(10)]
        [Column("screen_size")]
        public string? ScreenSize { get; set; }

        [MaxLength(50)]
        [Column("graphics_card")]
        public string? GraphicsCard { get; set; }

        [MaxLength(50)]
        [Column("operating_system")]
        public string? OperatingSystem { get; set; }

        [MaxLength(20)]
        [Column("battery_life")]
        public string? BatteryLife { get; set; }

        [MaxLength(20)]
        [Column("weight")]
        public string? Weight { get; set; }

        public virtual Product Product { get; set; }
    }

    [Table("chair_details")]
    public class ChairDetails
    {
        [Key, ForeignKey("Product")]
        [Column("product_id")]
        public int ProductId { get; set; }

        [MaxLength(50)]
        [Column("material")]
        public string? Material { get; set; }

        [MaxLength(20)]
        [Column("color")]
        public string? Color { get; set; }

        [Column("weight_capacity")]
        public int? WeightCapacity { get; set; }

        [MaxLength(100)]
        [Column("adjustable_features")]
        public string? AdjustableFeatures { get; set; }

        [MaxLength(50)]
        [Column("dimensions")]
        public string? Dimensions { get; set; }

        [Column("warranty_years")]
        public int? WarrantyYears { get; set; }

        [Column("comfort_rating")]
        public int? ComfortRating { get; set; }

        public virtual Product Product { get; set; }
    }

    [Table("mouse_details")]
    public class MouseDetails
    {
        [Key, ForeignKey("Product")]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("dpi")]
        public int? Dpi { get; set; }

        [Column("buttons")]
        public int? Buttons { get; set; }

        [MaxLength(20)]
        [Column("connection_type")]
        public string? ConnectionType { get; set; }

        [MaxLength(50)]
        [Column("ergonomic_design")]
        public string? ErgonomicDesign { get; set; }

        [MaxLength(50)]
        [Column("sensitivity_adjustment")]
        public string? SensitivityAdjustment { get; set; }

        [MaxLength(20)]
        [Column("wireless_range")]
        public string? WirelessRange { get; set; }

        [MaxLength(100)]
        [Column("compatibility_platforms")]
        public string? CompatibilityPlatforms { get; set; }

        public virtual Product Product { get; set; }
    }
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartResponseDto
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public List<CartItemDetailDto> Items { get; set; } = new List<CartItemDetailDto>();
        public decimal TotalPrice { get; set; }
    }

    public class CartItemDetailDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public byte[] Photo { get; set; }
        public decimal Subtotal => Price * Quantity;
    }
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string TypeName { get; set; }
        public byte[] PhotoUrl { get; set; }

        public LaptopDetails? LaptopDetails { get; set; }
        public ChairDetails? ChairDetails { get; set; }
        public MouseDetails? MouseDetails { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string TypeName { get; set; }
        public byte[] PhotoUrl { get; set; }

        public LaptopDetails? LaptopDetails { get; set; }
        public ChairDetails? ChairDetails { get; set; }
        public MouseDetails? MouseDetails { get; set; }

    }
}