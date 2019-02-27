using System;
using System.Windows.Media.Imaging;

namespace Aspect.Utility
{
    public static class ImageMetadataEx
    {
        public static TValue GetQueryAs<TValue>(this BitmapMetadata metadata, string query, TValue defaultValue)
        {
            if (metadata.TryGetQueryAs(query, out TValue result))
            {
                return result;
            }

            return defaultValue;
        }

        public static bool TryGetQueryAs<TValue>(this BitmapMetadata metadata, string query, out TValue result)
        {
            if (!metadata.ContainsQuery(query))
            {
                result = default(TValue);
                return false;
            }

            var value = metadata.GetQuery(query);
            if (value is TValue typedValue)
            {
                result = typedValue;
                return true;
            }

            try
            {
                result = (TValue) Convert.ChangeType(value, typeof(TValue));
                return true;
            }
            catch (Exception ex)
            {
                LogEx.For(typeof(ImageMetadataEx)).Error(
                    ex, "Failed to convert {Value} to {Type}",
                    value, typeof(TValue));

                result = default(TValue);
                return false;
            }
        }
    }
}
