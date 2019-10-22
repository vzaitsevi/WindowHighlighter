using System;
using WindowHighlighter.WinApi;

namespace WindowHighlighter.SystemListening
{
    public class SystemListener : IDisposable
    {
        public event EventHandler<SystemListenerEventArgs> SystemEvent;
        public delegate void SystemEventHandler(IntPtr hWinEventHook, SystemEvents @event, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        private readonly IntPtr _windowCloseWinEventHook;
        private readonly IntPtr _windowPositionChangedEventHook;
        private readonly IntPtr _windowMovingEventHook;
        private readonly IntPtr _windowDraggingEventHook;
        private readonly SystemEventHandler _handler;

        public SystemListener()
        {
            _handler = InternalSystemEventHandler;
            _windowCloseWinEventHook = Win32NativeMethods.SetWinEventHook(SystemEvents.ObjectDestroy, SystemEvents.ObjectDestroy, IntPtr.Zero, _handler, 0, 0, 0x0000);
            _windowPositionChangedEventHook = Win32NativeMethods.SetWinEventHook(SystemEvents.ObjectLocationChange, SystemEvents.ObjectLocationChange, IntPtr.Zero, _handler, 0, 0, 0x0000);
            _windowMovingEventHook = Win32NativeMethods.SetWinEventHook(SystemEvents.SystemMoveSizeStart, SystemEvents.SystemMoveSizeEnd, IntPtr.Zero, _handler, 0, 0, 0x0000);
            _windowDraggingEventHook = Win32NativeMethods.SetWinEventHook(SystemEvents.SystemDragStart, SystemEvents.SystemDragEnd, IntPtr.Zero, _handler, 0, 0, 0x0000);
        }

        ~SystemListener()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Win32NativeMethods.UnhookWinEvent(_windowCloseWinEventHook);
            Win32NativeMethods.UnhookWinEvent(_windowPositionChangedEventHook);
            Win32NativeMethods.UnhookWinEvent(_windowMovingEventHook);
            Win32NativeMethods.UnhookWinEvent(_windowDraggingEventHook);
        }

        protected virtual void OnSystemEvent(SystemListenerEventArgs e)
        {
            SystemEvent?.Invoke(this, e);
        }

        private void InternalSystemEventHandler(IntPtr hWinEventHook, SystemEvents @event, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            OnSystemEvent(new SystemListenerEventArgs(@event, hwnd));
        }
    }
}
