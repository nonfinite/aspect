using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using Aspect.Models;
using Aspect.Services;

namespace Aspect.UI.Converters
{
    public sealed class ImageTagViewModelFactory : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty TagServiceProperty = DependencyProperty.Register(
            "TagService", typeof(ITagService), typeof(ImageTagViewModelFactory),
            new PropertyMetadata(default(ITagService)));

        public ITagService TagService
        {
            get => (ITagService) GetValue(TagServiceProperty);
            set => SetValue(TagServiceProperty, value);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            new ImageTagViewModel((FileData) value, TagService);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
