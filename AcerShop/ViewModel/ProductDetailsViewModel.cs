using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcerShop.Model;
using System.Diagnostics;
using AcerShop.Services;

namespace AcerShop.ViewModel
{
    [QueryProperty(nameof(Product), "SelectedProduct")]
    public partial class ProductDetailsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private Product _product;

        [ObservableProperty]
        private bool _isImageZoomed;

        [ObservableProperty]
        private ImageSource _productImageSource;

        [ObservableProperty]
        private bool _isInCart;

        public bool IsNotInCart => !IsInCart;

        public bool IsLaptopDetailsVisible => Product?.TypeName?.Equals("Laptop", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsChairDetailsVisible => Product?.TypeName?.Equals("Chair", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsMouseDetailsVisible => Product?.TypeName?.Equals("Mouse", StringComparison.OrdinalIgnoreCase) ?? false;

        public ProductDetailsViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        partial void OnProductChanged(Product value)
        {
            Debug.WriteLine($"ProductDetailsViewModel.OnProductChanged called with ProductId: {value?.ProductId}");
            if (value == null) return;

            ProductImageSource = value.PhotoSource;

            OnPropertyChanged(nameof(IsLaptopDetailsVisible));
            OnPropertyChanged(nameof(IsChairDetailsVisible));
            OnPropertyChanged(nameof(IsMouseDetailsVisible));

            Debug.WriteLine($"Product {value.ProductId} details loaded. IsInCart: {value.IsInCart}, Quantity: {value.Quantity}");
        }

        [RelayCommand]
        private async Task AddToCart()
        {
            if (Product == null || !Product.IsAddToCartEnabled || App.CurrentUser == null)
            {
                Debug.WriteLine($"AddToCart DetailPage skipped: Product null? {Product == null}, Enabled? {Product?.IsAddToCartEnabled}, User null? {App.CurrentUser == null}");
                return;
            }

            Debug.WriteLine($"Attempting to add ProductId {Product.ProductId} from Details Page.");

            try
            {
                var success = await _apiService.AddToCartAsync(App.CurrentUser.Id, Product.ProductId);
                if (success)
                {
                    Debug.WriteLine($"Successfully added ProductId {Product.ProductId} via API (from Details Page). Updating model state.");
                    Product.IsInCart = true;
                    Debug.WriteLine($"Product {Product.ProductId} IsInCart set to {Product.IsInCart}. Button UI should update.");
                }
                else
                {
                    Debug.WriteLine($"API call AddToCartAsync failed for ProductId {Product.ProductId} (from Details Page).");
                    await Shell.Current.DisplayAlert("Неудача", "Не удалось добавить товар в корзину.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding ProductId {Product.ProductId} to cart (from Details Page): {ex}");
                await Shell.Current.DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private void ToggleImageZoom() => IsImageZoomed = !IsImageZoomed;

        [RelayCommand]
        private async Task GoBackAsync() => await Shell.Current.GoToAsync("..");
    }
}