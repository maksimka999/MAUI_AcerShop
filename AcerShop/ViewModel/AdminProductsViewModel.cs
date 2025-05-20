using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcerShop.Model;
using AcerShop.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using AcerShop.View;
using System.Linq;
using System;

namespace AcerShop.ViewModel
{
    public partial class AdminProductsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private List<Product> _allProducts = new();

        [ObservableProperty]
        private ObservableCollection<Product> _products = new();

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _searchText;

        public AdminProductsViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadProducts();
        }

        partial void OnSearchTextChanged(string value) => FilterProducts();

        private void FilterProducts()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Products = new ObservableCollection<Product>(_allProducts);
            }
            else
            {
                var searchQuery = SearchText.ToLower();
                Products = new ObservableCollection<Product>(_allProducts.Where(p =>
                    p.Name?.ToLower().Contains(searchQuery) == true ||
                    p.Description?.ToLower().Contains(searchQuery) == true ||
                    p.TypeName?.ToLower().Contains(searchQuery) == true ||
                    p.Price.ToString().Contains(searchQuery)));
            }
        }

        [RelayCommand]
        private async Task LoadProducts()
        {
            try
            {
                IsRefreshing = true;
                var products = await _apiService.GetProductsAsync();
                _allProducts = products?.ToList() ?? new List<Product>();
                FilterProducts();

                Debug.WriteLine($"Loaded {_allProducts.Count} products");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading products: {ex}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось загрузить товары", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task Refresh() => await LoadProducts();

        [RelayCommand]
        private async Task GoToEditProduct(Product product)
        {
            if (product == null) return;

            try
            {
                await Shell.Current.GoToAsync($"{nameof(EditProductPage)}", new Dictionary<string, object>
                {
                    { "ProductToEdit", product }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to edit page: {ex}");
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось перейти к редактированию", "OK");
            }
        }

        [RelayCommand]
        private async Task DeleteProduct(Product product)
        {
            if (product == null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Удаление товара",
                $"Удалить товар \"{product.Name}\"?",
                "Удалить",
                "Отмена");

            if (!confirm) return;

            try
            {
                bool success = await _apiService.DeleteProductAsync(product.ProductId);
                if (success)
                {
                    _allProducts.Remove(product);
                    FilterProducts();
                    await Shell.Current.DisplayAlert("Успех", "Товар удален", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось удалить товар", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting product: {ex}");
                await Shell.Current.DisplayAlert("Ошибка", "Ошибка при удалении", "OK");
            }
        }

        [RelayCommand]
        private async Task AddProduct()
        {
            await Shell.Current.GoToAsync(nameof(AddProductPage));
        }
    }
}