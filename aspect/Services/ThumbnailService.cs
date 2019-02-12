using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Aspect.Utility;

using Serilog;

namespace Aspect.Services
{
    public static class ThumbnailService
    {
        public static Task<BitmapSource> GetThumbnail(string file, int width, int height)
        {
            Log().Information("Getting {Width}x{Height} thumbnail for {FilePath}", width, height, file);
            return Task.Delay(1000).ContinueWith(t => (BitmapSource) null);
        }

        private static ILogger Log([CallerMemberName] string memberName = null) =>
            LogEx.For(typeof(ThumbnailService), memberName);
    }
}
