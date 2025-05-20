using AcerShop.ViewModel;
using Microsoft.Maui.Controls;

namespace AcerShop.View
{
	public partial class AdminProductsPage : ContentPage
	{
        public AdminProductsPage(AdminProductsViewModel viewModel)
        {
			InitializeComponent ();
			BindingContext = viewModel; 
        }
	}
}