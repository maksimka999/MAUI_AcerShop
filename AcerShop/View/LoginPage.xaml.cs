using AcerShop.ViewModel;
using Microsoft.Maui.Controls;

namespace AcerShop.View
{
	public partial class LoginPage : ContentPage
	{
        public LoginPage(LoginPageViewModel viewModel)
        {
			InitializeComponent ();
            BindingContext = viewModel;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

#if ANDROID
    if (DeviceInfo.Current.Platform == DevicePlatform.Android)
    {
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        activity?.Window?.SetDecorFitsSystemWindows(false);
    }
#endif
        }
    }
}