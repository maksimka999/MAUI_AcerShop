using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AcerShop.Model
{
    public partial class Product : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _productId;
        public int ProductId { get => _productId; set => SetField(ref _productId, value); }

        private string _name;
        public string Name { get => _name; set => SetField(ref _name, value); }

        private string _description;
        public string Description { get => _description; set => SetField(ref _description, value); }

        private decimal _price;
        public decimal Price { get => _price; set => SetField(ref _price, value); }

        private byte[] _photoUrl;
        public byte[] PhotoUrl { get => _photoUrl; set => SetField(ref _photoUrl, value, updatePhotoSource: true); }

        private string _typeName;
        public string TypeName { get => _typeName; set => SetField(ref _typeName, value); }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetField(ref _quantity, value))
                {
                    NotifyButtonStateChanged();
                }
            }
        }

        private bool _isInCart;
        public bool IsInCart
        {
            get => _isInCart;
            set
            {
                if (SetField(ref _isInCart, value))
                {
                    NotifyButtonStateChanged();
                }
            }
        }

        private int _quantityInCart;
        public int QuantityInCart { get => _quantityInCart; set => SetField(ref _quantityInCart, value); }

        private LaptopDetails _laptopDetails;
        public LaptopDetails LaptopDetails { get => _laptopDetails; set => SetField(ref _laptopDetails, value); }
        private ChairDetails _chairDetails;
        public ChairDetails ChairDetails { get => _chairDetails; set => SetField(ref _chairDetails, value); }
        private MouseDetails _mouseDetails;
        public MouseDetails MouseDetails { get => _mouseDetails; set => SetField(ref _mouseDetails, value); }

        public string ButtonText =>
            Quantity <= 0 ? "Нет в наличии"
            : IsInCart ? "В корзине"
            : "В корзину";

        public Color ButtonBackgroundColor
        {
            get
            {
                if (!IsAddToCartEnabled)
                {
                    return Colors.Gray;
                }
                else
                {
                    if (Application.Current.RequestedTheme == AppTheme.Light)
                    {
                        return Application.Current.Resources.TryGetValue("PrimaryColorLight", out var lightColor)
                            ? (Color)lightColor
                            : Color.FromArgb("#1C375C");
                    }
                    else
                    {
                        return Application.Current.Resources.TryGetValue("PrimaryColorDark", out var darkColor)
                            ? (Color)darkColor
                            : Color.FromArgb("#008000");
                    }
                }
            }
        }

        public bool IsAddToCartEnabled => Quantity > 0 && !IsInCart;

        public ImageSource PhotoSource => GetImageSource();

        private ImageSource _cachedPhotoSource;

        private ImageSource GetImageSource()
        {
            if (_cachedPhotoSource == null && _photoUrl != null && _photoUrl.Length > 0)
            {
                _cachedPhotoSource = ImageSource.FromStream(() => new MemoryStream(_photoUrl));
            }
            return _cachedPhotoSource ?? ImageSource.FromFile("noimage.png");
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null, bool updatePhotoSource = false)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);

            if (updatePhotoSource && propertyName == nameof(PhotoUrl))
            {
                _cachedPhotoSource = null;
                OnPropertyChanged(nameof(PhotoSource));
            }
            return true;
        }

        private void NotifyButtonStateChanged()
        {
            OnPropertyChanged(nameof(ButtonText));
            OnPropertyChanged(nameof(ButtonBackgroundColor));
            OnPropertyChanged(nameof(IsAddToCartEnabled));
        }

        public void UpdateCartStatus(bool inCart)
        {
            IsInCart = inCart;
        }
    }

    public partial class ChairDetails : ObservableObject
    {
        [ObservableProperty] private int _productId;
        [ObservableProperty] private string? _material;
        [ObservableProperty] private string? _color;
        [ObservableProperty] private int? _weightCapacity;
        [ObservableProperty] private string? _adjustableFeatures;
        [ObservableProperty] private string? _dimensions;
        [ObservableProperty] private int? _warrantyYears;
        [ObservableProperty] private int? _comfortRating;
    }

    public partial class MouseDetails : ObservableObject
    {
        [ObservableProperty] private int _productId;
        [ObservableProperty] private int? _dpi;
        [ObservableProperty] private int? _buttons;
        [ObservableProperty] private string? _connectionType;
        [ObservableProperty] private string? _ergonomicDesign;
        [ObservableProperty] private string? _sensitivityAdjustment;
        [ObservableProperty] private string? _wirelessRange;
        [ObservableProperty] private string? _compatibilityPlatforms;
    }

    public partial class LaptopDetails : ObservableObject
    {
        [ObservableProperty] private int _productId;
        [ObservableProperty] private string? _processor;
        [ObservableProperty] private string? _ram;
        [ObservableProperty] private string? _storageSize;
        [ObservableProperty] private string? _screenSize;
        [ObservableProperty] private string? _graphicsCard;
        [ObservableProperty] private string? _operatingSystem;
        [ObservableProperty] private string? _batteryLife;
        [ObservableProperty] private string? _weight;
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

    public partial class CartItemDetailDto : ObservableObject
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        [ObservableProperty] private byte[] _photo;
        [ObservableProperty] private int _maxQuantity;

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                {
                    OnPropertyChanged(nameof(CanIncrease));
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }

        public ImageSource ProductImageSource =>
            Photo != null && Photo.Length > 0
                ? ImageSource.FromStream(() => new MemoryStream(Photo))
                : ImageSource.FromFile("noimage.png");

        public decimal Subtotal => Price * Quantity;
        public bool CanIncrease => Quantity < MaxQuantity;

        partial void OnPhotoChanged(byte[] value)
        {
            OnPropertyChanged(nameof(ProductImageSource));
        }

        partial void OnMaxQuantityChanged(int value)
        {
            OnPropertyChanged(nameof(CanIncrease));
        }
    }

    public class ProductDtoForApi
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string TypeName { get; set; } = "";
        public byte[]? PhotoUrl { get; set; }
        public LaptopDetails? LaptopDetails { get; set; }
        public ChairDetails? ChairDetails { get; set; }
        public MouseDetails? MouseDetails { get; set; }
    }
}