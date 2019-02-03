using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Aspect.UI.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            Equals(value, parameter);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Equals(value, true))
            {
                return parameter;
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
