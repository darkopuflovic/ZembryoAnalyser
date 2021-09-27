using System;
using System.Runtime.InteropServices;

namespace ZembryoAnalyser
{
    internal static class Interop
    {
        [DllImport("Dwmapi.dll")]
        internal static extern int DwmIsCompositionEnabled([MarshalAs(UnmanagedType.Bool)] out bool pfEnabled);

        [DllImport("Dwmapi.dll", EntryPoint = "#127")]
        internal static extern int DwmGetColorizationParameters(out DwmColorizationParams parameters);

        [DllImport("uxtheme.dll", EntryPoint = "#132")]
        internal static extern uint ShouldAppsUseDarkMode();

        [DllImport("uxtheme.dll", EntryPoint = "#138")]
        internal static extern uint ShouldSystemUseDarkMode();

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);
    }
}
