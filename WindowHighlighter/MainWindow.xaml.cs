using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WindowHighlighter.Highlighting;
using WindowHighlighter.Settings;
using WindowHighlighter.SystemListening;
using WindowHighlighter.WinApi;
using Point = WindowHighlighter.WinApi.Point;

namespace WindowHighlighter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        private static readonly Regex NonDigitRegex = new Regex("[^0-9]+");
        private static readonly Regex ColorRegex = new Regex("^(#[a-fA-F0-9]{6}|#[a-fA-F0-9]{8}|black|green|silver|gray|olive|white|yellow|maroon|navy|red|blue|purple|teal|fuchsia|aqua)$");
        private readonly SystemListener _systemListener;
        private readonly HighlightSettings _settings;
        private readonly Dictionary<IntPtr, HighlightFrame> _highlightFrames;
        private IntPtr _windowUnderMouseHandle = IntPtr.Zero;
        

        public MainWindow()
        {
            InitializeComponent();
            Closing += OnWindowClosing;
            var settings = HighlightSettings.LoadSettings();
            HighlighterWidthSelector.Text = settings.HighlighterWidth.ToString();
            HighlighterColorSelector.Text = settings.HighlighterColor.ToString();
            HighlighterColor.Fill = new SolidColorBrush(settings.HighlighterColor);
            _settings = settings;
            InterestingWindowListPanel.DataContext = _settings;
            UpdateInterestingWindowListView();
            _highlightFrames = new Dictionary<IntPtr, HighlightFrame>();
            _systemListener = new SystemListener();
            _systemListener.SystemEvent += OnSystemEvent;
        }

        private void UpdateInterestingWindowListView()
        {
            var interestingWindowsDefined = _settings?.InterestingWindows?.Count > 0;
            InterestingWindowListEmptyText.Visibility = interestingWindowsDefined ? Visibility.Collapsed : Visibility.Visible;
            InterestingWindowList.Visibility = interestingWindowsDefined ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnAddNewInterestingWindowClicked(object sender, RoutedEventArgs e)
        {
            _settings?.InterestingWindows?.Add(new InterestingWindow());
            UpdateInterestingWindowListView();
        }

        private void OnDeleteInterestingWindowClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;
            var interestingWindowToDelete = button.Tag as InterestingWindow;
            _settings?.InterestingWindows?.Remove(interestingWindowToDelete);
            UpdateInterestingWindowListView();
        }

        private void OnSelectedWidthTyped(object sender, TextCompositionEventArgs e)
        {
            if (NonDigitRegex.IsMatch(e.Text)) e.Handled = true;
        }

        private void OnSelectedWidthPasted(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (string.IsNullOrEmpty(text) || NonDigitRegex.IsMatch(text))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void OnSelectedWidthChanged(object sender, TextChangedEventArgs e)
        {
            var currentWidth = HighlighterWidthSelector.Text;
            if (_settings == null) return;
            _settings.HighlighterWidth = string.IsNullOrEmpty(currentWidth) ? 0 : int.Parse(currentWidth);
        }

        private void OnSelectedColorChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var colorText = HighlighterColorSelector.Text;
            if (string.IsNullOrEmpty(colorText) || !ColorRegex.IsMatch(colorText)) return;
            var convertedColor = ColorConverter.ConvertFromString(colorText);
            if (convertedColor == null) return;
            var color = (Color)convertedColor;
            if (_settings == null) return;
            _settings.HighlighterColor = color;
            HighlighterColor.Fill = new SolidColorBrush(color);
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            _settings?.SaveSettings();
            if (_highlightFrames != null)
            {
                Application.Current?.Dispatcher?.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {{
                    foreach (var frame in _highlightFrames.Values)
                        frame.Dispose();
                    _highlightFrames.Clear();
                }}));
            }
        }

        private void OnSystemEvent(object sender, SystemListenerEventArgs e)
        {
            switch (e.SystemEvent)
            {
                case SystemEvents.ObjectDestroy:
                    OnPossibleInterestingWindowClosed(e.WindowHandle);
                    break;
                case SystemEvents.SystemDragStart:
                case SystemEvents.SystemMoveSizeStart:
                    OnPossibleInterestingWindowMoving(e.WindowHandle, true);
                    break;
                case SystemEvents.SystemDragEnd:
                case SystemEvents.SystemMoveSizeEnd:
                    OnPossibleInterestingWindowMoving(e.WindowHandle, false);
                    break;
                case SystemEvents.ObjectLocationChange:
                    if (e.WindowHandle == IntPtr.Zero)
                        OnMouseMove();
                    else
                        OnPossibleInterestingWindowPositionChanged(e.WindowHandle);
                    break;
            }
        }

        private void OnPossibleInterestingWindowClosed(IntPtr handle)
        {
            if (!_highlightFrames.ContainsKey(handle)) return;
            var frameToDestroy = _highlightFrames[handle];
            _highlightFrames.Remove(handle);
            Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {{
                frameToDestroy.Dispose();
            }}));
        }

        private void OnPossibleInterestingWindowMoving(IntPtr handle, bool underDragging)
        {
            if (!_highlightFrames.ContainsKey(handle)) return;
            var frameToHandle = _highlightFrames[handle];
            Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {{
                frameToHandle.IsUnderDragging = underDragging;
                if (underDragging)
                    frameToHandle.HideFrame();
                else
                {
                    frameToHandle.RefreshPosition(handle);
                    frameToHandle.ShowFrame(_settings.HighlighterColor, _settings.HighlighterWidth);
                }
            }}));
        }

        private void OnPossibleInterestingWindowPositionChanged(IntPtr handle)
        {
            if (!_highlightFrames.ContainsKey(handle)) return;
            var frameToRefresh = _highlightFrames[handle];
            Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {{
                frameToRefresh.RefreshPosition(handle);
                frameToRefresh.ShowFrame(_settings.HighlighterColor, _settings.HighlighterWidth);
            }}));
        }

        private void OnMouseMove()
        {
            Point mousePosition;
            Win32NativeMethods.GetCursorPos(out mousePosition);
            var windowHandleAtMousePosition = GetWindowHandleFromPoint(mousePosition.X, mousePosition.Y);
            if (_windowUnderMouseHandle == windowHandleAtMousePosition) return;

            if (_highlightFrames.ContainsKey(_windowUnderMouseHandle))
            {
                var oldFrame = _highlightFrames[_windowUnderMouseHandle];
                Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {{
                    oldFrame.HideFrame();
                }}));
            }

            _windowUnderMouseHandle = windowHandleAtMousePosition;
            if (IntPtr.Zero == windowHandleAtMousePosition) return;
            var frame = GetHighlightFrame(windowHandleAtMousePosition);
            Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {{
                frame.RefreshPosition(windowHandleAtMousePosition);
                frame.ShowFrame(_settings.HighlighterColor, _settings.HighlighterWidth);
            }}));
        }

        private HighlightFrame GetHighlightFrame(IntPtr handle)
        {
            if (_highlightFrames.ContainsKey(handle)) return _highlightFrames[handle];
            HighlightFrame frame = null;
            Application.Current?.Dispatcher?.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {{
                frame = new HighlightFrame(handle, _settings.HighlighterColor, _settings.HighlighterWidth);
                _highlightFrames.Add(handle, frame);
            }}));
            return frame;
        }

        private IntPtr GetWindowHandleFromPoint(int xPoint, int yPoint)
        {
            var handle = Win32NativeMethods.WindowFromPoint(xPoint, yPoint);
            if (IsInterestingWindow(handle)) return handle;
            var parentHandle = Win32NativeMethods.GetParent(handle);
            while (parentHandle != IntPtr.Zero)
            {
                if (IsInterestingWindow(parentHandle)) return parentHandle;
                parentHandle = Win32NativeMethods.GetParent(parentHandle);
            }
            return IntPtr.Zero;
        }

        private bool IsInterestingWindow(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return false;
            if (_settings?.InterestingWindows == null) return false;
            if (!Win32NativeMethods.IsWindowVisible(handle)) return false;
            var windowTitle = Win32NativeMethods.GetWindowText(handle) ?? "";
            var windowClass = Win32NativeMethods.GetWindowClass(handle) ?? "";
            if (Title.Equals(windowTitle)) return false;
            return _settings.InterestingWindows.Any(interestingWindow => interestingWindow.IsMatching(windowTitle, windowClass));
        }
    }
}
