using System;
using System.Runtime.InteropServices;

namespace Aspect.Services.Win32
{
    public static class Shell32
    {
        private const uint SEE_MASK_INVOKEIDLIST = 12;
        private const int SW_SHOW = 5;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        public static bool ShowFileProperties(string filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            return ShellExecuteEx(ref info);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public readonly IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)] public readonly string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)] public readonly string lpDirectory;
            public int nShow;
            public readonly IntPtr hInstApp;
            public readonly IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)] public readonly string lpClass;
            public readonly IntPtr hkeyClass;
            public readonly uint dwHotKey;
            public readonly IntPtr hIcon;
            public readonly IntPtr hProcess;
        }
    }
}
