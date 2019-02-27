using System;
using System.Windows;
using System.Windows.Media.Imaging;

using Aspect.Utility;

namespace Aspect.Models
{
    public sealed class ImageMetadata
    {
        public ImageMetadata(Uri uri)
        {
            var decoder = BitmapDecoder.Create(uri, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);
            Dimensions = new Size(decoder.Frames[0].PixelWidth, decoder.Frames[0].PixelHeight);

            if (decoder.Frames[0].Metadata is BitmapMetadata metadata)
            {
                Orientation = (ImageOrientation) metadata.GetQueryAs(
                    "System.Photo.Orientation", (ushort) ImageOrientation.Normal);
            }

            IsAnimated = uri.ToString().EndsWith(".gif", StringComparison.OrdinalIgnoreCase) &&
                         decoder.Frames.Count > 1;
        }

        public Size Dimensions { get; }
        public bool IsAnimated { get; }
        public ImageOrientation Orientation { get; }
    }
}
