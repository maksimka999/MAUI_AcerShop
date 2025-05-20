// CartPage.xaml.cs
using AcerShop.Model;
using AcerShop.ViewModel;

namespace AcerShop.View
{
    public partial class CartPage : ContentPage
    {
        public CartPage(CartPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is CartPageViewModel vm)
            {
                vm.CurrentUser = App.CurrentUser;
                vm.LoadCartCommand.Execute(null);
            }
        }

        private void OnQuantityChanged(object sender, TextChangedEventArgs e)
        {
            // Можно добавить валидацию ввода
        }
    }
}