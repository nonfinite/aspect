using System;
using System.Globalization;
using System.Windows.Data;

namespace Aspect.UI.Converters
{
    public class ParamToNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            Equals(value, parameter) ? null : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
