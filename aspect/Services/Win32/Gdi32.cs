using System;
using System.Runtime.InteropServices;

namespace Aspect.Services.Win32
{
    public static class Gdi32
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
