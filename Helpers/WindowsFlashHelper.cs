using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ClientSideChatApp.Helpers
{
    public static class WindowFlashHelper
    {
        [DllImport("user32.dll")]

        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {

            public uint cbSize;

            public IntPtr hwnd;

            public uint dwFlags;

            public uint uCount;

            public uint dwTimeout;

        }

        private const uint FLASHW_TRAY = 3;
        
        private const uint FLASHW_TIMERNOFG = 12;   

        public static void FlashTaskbar()
        {

            Application.Current.Dispatcher.Invoke(() =>
            {

                Window mainWindow = Application.Current.MainWindow;

                if (mainWindow == null || mainWindow.IsActive) return;

                var wih = new WindowInteropHelper(mainWindow);

                IntPtr hwnd = wih.Handle;

                if (hwnd == IntPtr.Zero) return;

                FLASHWINFO info = new FLASHWINFO
                {

                    hwnd = hwnd,

                    dwFlags = FLASHW_TRAY | FLASHW_TIMERNOFG,

                    uCount = uint.MaxValue,

                    dwTimeout = 0

                };

                info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));

                FlashWindowEx(ref info);
            });
        }
    }
}