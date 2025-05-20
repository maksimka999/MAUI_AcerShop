using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcerShop.Model;
using System.Diagnostics;
using AcerShop.Services;
using Microsoft.Maui.Storage;
using System.Globalization;

namespace AcerShop.ViewModel
{
    [QueryProperty(nameof(ProductToEdit), "ProductToEdit")]
    public partial class EditProductViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private Product _originalProduct;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;
        public bool IsNotBusy => !IsBusy;

        [ObservableProperty] private string _name;
        [ObservableProperty] private string _description;
        [ObservableProperty] private string _priceString;
        [ObservableProperty] private string _quantityString;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLaptopDetailsVisible))]
        [NotifyPropertyChangedFor(nameof(IsChairDetailsVisible))]
        [NotifyPropertyChangedFor(nameof(IsMouseDetailsVisible))]
        private string _typeName;
        [ObservableProperty] private byte[] _photoBytes;
        [ObservableProperty] private ImageSource _productImageSource;

        [ObservableProperty] private string _laptopProcessor;
        [ObservableProperty] private string _laptopRam;
        [ObservableProperty] private string _laptopStorageSize;
        [ObservableProperty] private string _laptopScreenSize;
        [ObservableProperty] private string _laptopGraphicsCard;
        [ObservableProperty] private string _laptopOperatingSystem;
        [ObservableProperty] private string _laptopBatteryLife;
        [ObservableProperty] private string _laptopWeight;

        [ObservableProperty] private string _chairMaterial;
        [ObservableProperty] private string _chairColor;
        [ObservableProperty] private string _chairWeightCapacity;
        [ObservableProperty] private string _chairAdjustableFeatures;
        [ObservableProperty] private string _chairDimensions;
        [ObservableProperty] private string _chairWarrantyYears;
        [ObservableProperty] private string _chairComfortRating;

        [ObservableProperty] private string _mouseDpi;
        [ObservableProperty] private string _mouseButtons;
        [ObservableProperty] private string _mouseConnectionType;
        [ObservableProperty] private string _mouseErgonomicDesign;
        [ObservableProperty] private string _mouseSensitivityAdjustment;
        [ObservableProperty] private string _mouseWirelessRange;
        [ObservableProperty] private string _mouseCompatibilityPlatforms;

        public bool IsLaptopDetailsVisible => TypeName?.Equals("Laptop", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsChairDetailsVisible => TypeName?.Equals("Chair", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsMouseDetailsVisible => TypeName?.Equals("Mouse", StringComparison.OrdinalIgnoreCase) ?? false;

        public EditProductViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _productImageSource = "noimage.png";
        }

        [ObservableProperty]
        private Product _productToEdit;

        partial void OnProductToEditChanged(Product value)
        {
            if (value == null) return;

            _originalProduct = value;

            Name = value.Name;
            Description = value.Description;
            PriceString = value.Price.ToString(CultureInfo.InvariantCulture);
            QuantityString = value.Quantity.ToString();
            TypeName = value.TypeName;
            PhotoBytes = value.PhotoUrl;

            ProductImageSource = value.PhotoUrl != null && value.PhotoUrl.Length > 0
                ? ImageSource.FromStream(() => new MemoryStream(value.PhotoUrl))
                : "noimage.png";

            LoadDetails(value);
        }

        private void LoadDetails(Product product)
        {
            LaptopProcessor = LaptopRam = LaptopStorageSize = LaptopScreenSize = null;
            LaptopGraphicsCard = LaptopOperatingSystem = LaptopBatteryLife = LaptopWeight = null;
            ChairMaterial = ChairColor = ChairWeightCapacity = ChairAdjustableFeatures = null;
            ChairDimensions = ChairWarrantyYears = ChairComfortRating = null;
            MouseDpi = MouseButtons = MouseConnectionType = MouseErgonomicDesign = null;
            MouseSensitivityAdjustment = MouseWirelessRange = MouseCompatibilityPlatforms = null;

            if (product.LaptopDetails != null)
            {
                LaptopProcessor = product.LaptopDetails.Processor;
                LaptopRam = product.LaptopDetails.Ram;
                LaptopStorageSize = product.LaptopDetails.StorageSize;
                LaptopScreenSize = product.LaptopDetails.ScreenSize;
                LaptopGraphicsCard = product.LaptopDetails.GraphicsCard;
                LaptopOperatingSystem = product.LaptopDetails.OperatingSystem;
                LaptopBatteryLife = product.LaptopDetails.BatteryLife;
                LaptopWeight = product.LaptopDetails.Weight;
            }
            if (product.ChairDetails != null)
            {
                ChairMaterial = product.ChairDetails.Material;
                ChairColor = product.ChairDetails.Color;
                ChairWeightCapacity = int.TryParse(ChairWeightCapacity, out var weightCap) ? weightCap.ToString() : null;
                ChairAdjustableFeatures = product.ChairDetails.AdjustableFeatures;
                ChairDimensions = product.ChairDetails.Dimensions;
                ChairWarrantyYears = int.TryParse(ChairWarrantyYears, out var warranty) ? warranty.ToString() : null;
                ChairComfortRating = int.TryParse(ChairComfortRating, out var rating) ? rating.ToString() : null;
            }
            if (product.MouseDetails != null)
            {
                MouseDpi = int.TryParse(MouseDpi, out var dpi) ? dpi.ToString() : null;
                MouseButtons = int.TryParse(MouseButtons, out var buttons) ? buttons.ToString() : null;
                MouseConnectionType = product.MouseDetails.ConnectionType;
                MouseErgonomicDesign = product.MouseDetails.ErgonomicDesign;
                MouseSensitivityAdjustment = product.MouseDetails.SensitivityAdjustment;
                MouseWirelessRange = product.MouseDetails.WirelessRange;
                MouseCompatibilityPlatforms = product.MouseDetails.CompatibilityPlatforms;
            }
        }

        [RelayCommand]
        private async Task PickPhotoAsync()
        {
            if (IsBusy) return;
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions { Title = "Выберите фото товара" });
                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    PhotoBytes = memoryStream.ToArray();
                    ProductImageSource = ImageSource.FromStream(() => new MemoryStream(PhotoBytes));
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить фото: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task SaveProductAsync()
        {
            if (IsBusy || _originalProduct == null) return;

            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(PriceString) ||
                string.IsNullOrWhiteSpace(QuantityString) ||
                string.IsNullOrWhiteSpace(TypeName))
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Заполните все обязательные поля.", "OK");
                return;
            }

            if (!decimal.TryParse(PriceString, NumberStyles.Any, CultureInfo.InvariantCulture, out var priceValue) || priceValue < 0)
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Введите корректную цену.", "OK");
                return;
            }

            if (!int.TryParse(QuantityString, out var quantityValue) || quantityValue < 0)
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Введите корректное количество.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var productToUpdate = new Product
                {
                    ProductId = _originalProduct.ProductId,
                    Name = Name,
                    Description = Description,
                    Price = priceValue,
                    Quantity = quantityValue,
                    TypeName = TypeName,
                    PhotoUrl = PhotoBytes
                };

                if (IsLaptopDetailsVisible)
                {
                    productToUpdate.LaptopDetails = new LaptopDetails
                    {
                        Processor = LaptopProcessor,
                        Ram = LaptopRam,
                        StorageSize = LaptopStorageSize,
                        ScreenSize = LaptopScreenSize,
                        GraphicsCard = LaptopGraphicsCard,
                        OperatingSystem = LaptopOperatingSystem,
                        BatteryLife = LaptopBatteryLife,
                        Weight = LaptopWeight
                    };
                }
                else if (IsChairDetailsVisible)
                {
                    productToUpdate.ChairDetails = new ChairDetails
                    {
                        Material = ChairMaterial,
                        Color = ChairColor,
                        WeightCapacity = int.TryParse(ChairWeightCapacity, out var weightCap) ? weightCap : (int?)null,
                        AdjustableFeatures = ChairAdjustableFeatures,
                        Dimensions = ChairDimensions,
                        WarrantyYears = int.TryParse(ChairWarrantyYears, out var warranty) ? warranty : (int?)null,
                        ComfortRating = int.TryParse(ChairComfortRating, out var rating) ? rating : (int?)null
                    };
                }
                else if (IsMouseDetailsVisible)
                {
                    productToUpdate.MouseDetails = new MouseDetails
                    {
                        Dpi = int.TryParse(MouseDpi, out var dpi) ? dpi : (int?)null,
                        Buttons = int.TryParse(MouseButtons, out var buttons) ? buttons : (int?)null,
                        ConnectionType = MouseConnectionType,
                        ErgonomicDesign = MouseErgonomicDesign,
                        SensitivityAdjustment = MouseSensitivityAdjustment,
                        WirelessRange = MouseWirelessRange,
                        CompatibilityPlatforms = MouseCompatibilityPlatforms
                    };
                }

                bool success = await _apiService.UpdateProductAsync(productToUpdate);

                if (success)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось обновить товар.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении: {ex}");
                await Shell.Current.DisplayAlert("Ошибка", "Произошла ошибка при сохранении.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            if (IsBusy) return;
            await Shell.Current.GoToAsync("..");
        }
    }
}