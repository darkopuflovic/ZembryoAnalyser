using System;
using System.Windows.Media;

namespace ZembryoAnalyser
{
    public static class WindowColors
    {
        public static SolidColorBrush InactiveColor { get; private set; }

        static WindowColors()
        {
            Color temp = BlendColor(Color.FromArgb(255, 152, 47, 255), Color.FromArgb(255, 30, 30, 30), 50) ?? Color.FromArgb(255, 152, 47, 255);
            InactiveColor = new SolidColorBrush(temp);
        }

        public static void SetInactiveColor(Brush accentBrush, Brush backgroundBrush)
        {
            if (accentBrush is SolidColorBrush && backgroundBrush is SolidColorBrush)
            {
                Color accentColor = (accentBrush as SolidColorBrush).Color;
                Color backgroundColor = (backgroundBrush as SolidColorBrush).Color;
                Color temp = BlendColor(accentColor, backgroundColor, 30) ?? accentColor;
                InactiveColor = new SolidColorBrush(temp);
            }
        }

        private static Color? BlendColor(Color color1, Color color2, double color2Perc)
        {
            return color2Perc is < 0 or > 100 ? null
                : Color.FromRgb(
                    BlendColorChannel(color1.R, color2.R, color2Perc),
                    BlendColorChannel(color1.G, color2.G, color2Perc),
                    BlendColorChannel(color1.B, color2.B, color2Perc));
        }

        private static byte BlendColorChannel(double channel1, double channel2, double channel2Perc)
        {
            double buff = channel1 + ((channel2 - channel1) * channel2Perc / 100D);
            return Math.Min((byte)Math.Round(buff), (byte)255);
        }

        private static DwmColorizationParams? GetColorizationParameters()
        {
            try
            {
                int succeed = Interop.DwmIsCompositionEnabled(out bool isEnabled);

                if ((succeed != 0) || !isEnabled)
                {
                    return null;
                }

                succeed = Interop.DwmGetColorizationParameters(out DwmColorizationParams parameters);

                return succeed != 0 ? null : parameters;
            }
            catch
            {
                return null;
            }
        }

        private static Color? GetChromeColor()
        {
            Color? targetColor = GetColorizationColor();
            DwmColorizationParams? parameters = GetColorizationParameters();
            Color baseColor = Color.FromRgb(217, 217, 217);

            return targetColor.HasValue && parameters.HasValue
                ? BlendColor(targetColor.Value, baseColor, 100 - parameters.Value.ColorizationColorBalance)
                : null;
        }

        private static Color? GetColorizationColor()
        {
            try
            {
                DwmColorizationParams? parameters = GetColorizationParameters();

                if (!parameters.HasValue)
                {
                    return null;
                }

                byte[] array = new byte[]
                {
                    (byte)(parameters.Value.ColorizationColor >> 24),
                    (byte)(parameters.Value.ColorizationColor >> 16),
                    (byte)(parameters.Value.ColorizationColor >> 8),
                    (byte)parameters.Value.ColorizationColor
                };

                return Color.FromArgb(255, array[1], array[2], array[3]);
            }
            catch
            {
                return null;
            }
        }

        public static Color? GetAccentColor()
        {
            return Environment.OSVersion.Version.Major switch
            {
                >= 10 => GetColorizationColor(),
                >= 7 => GetChromeColor(),
                _ => null
            };
        }

        public static bool? AreAppsInDarkMode()
        {
            try
            {
                return (Interop.ShouldAppsUseDarkMode() & 0xFF) == 1;
            }
            catch
            {
                return null;
            }
        }

        public static bool? AreWindowsInDarkMode()
        {
            try
            {
                return (Interop.ShouldSystemUseDarkMode() & 0xFF) == 1;
            }
            catch
            {
                return null;
            }
        }
    }
}
