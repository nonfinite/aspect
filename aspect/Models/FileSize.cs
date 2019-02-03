namespace Aspect.Models
{
    public struct FileSize
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
    }
}
