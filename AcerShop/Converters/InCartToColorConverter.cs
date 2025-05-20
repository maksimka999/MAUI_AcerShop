using System;
using System.Globalization;
using AcerShop.Model;
using Microsoft.Maui.Controls;

namespace AcerShop.Converters
{
    public class InCartToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Product product)
            {
                if (product.Quantity == 0 || product.IsInCart)
                {
                    return Colors.Gray;
                }
                else
                {
                    if (Application.Current.RequestedTheme == AppTheme.Light)
                    {
                        return Application.Current.Resources.TryGetValue("PrimaryColorLight", out var lightColor)
                            ? (Color)lightColor
                            : Color.FromArgb("#1C375C"); 
                    }
                    else
                    {
                        return Application.Current.Resources.TryGetValue("PrimaryColorDark", out var darkColor)
                            ? (Color)darkColor
                            : Color.FromArgb("#008000");
                    }
                }
            }
            return Colors.Gray; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}