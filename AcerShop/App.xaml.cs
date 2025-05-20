using AcerShop.Model;
using AcerShop.View;

namespace AcerShop
{
    public partial class App : Application
    {
        public static User? CurrentUser { get; set; }
        public App()
        {
           InitializeComponent();
           MainPage = new AppShell();

           AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
           {
               // Запишите в логи или покажите сообщение
               System.Diagnostics.Debug.WriteLine($"Unhandled exception: {args.ExceptionObject}");
           };
        }

    }
}