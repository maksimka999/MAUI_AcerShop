using AcerShop.ViewModel;
using Microsoft.Maui.Controls;

namespace AcerShop.View
{
	public partial class AddProductPage : ContentPage
	{
        public AddProductPage(AddProductViewModel viewModel)
        {
			InitializeComponent ();
			BindingContext = viewModel; 
        }
	}
}