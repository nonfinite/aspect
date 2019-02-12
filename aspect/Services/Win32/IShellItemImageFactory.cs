using System;
using System.Runtime.InteropServices;

namespace Aspect.Services.Win32
{
    [ComImport]
    [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellItemImageFactory
    {
        [PreserveSig]
        int GetImage(
            [In] [MarshalAs(UnmanagedType.Struct)] NativeSize size,
            [In] ThumbnailOptions flags,
            [Out] out IntPtr phbm);
    }
}
