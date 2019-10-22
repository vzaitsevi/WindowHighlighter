using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using WindowHighlighter.WinApi;
using Rect = WindowHighlighter.WinApi.Rect;
using Win32NativeMethods = WindowHighlighter.WinApi.Win32NativeMethods;

namespace WindowHighlighter.Highlighting
{
    /// <summary>
    /// Interaction logic for HighlightFrame.xaml
    /// </summary>
    public partial class HighlightFrame
    {
        private const string FrameName = "HighlighterFrame";
        private Grid _frameGrid;
        private HwndSource _hwndSource;

        public bool IsUnderDragging { get; set; }

        public HighlightFrame(IntPtr handle, Color color, int width)
        {
            InitializeComponent();
            InitializeGrid();
            CreateHwndSource(handle);
            ShowFrame(color, width);
        }

        public void ShowFrame(Color color, int width)
        {
            _frameGrid.Children.Clear();
            _frameGrid.Children.Add(BuildHighlightFrame(color, width));
            if (!IsUnderDragging)
                Visibility = Visibility.Visible;
        }

        public void HideFrame()
        {
            _frameGrid.Children.Clear();
            Visibility = Visibility.Hidden;
        }

        public void RefreshPosition(IntPtr handle)
        {
            var rect = GetContainerArea(handle);
            Win32NativeMethods.MoveWindow(_hwndSource.Handle, rect.Left, rect.Top, rect.Width, rect.Height, true);
        }

        public void Dispose()
        {
            _hwndSource?.Dispose();
        }

        private void InitializeGrid()
        {
            IsHitTestVisible = false;
            _frameGrid = new Grid {IsHitTestVisible = false};
            AddChild(_frameGrid);
        }

        private void CreateHwndSource(IntPtr handle)
        {
            var rect = GetContainerArea(handle);
            var hwndSourceParameters = new HwndSourceParameters()
            {
                ParentWindow = handle,
                AcquireHwndFocusInMenuMode = false,
                PositionX = rect.Left,
                PositionY = rect.Top,
                Width = rect.Width,
                Height = rect.Height,
                WindowStyle = -2113929216,
                ExtendedWindowStyle = 524292 | Win32NativeMethods.WS_EX_TRANSPARENT,
                UsesPerPixelOpacity = true,
                WindowName = FrameName,
            };
            var hwndSource = new HwndSource(hwndSourceParameters) {RootVisual = this};
            var tableAbove = Win32NativeMethods.GetWindow(handle, (uint) GetWindowConsts.GW_HWNDPREV);
            Win32NativeMethods.SetWindowPos(hwndSource.Handle, tableAbove, 0, 0, 0, 0, (uint)SetWindowPosConsts.SWP_SHOWWINDOW | (uint)SetWindowPosConsts.SWP_NOACTIVATE | (uint)SetWindowPosConsts.SWP_NOMOVE | (uint)SetWindowPosConsts.SWP_NOSIZE);
            _hwndSource = hwndSource;
        }

        private Rect GetContainerArea(IntPtr handle)
        {
            var area = new Rect();
            Win32NativeMethods.GetWindowRect(handle, ref area);
            return area;
        }

        private Rectangle BuildHighlightFrame(Color color, int width)
        {
            var rect = new Rectangle
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = width,
                Width = Width + width,
                Height = Height + width,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(rect, 0 - width / 2);
            Canvas.SetTop(rect, 0 - width / 2);
            return rect;
        }
    }
}
