using Android.App;
using Android.Runtime;

// Для Android 12 и ниже (API <= 32)
[assembly: UsesPermission(Android.Manifest.Permission.ReadExternalStorage)]

// Для Android 13 и выше (API >= 33)
[assembly: UsesPermission(Android.Manifest.Permission.ReadMediaImages)]

namespace AcerShop;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}