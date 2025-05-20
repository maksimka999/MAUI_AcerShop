using AcerShop.Model;
using AcerShop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Media;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.Messaging;
using AcerShop.Messages;

namespace AcerShop.ViewModel
{
    public partial class AddProductViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string _title = "Добавить товар";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        [ObservableProperty]
        private string _name = "";

        [ObservableProperty]
        private string? _description;

        [ObservableProperty]
        private string _priceString = "";

        [ObservableProperty]
        private string _quantityString = "";

        [ObservableProperty]
        private byte[]? _photoBytes;

        [ObservableProperty]
        private ImageSource? _productImageSource;

        public ObservableCollection<string> ProductTypes { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddProductCommand))]
        private string? _selectedProductType;

        [ObservableProperty] private bool _isLaptopDetailsVisible;
        [ObservableProperty] private bool _isChairDetailsVisible;
        [ObservableProperty] private bool _isMouseDetailsVisible;

        [ObservableProperty] private string? _laptopProcessor;
        [ObservableProperty] private string? _laptopRam;
        [ObservableProperty] private string? _laptopStorageSize;
        [ObservableProperty] private string? _laptopScreenSize;
        [ObservableProperty] private string? _laptopGraphicsCard;
        [ObservableProperty] private string? _laptopOperatingSystem;
        [ObservableProperty] private string? _laptopBatteryLife;
        [ObservableProperty] private string? _laptopWeight;

        [ObservableProperty] private string? _chairMaterial;
        [ObservableProperty] private string? _chairColor;
        [ObservableProperty] private string? _chairWeightCapacity;
        [ObservableProperty] private string? _chairAdjustableFeatures;
        [ObservableProperty] private string? _chairDimensions;
        [ObservableProperty] private string? _chairWarrantyYears;
        [ObservableProperty] private string? _chairComfortRating;

        [ObservableProperty] private string? _mouseDpi;
        [ObservableProperty] private string? _mouseButtons;
        [ObservableProperty] private string? _mouseConnectionType;
        [ObservableProperty] private string? _mouseErgonomicDesign;
        [ObservableProperty] private string? _mouseSensitivityAdjustment;
        [ObservableProperty] private string? _mouseWirelessRange;
        [ObservableProperty] private string? _mouseCompatibilityPlatforms;


        public bool IsNotBusy => !IsBusy;

        public AddProductViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadProductTypes();
            UpdateDetailsVisibility();
        }

        private void LoadProductTypes()
        {
            ProductTypes.Add("Laptop");
            ProductTypes.Add("Chair");
            ProductTypes.Add("Mouse");
        }

        partial void OnSelectedProductTypeChanged(string? value)
        {
            UpdateDetailsVisibility();
            ClearDetailsFields();
        }

        private void UpdateDetailsVisibility()
        {
            IsLaptopDetailsVisible = SelectedProductType == "Laptop";
            IsChairDetailsVisible = SelectedProductType == "Chair";
            IsMouseDetailsVisible = SelectedProductType == "Mouse";
        }

        private void ClearDetailsFields()
        {
            LaptopProcessor = null;
            LaptopRam = null;
            LaptopStorageSize = null;
            LaptopScreenSize = null;
            LaptopGraphicsCard = null;
            LaptopOperatingSystem = null;
            LaptopBatteryLife = null;
            LaptopWeight = null;

            ChairMaterial = null;
            ChairColor = null;
            ChairWeightCapacity = null;
            ChairAdjustableFeatures = null;
            ChairDimensions = null;
            ChairWarrantyYears = null;
            ChairComfortRating = null;

            MouseDpi = null;
            MouseButtons = null;
            MouseConnectionType = null;
            MouseErgonomicDesign = null;
            MouseSensitivityAdjustment = null;
            MouseWirelessRange = null;
            MouseCompatibilityPlatforms = null;
        }

        [RelayCommand]
        private async Task PickPhotoAsync()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Выберите фото товара"
                });

                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    PhotoBytes = memoryStream.ToArray();
                    ProductImageSource = ImageSource.FromStream(() => new MemoryStream(PhotoBytes));
                }
                else
                {
                    PhotoBytes = null;
                    ProductImageSource = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error picking photo: {ex.Message}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось выбрать фото.", "OK");
            }
        }

        [RelayCommand]
        private async Task AddProductAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Название' обязательно для заполнения.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(PriceString))
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Цена' обязательно для заполнения.", "OK");
                return;
            }
            if (!decimal.TryParse(PriceString, NumberStyles.Any, CultureInfo.InvariantCulture, out var priceValue) || priceValue < 0)
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Введите корректную положительную цену.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(QuantityString))
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Количество' обязательно для заполнения.", "OK");
                return;
            }
            if (!int.TryParse(QuantityString, out var quantityValue) || quantityValue < 0)
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Введите корректное положительное количество.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(_selectedProductType))
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Выберите тип товара.", "OK");
                return;
            }
            if (PhotoBytes == null || PhotoBytes.Length == 0)
            {
                await Shell.Current.DisplayAlert("Ошибка валидации", "Необходимо выбрать фотографию для товара.", "OK");
                return;
            }

            if (IsLaptopDetailsVisible)
            {
                if (string.IsNullOrWhiteSpace(LaptopProcessor)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Процессор' обязательно для заполнения (ноутбук).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(LaptopRam)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'ОЗУ' обязательно для заполнения (ноутбук).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(LaptopStorageSize)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Накопитель' обязательно для заполнения (ноутбук).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(LaptopScreenSize)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Экран' обязательно для заполнения (ноутбук).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(LaptopGraphicsCard)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Видеокарта' обязательно для заполнения (ноутбук).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(LaptopOperatingSystem)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Операционная система' обязательно для заполнения (ноутбук).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(LaptopBatteryLife)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Время работы батареи' обязательно для заполнения (ноутбук).", "OK"); return; }
                if (!int.TryParse(LaptopBatteryLife, out var laptopBatteryLifeValue) || laptopBatteryLifeValue < 0) { await Shell.Current.DisplayAlert("Ошибка валидации", "Введите корректное время работы батареи (число).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(LaptopWeight)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Вес' обязательно для заполнения (ноутбук).", "OK"); return; }
                if (!double.TryParse(LaptopWeight, NumberStyles.Any, CultureInfo.InvariantCulture, out var laptopWeightValue) || laptopWeightValue < 0) { await Shell.Current.DisplayAlert("Ошибка валидации", "Введите корректный вес (число).", "OK"); return; }

            }
            else if (IsChairDetailsVisible)
            {
                if (string.IsNullOrWhiteSpace(ChairMaterial)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Материал' обязательно для заполнения (стул).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(ChairColor)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Цвет' обязательно для заполнения (стул).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(ChairWeightCapacity)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Грузоподъемность' обязательно для заполнения (стул).", "OK"); return; }
                if (!int.TryParse(ChairWeightCapacity, out var weightCap) || weightCap < 0) { await Shell.Current.DisplayAlert("Ошибка валидации", "Введите корректную грузоподъемность стула (число).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(ChairAdjustableFeatures)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Регулируемые функции' обязательно для заполнения (стул).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(ChairDimensions)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Размеры' обязательно для заполнения (стул).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(ChairWarrantyYears)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Срок гарантии' обязательно для заполнения (стул).", "OK"); return; }
                if (!int.TryParse(ChairWarrantyYears, out var warranty) || warranty < 0) { await Shell.Current.DisplayAlert("Ошибка валидации", "Введите корректный срок гарантии стула (число).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(ChairComfortRating)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Рейтинг комфорта' обязательно для заполнения (стул).", "OK"); return; }
                if (!int.TryParse(ChairComfortRating, out var rating) || rating < 0 || rating > 10) { await Shell.Current.DisplayAlert("Ошибка валидации", "Введите корректный рейтинг комфорта стула (число от 0 до 10).", "OK"); return; }

            }
            else if (IsMouseDetailsVisible)
            {
                if (string.IsNullOrWhiteSpace(MouseDpi)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'DPI' обязательно для заполнения (мышь).", "OK"); return; }
                if (!int.TryParse(MouseDpi, out var dpi) || dpi < 0) { await Shell.Current.DisplayAlert("Ошибка валидации", "Введите корректное значение DPI мыши (число).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(MouseButtons)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Кнопки' обязательно для заполнения (мышь).", "OK"); return; }
                if (!int.TryParse(MouseButtons, out var buttons) || buttons < 0) { await Shell.Current.DisplayAlert("Ошибка валидации", "Введите корректное количество кнопок мыши (число).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(MouseConnectionType)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Тип подключения' обязательно для заполнения (мышь).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(MouseErgonomicDesign)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Эргономичный дизайн' обязательно для заполнения (мышь).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(MouseSensitivityAdjustment)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Настройка чувствительности' обязательно для заполнения (мышь).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(MouseWirelessRange)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Диапазон беспроводной связи' обязательно для заполнения (мышь).", "OK"); return; }
                if (string.IsNullOrWhiteSpace(MouseCompatibilityPlatforms)) { await Shell.Current.DisplayAlert("Ошибка валидации", "Поле 'Платформы совместимости' обязательно для заполнения (мышь).", "OK"); return; }
            }


            IsBusy = true;
            try
            {
                var productToAdd = new Product
                {
                    ProductId = 0,
                    Name = Name,
                    Description = Description,
                    Price = priceValue,
                    Quantity = quantityValue,
                    TypeName = _selectedProductType,
                    PhotoUrl = PhotoBytes
                };

                if (IsLaptopDetailsVisible)
                {

                    productToAdd.LaptopDetails = new LaptopDetails
                    {
                        Processor = LaptopProcessor,
                        Ram = LaptopRam,
                        StorageSize = LaptopStorageSize,
                        ScreenSize = LaptopScreenSize,
                        GraphicsCard = LaptopGraphicsCard,
                        OperatingSystem = LaptopOperatingSystem,
                        BatteryLife = LaptopBatteryLife,
                        Weight = LaptopWeight,
                    };
                }
                else if (IsChairDetailsVisible)
                {
                    int weightCap = int.Parse(ChairWeightCapacity!); 
                    int warranty = int.Parse(ChairWarrantyYears!); 
                    int rating = int.Parse(ChairComfortRating!); 

                    productToAdd.ChairDetails = new ChairDetails
                    {
                        Material = ChairMaterial,
                        Color = ChairColor,
                        WeightCapacity = weightCap,
                        AdjustableFeatures = ChairAdjustableFeatures,
                        Dimensions = ChairDimensions,
                        WarrantyYears = warranty,
                        ComfortRating = rating,
                    };
                }
                else if (IsMouseDetailsVisible)
                {
                    int dpi = int.Parse(MouseDpi!); 
                    int buttons = int.Parse(MouseButtons!); 

                    productToAdd.MouseDetails = new MouseDetails
                    {
                        Dpi = dpi,
                        Buttons = buttons,
                        ConnectionType = MouseConnectionType,
                        ErgonomicDesign = MouseErgonomicDesign,
                        SensitivityAdjustment = MouseSensitivityAdjustment,
                        WirelessRange = MouseWirelessRange,
                        CompatibilityPlatforms = MouseCompatibilityPlatforms,
                    };
                }

                bool success = await _apiService.AddProductAsync(productToAdd);

                if (success)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось добавить товар. Проверьте введенные данные и соединение.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении: {ex}");
                await Shell.Current.DisplayAlert("Ошибка", $"Произошла ошибка при сохранении: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}