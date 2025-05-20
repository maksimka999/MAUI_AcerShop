using AcerShop.ViewModel;

namespace AcerShop.View
{
    public partial class ProductDetailsPage : ContentPage
    {
        public ProductDetailsPage(ProductDetailsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is ProductDetailsViewModel vm && vm.Product != null)
            {
                Content.Opacity = 0;
                Content.FadeTo(1, 300, Easing.SinIn);
            }
        }
    }
}