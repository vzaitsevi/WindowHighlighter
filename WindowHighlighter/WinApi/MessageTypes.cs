namespace WindowHighlighter.WinApi
{
    public enum MessageTypes
    {
        WM_GETTEXT = 0x000D,
        WM_SETTEXT = 0x000C,
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_LBUTTONDBLCLK = 0x0203,
        WM_KEYDOWN = 0x100,
        WM_KEYUP = 0x101,
        WM_SYSKEYDOWN = 0x104,
        WM_SYSKEYUP = 0x105,
        WM_COMMAND = 0x0111,
        WM_SYSCOMMAND = 0x0112,
        WM_GETTEXTLENGTH = 0x000E,
        WM_NCLBUTTONDOWN = 0x00A1,
        WM_NCRBUTTONDOWN = 0x00A4,
        WM_MOUSEMOVE = 0x0200,
        SC_CLOSE = 0xF060,
        VK_RETURN = 0x0D,
        LB_GETCOUNT = 0x018B,
        LB_GETTEXTLEN = 0x018A,
        LB_GETTEXT = 0x0189,
    }
}
