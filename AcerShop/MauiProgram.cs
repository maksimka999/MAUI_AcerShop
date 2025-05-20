using AcerShop.ViewModel;
using AcerShop.View;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using AcerShop.Services;
using Microsoft.Extensions.Logging;
using UraniumUI;
using AcerShop.Services;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.Maui.LifecycleEvents;

#if ANDROID
using Android.OS;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Content.Res;
using AndroidX.AppCompat.Widget;
using Android.Views;
using Android.Graphics;
#endif

namespace AcerShop;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("MaterialIcons-Regular.ttf", "Material");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("FontAwesome.ttf", "FontAwesome");
                fonts.AddFontAwesomeIconFonts();
            })
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .UseMauiCommunityToolkit();

        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<PasswordValidationService>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginPageViewModel>();

        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<ProfilePageViewModel>();

        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton<MainPageViewModel>();

        builder.Services.AddTransient<EditProductPage>();
        builder.Services.AddTransient<EditProductViewModel>();

        builder.Services.AddTransient<AdminProductsPage>();
        builder.Services.AddTransient<AdminProductsViewModel>();

        builder.Services.AddTransient<ProductDetailsPage>();
        builder.Services.AddTransient<ProductDetailsViewModel>();

        builder.Services.AddTransient<CartPage>();
        builder.Services.AddTransient<CartPageViewModel>();

        builder.Services.AddTransient<AddProductPage>();
        builder.Services.AddTransient<AddProductViewModel>();
   
        Microsoft.Maui.Handlers.SearchBarHandler.Mapper.AppendToMapping("RemoveUnderline", (handler, view) =>
        {
#if ANDROID
            if (handler.PlatformView is AndroidX.AppCompat.Widget.SearchView searchView)
            {
                int editTextId = searchView.Context.Resources.GetIdentifier("search_src_text", "id", searchView.Context.PackageName);
                if (editTextId != 0)
                {
                    var editText = searchView.FindViewById<EditText>(editTextId);
                    if (editText != null)
                    {
                        editText.Background = null;
                    }
                }

                int plateId = searchView.Context.Resources.GetIdentifier("search_plate", "id", searchView.Context.PackageName);
                if (plateId != 0)
                {
                    var plateView = searchView.FindViewById<Android.Views.View>(plateId);
                    if (plateView != null)
                    {
                        plateView.Background = null;
                    }
                }
            }
#endif
        });

#if ANDROID
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddAndroid(android => android
                .OnCreate((activity, bundle) => 
                {
                    // Apply fullscreen flags
                    activity.Window?.SetFlags(
                        WindowManagerFlags.LayoutNoLimits, 
                        WindowManagerFlags.LayoutNoLimits);

                    // Apply immersive mode
                    SetFullscreenMode(activity);
                })
                .OnResume(activity => 
                {
                    // Reapply when returning to app
                    SetFullscreenMode(activity);
                }));
        });

  static void SetFullscreenMode(Android.App.Activity activity)
{
    if (activity.Window == null) return;

    if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
    {
        var decorView = activity.Window.DecorView;
        if (decorView != null)
        {
            var insetsController = decorView.WindowInsetsController;
            if (insetsController != null)
            {
                insetsController.Hide(WindowInsets.Type.SystemBars());
                insetsController.SystemBarsBehavior = (int)Android.Views.WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
            }
        }
    }
    else
    {
        var decorView = activity.Window.DecorView;
        if (decorView != null)
        {
            var uiFlags = SystemUiFlags.LayoutStable |
                         SystemUiFlags.LayoutHideNavigation |
                         SystemUiFlags.LayoutFullscreen |
                         SystemUiFlags.HideNavigation |
                         SystemUiFlags.Fullscreen |
                         SystemUiFlags.ImmersiveSticky;

            decorView.SystemUiVisibility = (StatusBarVisibility)uiFlags;
        }
    }
}



#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}