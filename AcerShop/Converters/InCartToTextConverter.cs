using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using AcerShop.Model;

namespace AcerShop.Converters
{
    public class InCartToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Product product) return "Ошибка";

            return product.Quantity <= 0 ? "Нет в наличии"
                : product.IsInCart ? "В корзине"
                : "В корзину";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}