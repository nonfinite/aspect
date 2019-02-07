using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

using Aspect.Utility;

using Optional;

namespace Aspect.Models
{
    public sealed class FileData : NotifyPropertyChanged
    {
        public FileData(string path, DateTime modifiedInstant, FileSize size)
        {
            Uri = _PathToUri(path);
            Name = Path.GetFileName(path) ?? "";
            ModifiedInstant = modifiedInstant;
            Size = size;

            mDimensions = new Lazy<Size>(() =>
            {
                var decoder = BitmapDecoder.Create(Uri, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);
                return new Size(decoder.Frames[0].PixelWidth, decoder.Frames[0].PixelHeight);
            });
        }

        public static readonly HashSet<string> SupportedFileExtensions = new HashSet<string>(new[]
        {
            ".png",
            ".bmp",
            ".jpg",
            ".jpeg",
            ".gif",
        }, StringComparer.OrdinalIgnoreCase);

        private readonly Lazy<Size> mDimensions;

        private long mRandomKey;

        private Rating? mRating;

        public Size Dimensions => mDimensions.Value;
        public DateTime ModifiedInstant { get; }
        public string Name { get; }

        public long RandomKey
        {
            get => mRandomKey;
            set => Set(ref mRandomKey, value);
        }

        public Rating? Rating
        {
            get => mRating;
            set => Set(ref mRating, value);
        }

        public FileSize Size { get; }

        public Uri Uri { get; }

        private static Uri _PathToUri(string path) => new Uri(Path.GetFullPath(path));

        public static Option<FileData> From(string filePath)
        {
            var info = new FileInfo(filePath);
            if (!info.Exists)
            {
                return Option.None<FileData>();
            }

            if (!SupportedFileExtensions.Contains(info.Extension))
            {
                return Option.None<FileData>();
            }

            return Option.Some(new FileData(info.FullName, info.LastWriteTime, new FileSize(info.Length)));
        }

        public bool IsFile(string filePath) =>
            Uri.Compare(Uri, _PathToUri(filePath),
                UriComponents.AbsoluteUri,
                UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0;

        public override string ToString() => $"{Name} | {ModifiedInstant}";
    }
}
