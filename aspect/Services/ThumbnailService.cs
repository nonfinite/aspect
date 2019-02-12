using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using Aspect.Services.Win32;
using Aspect.Utility;

using Serilog;

namespace Aspect.Services
{
    public static class ThumbnailService
    {
        private static IntPtr? _GetHBitmapThumbnail(string fileName, int width, int height, ThumbnailOptions options)
        {
            const int HR_OK = 0;

            var nativeShellItem = Shell32.CreateItemFromName(fileName);
            if (nativeShellItem == null)
            {
                return null;
            }

            int hr;
            IntPtr hBitmap;

            try
            {
                var nativeSize = new NativeSize {Width = width, Height = height};

                hr = ((IShellItemImageFactory) nativeShellItem)
                    .GetImage(nativeSize, options, out hBitmap);
            }
            finally
            {
                Marshal.ReleaseComObject(nativeShellItem);
            }

            if (hr != HR_OK)
            {
                Log().Information("GetImage for {FileName} failed with {HRESULT}", fileName, hr);
                return null;
            }

            return hBitmap;
        }

        private static BitmapSource _GetThumbnail(string fileName, int width, int height, ThumbnailOptions options)
        {
            var hBitmap = _GetHBitmapThumbnail(fileName, width, height, options);
            if (!hBitmap.HasValue)
            {
                return null;
            }

            try
            {
                var src = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap.Value, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                // freeze allows this to cross the task's tread barrier
                src.Freeze();

                return src;
            }
            finally
            {
                // delete HBitmap to avoid memory leaks
                Gdi32.DeleteObject(hBitmap.Value);
            }
        }

        public static Task<BitmapSource> GetThumbnail(string file, int width, int height)
        {
            Log().Information("Getting {Width}x{Height} thumbnail for {FilePath}", width, height, file);

            var result = _GetThumbnail(file, width, height,
                ThumbnailOptions.BiggerSizeOk | ThumbnailOptions.InMemoryOnly | ThumbnailOptions.ThumbnailOnly);
            if (result != null)
            {
                return Task.FromResult(result);
            }

            return Task.Run(() => _GetThumbnail(file, width, height, ThumbnailOptions.BiggerSizeOk | ThumbnailOptions.ThumbnailOnly));
        }

        private static ILogger Log([CallerMemberName] string memberName = null) =>
            LogEx.For(typeof(ThumbnailService), memberName);
    }
}
