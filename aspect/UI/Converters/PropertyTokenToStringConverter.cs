using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Data;

using Serilog.Events;
using Serilog.Parsing;

namespace Aspect.UI.Converters
{
    public sealed class PropertyTokenToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            PropertyToken token = null;
            LogEvent evt = null;

            foreach (var value in values)
            {
                switch (value)
                {
                    case PropertyToken t:
                        token = t;
                        break;
                    case LogEvent e:
                        evt = e;
                        break;
                }
            }

            if (token == null || evt == null)
            {
                return null;
            }

            var sb = new StringBuilder();

            using (var writer = new StringWriter(sb))
            {
                token.Render(evt.Properties, writer, culture);
            }

            return sb.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
