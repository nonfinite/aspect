using System;
using System.Collections.Generic;
using System.IO;

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
        }

        private static readonly HashSet<string> SupportedFileExtensions = new HashSet<string>(new[]
        {
            ".png",
            ".bmp",
            ".jpg",
            ".jpeg",
            ".gif",
        }, StringComparer.OrdinalIgnoreCase);

        private long mRandomKey;

        public Uri Uri { get; }
        public string Name { get; }
        public DateTime ModifiedInstant { get; }
        public FileSize Size { get; }

        public long RandomKey
        {
            get => mRandomKey;
            set => Set(ref mRandomKey, value);
        }

        private static Uri _PathToUri(string path) => new Uri(Path.GetFullPath(path));

        public override string ToString() => $"{Name} | {ModifiedInstant}";

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
    }
}
