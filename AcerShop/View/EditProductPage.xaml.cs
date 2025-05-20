using AcerShop.ViewModel;
using Microsoft.Maui.Controls;

namespace AcerShop.View
{
    public partial class EditProductPage : ContentPage
    {
        public EditProductPage(EditProductViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;

        }

    
    }
}