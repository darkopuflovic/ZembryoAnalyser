using System.Runtime.InteropServices;

namespace ZembryoAnalyser
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DwmColorizationParams
    {
        internal uint ColorizationColor;
        internal uint ColorizationAfterglow;
        internal uint ColorizationColorBalance;
        internal uint ColorizationAfterglowBalance;
        internal uint ColorizationBlurBalance;
        internal uint ColorizationGlassReflectionIntensity;
        internal uint ColorizationOpaqueBlend;
    }
}
