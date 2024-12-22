using System.Runtime.InteropServices;

namespace ZembryoAnalyser;

internal static partial class Interop
{
    [LibraryImport("Dwmapi.dll", EntryPoint = "DwmIsCompositionEnabled")]
    internal static partial int DwmIsCompositionEnabled([MarshalAs(UnmanagedType.Bool)] out bool pfEnabled);

    [LibraryImport("Dwmapi.dll", EntryPoint = "#127")]
    internal static partial int DwmGetColorizationParameters(out DwmColorizationParams parameters);

    [LibraryImport("uxtheme.dll", EntryPoint = "#132")]
    internal static partial uint ShouldAppsUseDarkMode();

    [LibraryImport("uxtheme.dll", EntryPoint = "#138")]
    internal static partial uint ShouldSystemUseDarkMode();

    [LibraryImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeleteObject(nint hObject);
}
