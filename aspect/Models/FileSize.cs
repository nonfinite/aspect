using System;

namespace Aspect.Models
{
    public struct FileSize : IComparable<FileSize>, IComparable
    {
        public FileSize(long bytes)
        {
            if (bytes < 0)
            {
                Bytes = 0;
            }
            else
            {
                Bytes = (ulong) bytes;
            }
        }

        public ulong Bytes { get; }

        public override string ToString()
        {
            decimal value = Bytes;
            var format = "{0:0} B";

            if (value > 1024)
            {
                value /= 1024;
                format = "{0:0.00} KB";
            }

            if (value > 1024)
            {
                value /= 1024;
                format = "{0:0.00} MB";
            }

            if (value > 1024)
            {
                value /= 1024;
                format = "{0:0.00} GB";
            }

            return string.Format(format, value);
        }

        public int CompareTo(FileSize other)
        {
            return Bytes.CompareTo(other.Bytes);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            return obj is FileSize other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(FileSize)}");
        }
    }
}
