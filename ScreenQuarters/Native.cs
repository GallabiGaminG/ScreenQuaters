using System.Runtime.InteropServices;
using System.Text;
namespace ScreenQuarters;
internal static class Native
{
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] public struct Point { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)] public struct Placement
    {
        public int Length, Flags, ShowCmd;
        public Point MinPosition, MaxPosition;
        public Rect NormalPosition;
        public static Placement Create() => new() { Length = Marshal.SizeOf<Placement>() };
    }
    [StructLayout(LayoutKind.Sequential)] public struct Info { public int Size; public Rect Bounds, Work; public uint Flags; }
    public delegate void WinEventProc(nint hook, uint evt, nint hwnd, int objectId, int childId, uint thread, uint time);
    public static Info? MonitorInfo(nint monitor)
    {
        var info = new Info { Size = Marshal.SizeOf<Info>() };
        return GetMonitorInfo(monitor, ref info) ? info : null;
    }
    public static long Style(nint hwnd) => IntPtr.Size == 8 ? GetWindowLongPtr(hwnd, -16).ToInt64() : GetWindowLong(hwnd, -16);
    [DllImport("user32.dll")] public static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] public static extern bool IsWindow(nint hwnd);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(nint hwnd);
    [DllImport("user32.dll")] public static extern bool IsIconic(nint hwnd);
    [DllImport("user32.dll")] public static extern bool IsZoomed(nint hwnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(nint hwnd, int command);
    [DllImport("user32.dll")] public static extern nint GetAncestor(nint hwnd, uint flags);
    [DllImport("user32.dll")] public static extern nint GetWindow(nint hwnd, uint command);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(nint hwnd, out uint processId);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetClassName(nint hwnd, StringBuilder name, int length);
    [DllImport("user32.dll", EntryPoint="GetWindowLongPtrW")] static extern nint GetWindowLongPtr(nint hwnd, int index);
    [DllImport("user32.dll", EntryPoint="GetWindowLongW")] static extern int GetWindowLong(nint hwnd, int index);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(nint hwnd, out Rect rect);
    [DllImport("user32.dll")] public static extern bool GetWindowPlacement(nint hwnd, ref Placement placement);
    [DllImport("user32.dll")] public static extern bool SetWindowPlacement(nint hwnd, ref Placement placement);
    [DllImport("user32.dll", SetLastError=true)] public static extern bool SetWindowPos(nint hwnd, nint after, int x, int y, int width, int height, uint flags);
    [DllImport("user32.dll")] public static extern nint MonitorFromWindow(nint hwnd, uint flags);
    [DllImport("user32.dll")] public static extern nint MonitorFromPoint(Point point, uint flags);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] static extern bool GetMonitorInfo(nint monitor, ref Info info);
    [DllImport("user32.dll")] public static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll", SetLastError=true)] public static extern bool RegisterHotKey(nint hwnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] public static extern bool UnregisterHotKey(nint hwnd, int id);
    [DllImport("user32.dll")] public static extern nint SetWinEventHook(uint min, uint max, nint module, WinEventProc callback, uint processId, uint threadId, uint flags);
    [DllImport("user32.dll")] public static extern bool UnhookWinEvent(nint hook);
    [DllImport("dwmapi.dll")] public static extern int DwmGetWindowAttribute(nint hwnd, uint attribute, out Rect value, int size);
}
