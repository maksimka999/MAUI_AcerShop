using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using AcerShop.Model;

namespace AcerShop.Converters
{
    public class InCartToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Product product && product.Quantity > 0 && !product.IsInCart;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}