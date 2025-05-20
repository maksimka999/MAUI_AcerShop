using AcerShop.Messages;
using AcerShop.Model;
using AcerShop.Services;
using AcerShop.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace AcerShop.ViewModel
{
    public partial class CartPageViewModel : ObservableObject
    {
        private readonly ApiService _apiService;


        [ObservableProperty]
        private ObservableCollection<CartItemDetailDto> _cartItems = new();

        [ObservableProperty]
        private decimal _totalPrice;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private User _currentUser;

        public CartPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        private async Task LoadCart()
        {
            if (CurrentUser == null) return;

            IsLoading = true;

            try
            {
                var cart = await _apiService.GetCartAsync(CurrentUser.Id);
                if (cart != null)
                {
                    foreach (var item in cart.Items)
                    {
                        var product = await _apiService.GetProductByIdAsync(item.ProductId);
                        item.MaxQuantity = product?.Quantity ?? item.Quantity;
                    }

                    CartItems = new ObservableCollection<CartItemDetailDto>(cart.Items);
                    CalculateTotal();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task IncreaseQuantity(CartItemDetailDto item)
        {

            if (item == null || !item.CanIncrease) return;

            item.Quantity++;

            await UpdateCartItem(item);
        }
        [RelayCommand]
        private async Task DecreaseQuantity(CartItemDetailDto item)
        {
            if (item == null) return;

            item.Quantity--;

            if (item.Quantity <= 0)
            {
                await RemoveItem(item);
                
                return;
            }

            await UpdateCartItem(item);
        }

        private async Task UpdateCartItem(CartItemDetailDto item)
        {
            var success = await _apiService.UpdateCartItemAsync(CurrentUser.Id, item.ProductId, item.Quantity);
            if (success)
            {
                CalculateTotal();
            }
            
        }
        [RelayCommand]
        private async Task RemoveItem(CartItemDetailDto item)
        {
            if (CurrentUser == null || item == null) return;

            await _apiService.RemoveFromCartAsync(CurrentUser.Id, item.ProductId);
            CartItems.Remove(item);
            CalculateTotal();
        }

        [RelayCommand]
        private async Task ClearCart()
        {
            if (CurrentUser == null) return;

            await _apiService.ClearCartAsync(CurrentUser.Id);
            CartItems.Clear();
            TotalPrice = 0;
        }

        [RelayCommand]
        private async Task Checkout()
        {
            if (CurrentUser == null) return;

            try
            {
                // Отправляем заказ
                var success = await _apiService.CheckoutAsync(CurrentUser.Id);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Успех", "Заказ оформлен!", "OK");
                    WeakReferenceMessenger.Default.Send(new ProductsChangedMessage());
                    await ClearCart();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        private void CalculateTotal()
        {
            TotalPrice = CartItems.Sum(item => item.Subtotal);
        }

        [RelayCommand]
        private async Task GoToHome()
        {
            WeakReferenceMessenger.Default.Send(new ProductsChangedMessage());

            // Устанавливаем флаг обновления
            var mainVm = App.Current.Handler.MauiContext.Services.GetService<MainPageViewModel>();
            if (mainVm != null)
            {
                mainVm.ShouldRefreshOnAppearing = true;
            }

            await Shell.Current.GoToAsync("//MainPage");
        }

        [RelayCommand]
        private async Task GoToProfile()
        {
            // Предполагается, что роут ProfilePage настроен в AppShell
            await Shell.Current.GoToAsync(nameof(ProfilePage));
        }
    }
}