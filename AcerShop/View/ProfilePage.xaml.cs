using AcerShop.ViewModel;
using AcerShop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;


namespace AcerShop.View
{
    [Preserve(AllMembers = true)]
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ProfilePage : ContentPage
	{
		public ProfilePage(ProfilePageViewModel viewModel )
		{
			InitializeComponent ();


            BindingContext = viewModel;
        }
	}
}