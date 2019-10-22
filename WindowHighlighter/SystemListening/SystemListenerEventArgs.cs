using System;

namespace WindowHighlighter.SystemListening
{
    public class SystemListenerEventArgs : EventArgs
    {
        public SystemEvents SystemEvent { get; }
        public IntPtr WindowHandle { get; }

        public SystemListenerEventArgs(SystemEvents @event, IntPtr hwnd)
        {
            SystemEvent = @event;
            WindowHandle = hwnd;
        }
    }
}
