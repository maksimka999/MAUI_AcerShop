using AcerShop.ViewModel;
using AcerShop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using System.Diagnostics;


namespace AcerShop.View
{

	public partial class RegisterPage : ContentPage
	{
        public RegisterPage(RegisterViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel; // Важно!
            Debug.WriteLine("RegisterPage initialized!");
        }
    }
}