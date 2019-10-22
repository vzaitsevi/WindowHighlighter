using System.Text.RegularExpressions;

namespace WindowHighlighter.Settings
{
    public class InterestingWindow
    {
        public string WindowTitlePattern { get; set; }
        public string WindowClassPattern { get; set; }

        public InterestingWindow()
        {
            WindowTitlePattern = "Enter title here...";
            WindowClassPattern = "Enter class here...";
        }

        public InterestingWindow(string windowTitlePattern, string windowClassPattern)
        {
            WindowTitlePattern = windowTitlePattern;
            WindowClassPattern = windowClassPattern;
        }

        public bool IsMatching(string windowTitle, string windowClass)
        {
            return new Regex(WindowTitlePattern ?? "").IsMatch(windowTitle) && new Regex(WindowClassPattern ?? "").IsMatch(windowClass);
        }
    }
}
