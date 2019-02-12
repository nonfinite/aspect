using System.Runtime.InteropServices;

namespace Aspect.Services.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeSize
    {
        public int Width;
        public int Height;
    }
}
