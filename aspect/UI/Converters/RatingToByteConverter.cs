using System;
using System.Globalization;
using System.Windows.Data;

using Aspect.Models;

namespace Aspect.UI.Converters
{
    public sealed class RatingToByteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case Rating rating:
                    return rating.Value;
                case byte b when b == 0:
                    return null;
                case byte b:
                    return new Rating(b);
                default:
                    if (targetType == typeof(byte))
                    {
                        return 0;
                    }
                    else
                    {
                        return null;
                    }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            Convert(value, targetType, parameter, culture);
    }
}
