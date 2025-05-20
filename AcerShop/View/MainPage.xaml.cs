using AcerShop.ViewModel;
using Microsoft.Maui.Controls;

namespace AcerShop.View
{
    public partial class MainPage : ContentPage
    {
        public MainPageViewModel ViewModel { get; }
        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is MainPageViewModel vm)
            {
                await vm.OnAppearingAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (BindingContext is MainPageViewModel vm)
            {
                vm.OnDisappearing();
            }
        }
    }

}