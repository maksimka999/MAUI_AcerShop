// AcerShop.ViewModel/MainPageViewModel.cs

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AcerShop.Services;
using AcerShop.Model;
using Microsoft.Maui.Controls;
using System;
using AcerShop.View;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using CommunityToolkit.Mvvm.Messaging;
using AcerShop.Messages;

namespace AcerShop.ViewModel
{
    public partial class MainPageViewModel : ObservableObject, IRecipient<ProductsChangedMessage>
    {
        private readonly ApiService _apiService;
        private readonly Dictionary<string, string> _productTypeMapping = new()
        {
            { "Стулья", "Chair" },
            { "Ноутбуки", "Laptop" },
            { "Мышки", "Mouse" }
        };
        private List<Product> _allProducts = new();
        private CancellationTokenSource _filterCts = new CancellationTokenSource();
        private CancellationTokenSource _searchDebounceCts = new CancellationTokenSource();

        [ObservableProperty]
        private ObservableCollection<Product> _products = new();

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private bool _shouldRefreshOnAppearing;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _loadingMessage = "Загрузка товаров...";

        [ObservableProperty]
        private bool _isFilterMenuVisible;

        [ObservableProperty]
        private ObservableCollection<string> _availableProductTypes;

        [ObservableProperty]
        private string _selectedProductType;

        [ObservableProperty]
        private string _minPriceText;

        [ObservableProperty]
        private string _maxPriceText;

        public MainPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            InitializeProductTypes();
            _ = LoadInitialDataAsync();
        }

        private void InitializeProductTypes()
        {
            AvailableProductTypes = new ObservableCollection<string> { "Все типы" };
            foreach (var type in _productTypeMapping.Keys.OrderBy(k => k))
                AvailableProductTypes.Add(type);
            SelectedProductType = AvailableProductTypes.First();
        }

        public async Task OnAppearingAsync()
        {
            Debug.WriteLine("MainPageViewModel OnAppearingAsync: Registering messenger.");
            WeakReferenceMessenger.Default.Register<ProductsChangedMessage>(this);

            if (ShouldRefreshOnAppearing)
            {
                await ReloadProductsAsync();
                ShouldRefreshOnAppearing = false;
            }
        }

        public void OnDisappearing()
        {
            Debug.WriteLine("MainPageViewModel OnDisappearing: Unregistering messenger.");
            WeakReferenceMessenger.Default.Unregister<ProductsChangedMessage>(this);
            _filterCts?.Cancel();
            _searchDebounceCts?.Cancel();
        }

        public void Receive(ProductsChangedMessage message)
        {
            Debug.WriteLine("ProductsChangedMessage received in MainPageViewModel. Triggering reload.");
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await ReloadProductsAsync();
            });
        }

        partial void OnSearchTextChanged(string oldValue, string newValue)
        {
            Debug.WriteLine($"SearchTextChanged: {newValue}. Debouncing filter application.");
            DebounceApplyFilters();
        }

        private void DebounceApplyFilters()
        {
            _searchDebounceCts.Cancel();
            _searchDebounceCts = new CancellationTokenSource();
            var token = _searchDebounceCts.Token;

            Task.Delay(300, token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    Debug.WriteLine("Debounce timer elapsed. Applying filters due to SearchText change.");
                    ApplyFiltersAsync();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        [RelayCommand]
        private async Task GoToHome()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        [RelayCommand]
        private async Task ReloadProductsAsync()
        {
            Debug.WriteLine("ReloadProductsAsync triggered.");
            await LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            IsLoading = true;
            LoadingMessage = "Обновление товаров...";
            try
            {
                Debug.WriteLine("Loading products from API...");
                await FetchProductsFromApiAsync();
                Debug.WriteLine("Syncing cart state...");
                await SyncCartStateAsync();
                Debug.WriteLine("Applying filters after initial load/reload...");
                await ApplyFiltersAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex}");
                _allProducts = new List<Product>();
                MainThread.BeginInvokeOnMainThread(() => Products.Clear());
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Не удалось обновить список товаров", "OK");
            }
            finally
            {
                Debug.WriteLine("Finished loading/reloading products.");
                IsLoading = false;
                LoadingMessage = string.Empty;
            }
        }

        private async Task FetchProductsFromApiAsync()
        {
            var productsFromApi = await _apiService.GetProductsAsync().ConfigureAwait(false);
            _allProducts = productsFromApi?.ToList() ?? new List<Product>();
            Debug.WriteLine($"Fetched {_allProducts.Count} products from API.");
        }

        private async Task SyncCartStateAsync()
        {
            if (App.CurrentUser == null || !_allProducts.Any())
            {
                Debug.WriteLine("Skipping cart state sync: User not logged in or no products.");
                return;
            }
            Debug.WriteLine($"Syncing cart state for UserId {App.CurrentUser.Id}...");
            try
            {
                var cart = await _apiService.GetCartAsync(App.CurrentUser.Id).ConfigureAwait(false);
                var cartProductIds = cart?.Items.Select(i => i.ProductId).ToHashSet() ?? new HashSet<int>();
                Debug.WriteLine($"Cart loaded for sync. Found {cartProductIds.Count} item IDs in cart.");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var product in _allProducts)
                    {
                        bool isInCart = cartProductIds.Contains(product.ProductId);
                        if (product.IsInCart != isInCart)
                        {
                            product.IsInCart = isInCart;
                        }
                    }
                    Debug.WriteLine("Cart state synchronized for all products on MainThread.");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error syncing cart state: {ex}");
            }
        }

        private async Task ApplyFiltersAsync()
        {
            _filterCts.Cancel();
            _filterCts = new CancellationTokenSource();
            var token = _filterCts.Token;

            if (_allProducts == null)
            {
                Debug.WriteLine("Cannot apply filters: _allProducts is null.");
                MainThread.BeginInvokeOnMainThread(() => Products.Clear());
                return;
            }

            Debug.WriteLine($"ApplyFiltersAsync called. Search: '{SearchText}', Type: '{SelectedProductType}', Min: '{MinPriceText}', Max: '{MaxPriceText}'");
            // IsLoading = true; // Optional: Show loading indicator during filtering
            // LoadingMessage = "Фильтрация товаров...";

            try
            {
                List<Product> filteredProducts = await Task.Run(() =>
                {
                    if (token.IsCancellationRequested) return new List<Product>();

                    IEnumerable<Product> filtered = _allProducts;

                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        var searchQuery = SearchText.ToLowerInvariant();
                        filtered = filtered.Where(p =>
                            p.Name?.ToLowerInvariant().Contains(searchQuery) == true ||
                            p.Description?.ToLowerInvariant().Contains(searchQuery) == true);
                    }
                    if (token.IsCancellationRequested) return new List<Product>();

                    if (!string.IsNullOrWhiteSpace(SelectedProductType) && SelectedProductType != "Все типы")
                    {
                        if (_productTypeMapping.TryGetValue(SelectedProductType, out var englishType))
                        {
                            filtered = filtered.Where(p => p.TypeName == englishType);
                        }
                    }
                    if (token.IsCancellationRequested) return new List<Product>();

                    if (decimal.TryParse(MinPriceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var minPrice) && minPrice > 0)
                        filtered = filtered.Where(p => p.Price >= minPrice);
                    if (token.IsCancellationRequested) return new List<Product>();

                    if (decimal.TryParse(MaxPriceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var maxPrice) && maxPrice > 0)
                        filtered = filtered.Where(p => p.Price <= maxPrice);

                    return token.IsCancellationRequested ? new List<Product>() : filtered.OrderBy(p => p.Name).ToList();

                }, token);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine("Filtering task was cancelled.");
                        return;
                    }
                    Products.Clear();
                    if (filteredProducts != null)
                    {
                        foreach (var product in filteredProducts)
                        {
                            Products.Add(product);
                        }
                    }
                    Debug.WriteLine($"UI updated with {Products.Count} products after filtering.");
                });
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Filtering task was cancelled via OperationCanceledException.");
                MainThread.BeginInvokeOnMainThread(() => Products.Clear()); // Clear if ongoing filter was cancelled
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during filtering task: {ex}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Products.Clear();
                    Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Не удалось применить фильтры.", "OK");
                });
            }
            finally
            {
                // IsLoading = false;
                // LoadingMessage = string.Empty;
            }
        }


        [RelayCommand]
        private void ToggleFilterMenu() => IsFilterMenuVisible = !IsFilterMenuVisible;

        [RelayCommand]
        private async Task ApplyFiltersFromMenu()
        {
            IsFilterMenuVisible = false;
            Debug.WriteLine("ApplyFiltersFromMenuCommand called. Applying all filters.");
            _searchDebounceCts.Cancel(); // Cancel any pending search debounce
            await ApplyFiltersAsync();
        }

        [RelayCommand]
        private async Task ResetFiltersAndApply()
        {
            SearchText = string.Empty; // This will trigger OnSearchTextChanged, then DebounceApplyFilters
            SelectedProductType = AvailableProductTypes.First();
            MinPriceText = string.Empty;
            MaxPriceText = string.Empty;
            IsFilterMenuVisible = false;
            Debug.WriteLine("ResetFiltersAndApplyCommand called. Filters reset, applying all filters.");
            _searchDebounceCts.Cancel(); // Cancel any pending search debounce
            await ApplyFiltersAsync(); // Explicitly apply all filters after reset
        }

        [RelayCommand]
        private async Task ViewProductDetails(Product product)
        {
            if (product == null) return;
            try
            {
                await Shell.Current.GoToAsync($"{nameof(ProductDetailsPage)}", new Dictionary<string, object>
                {
                    { "SelectedProduct", product }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to product details: {ex}");
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Не удалось открыть детали товара", "OK");
            }
        }

        [RelayCommand]
        private async Task GoToCart()
        {
            if (App.CurrentUser == null)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Необходимо войти в систему", "OK");
                return;
            }
            await Shell.Current.GoToAsync(nameof(CartPage));
        }

        [RelayCommand]
        private async Task GoToProfile()
        {
            if (App.CurrentUser == null)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Необходимо войти в систему", "OK");
                return;
            }
            await Shell.Current.GoToAsync(nameof(ProfilePage));
        }

        [RelayCommand]
        private async Task AddToCart(Product product)
        {
            if (product == null || !product.IsAddToCartEnabled)
            {
                Debug.WriteLine($"AddToCart skipped: Product is null or not enabled (Quantity: {product?.Quantity}, IsInCart: {product?.IsInCart})");
                return;
            }
            if (App.CurrentUser == null)
            {
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Необходимо войти в систему", "OK");
                return;
            }
            Debug.WriteLine($"Attempting to add ProductId {product.ProductId} to cart for UserId {App.CurrentUser.Id}");
            try
            {
                bool success = await _apiService.AddToCartAsync(App.CurrentUser.Id, product.ProductId);
                if (success)
                {
                    Debug.WriteLine($"Successfully added ProductId {product.ProductId} via API. Updating local state.");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        product.IsInCart = true;
                        Debug.WriteLine($"Product {product.ProductId} IsInCart updated to {product.IsInCart}. UI should update.");
                    });
                }
                else
                {
                    Debug.WriteLine($"API call AddToCartAsync failed for ProductId {product.ProductId}.");
                    await Shell.Current.CurrentPage.DisplayAlert("Ошибка", "Не удалось добавить товар в корзину (ответ API: неудача).", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding ProductId {product.ProductId} to cart: {ex}");
                await Shell.Current.CurrentPage.DisplayAlert("Ошибка", $"Произошла ошибка при добавлении в корзину: {ex.Message}", "OK");
            }
        }
    }
}