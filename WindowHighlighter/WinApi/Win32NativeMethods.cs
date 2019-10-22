using System;
using System.Runtime.InteropServices;
using System.Text;
using WindowHighlighter.SystemListening;

namespace WindowHighlighter.WinApi
{
    public static class Win32NativeMethods
    {
        public const int WS_EX_TRANSPARENT = 0x00000020;

        [DllImport("User32.dll", SetLastError = true)]
        public static extern IntPtr SetWinEventHook(SystemEvents eventMin, SystemEvents eventMax, IntPtr hmodWinEventProc, SystemListener.SystemEventHandler lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(int xPoint, int yPoint);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll")]
        public static extern long SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("USER32.DLL")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern uint RealGetWindowClass(IntPtr hWnd, StringBuilder type, uint cchType);

        [DllImport("user32.dll", EntryPoint = "SendMessageTimeout", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint SendMessageTimeoutText(IntPtr hWnd, int msg, int countOfChars, StringBuilder text, SendMessageTimeoutFlags flags, uint timeout, out IntPtr result);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, SendMessageTimeoutFlags flags, uint timeout, out IntPtr result);

        public static string GetWindowClass(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return "";
            var windowClass = new StringBuilder(256);
            RealGetWindowClass(hWnd, windowClass, 256);
            return windowClass.ToString().Trim();
        }

        public static string GetWindowText(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return "";
            IntPtr length;
            var result = (int) SendMessageTimeout(hWnd, (uint) MessageTypes.WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | SendMessageTimeoutFlags.SMTO_BLOCK | SendMessageTimeoutFlags.SMTO_ERRORONEXIT, 200, out length);
            if (result == 0 || length == IntPtr.Zero) return "";

            var text = new StringBuilder((int)length + 1);
            var resultUint = SendMessageTimeoutText(hWnd, (int) MessageTypes.WM_GETTEXT, (int)length + 1, text, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | SendMessageTimeoutFlags.SMTO_BLOCK | SendMessageTimeoutFlags.SMTO_ERRORONEXIT, 400, out length);
            return resultUint == 0 ? "" : text.ToString();
        }

    }
}
