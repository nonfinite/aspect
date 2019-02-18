using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Aspect.UI.Converters
{
    public sealed class KeyValue : DependencyObject
    {
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
            "From", typeof(object), typeof(KeyValue), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
            "To", typeof(object), typeof(KeyValue), new PropertyMetadata(default(object)));

        public object From
        {
            get => (object) GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public object To
        {
            get => (object) GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }
    }

    [ContentProperty(nameof(Items))]
    public sealed class ValueMapConverter : IValueConverter
    {
        public object ElseFrom { get; set; }
        public object ElseTo { get; set; }
        public List<KeyValue> Items { get; set; } = new List<KeyValue>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var item in Items)
            {
                if (Equals(item.From, value))
                {
                    return item.To;
                }
            }

            return ElseTo;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var item in Items)
            {
                if (Equals(item.To, value))
                {
                    return item.From;
                }
            }

            return ElseFrom;
        }
    }
}
