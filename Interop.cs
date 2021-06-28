using System;
using System.Runtime.InteropServices;

namespace ZembryoAnalyser
{
    public static class Interop
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct DWMCOLORIZATIONPARAMS
        {
            internal uint ColorizationColor;
            internal uint ColorizationAfterglow;
            internal uint ColorizationColorBalance;
            internal uint ColorizationAfterglowBalance;
            internal uint ColorizationBlurBalance;
            internal uint ColorizationGlassReflectionIntensity;
            internal uint ColorizationOpaqueBlend;
        }

        [DllImport("Dwmapi.dll")]
        internal static extern int DwmIsCompositionEnabled([MarshalAs(UnmanagedType.Bool)] out bool pfEnabled);

        [DllImport("Dwmapi.dll", EntryPoint = "#127")]
        internal static extern int DwmGetColorizationParameters(out DWMCOLORIZATIONPARAMS parameters);

        [DllImport("uxtheme.dll", EntryPoint = "#132")]
        internal static extern uint ShouldAppsUseDarkMode();

        [DllImport("uxtheme.dll", EntryPoint = "#138")]
        internal static extern uint ShouldSystemUseDarkMode();

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);
    }
}
