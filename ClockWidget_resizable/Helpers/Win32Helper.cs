using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ClockWidget.Helpers
{
    public static class Win32Helper
    {
        const int GWL_EXSTYLE = -20;
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int WS_EX_LAYERED = 0x00080000;

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public static void SetClickThrough(Window window, bool enable)
        {
            if (window == null) return;
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return;

            var style = GetWindowLongPtr64(hwnd, GWL_EXSTYLE).ToInt64();
            if (enable)
            {
                style |= WS_EX_TRANSPARENT | WS_EX_LAYERED;
            }
            else
            {
                style &= ~(WS_EX_TRANSPARENT);
            }
            SetWindowLongPtr64(hwnd, GWL_EXSTYLE, (IntPtr)style);
        }
    }
}
