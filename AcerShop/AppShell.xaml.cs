using AcerShop.View;
using AcerShop.ViewModel;
using Microsoft.Maui.Controls;

namespace AcerShop
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
            Navigating += OnShellNavigating;
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
            Routing.RegisterRoute(nameof(AdminProductsPage), typeof(AdminProductsPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(EditProductPage), typeof(EditProductPage));
            Routing.RegisterRoute(nameof(ProductDetailsPage), typeof(ProductDetailsPage));
            Routing.RegisterRoute(nameof(CartPage), typeof(CartPage));
            Routing.RegisterRoute(nameof(AddProductPage), typeof(AddProductPage));
        }

        private void OnShellNavigating(object sender, ShellNavigatingEventArgs e)
        {
            if (e.Target.Location.OriginalString == "//MainPage")
            {
                // Проверяем, откуда идёт переход (Profile или Cart)
                if (e.Source == ShellNavigationSource.Pop ||
                    e.Source == ShellNavigationSource.ShellItemChanged)
                {
                    // Получаем текущую страницу перед переходом
                    var previousPage = Current?.CurrentPage;

                    if (previousPage is ProfilePage || previousPage is CartPage)
                    {
                        var mainVm = Handler?.MauiContext?.Services.GetService<MainPageViewModel>();
                        if (mainVm != null)
                        {
                            mainVm.ShouldRefreshOnAppearing = true;
                        }
                    }
                }
            }
        }
    }
}