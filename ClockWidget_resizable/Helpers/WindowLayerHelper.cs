using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public static class WindowLayerHelper
{
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;

    public static void ApplyDesktopMode(Window window)
    {
        try
        {
            var hwnd = new WindowInteropHelper(window).Handle;

            // Push behind everything
            SetWindowPos(hwnd, HWND_BOTTOM, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);

            // Optional: ensure visibility
            window.Show();
            window.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Desktop mode failed: " + ex.Message);
        }
    }

    public static void ApplyOverlayMode(Window window)
    {
        window.Topmost = true;
        window.Show();
        window.Visibility = Visibility.Visible;
    }
}