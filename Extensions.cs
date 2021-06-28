using OxyPlot;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace ZembryoAnalyser
{
    public static class Extensions
    {
        public static bool ToBoolean(this object value, bool defaultValue = false) =>
            value is bool val ? val : defaultValue;

        public static Brush ToBrush(this object value, byte[] defaultBrush = null) =>
            value is byte[] val
                ? new SolidColorBrush(Color.FromArgb(val[0], val[1], val[2], val[3]))
                : new SolidColorBrush(Color.FromArgb(defaultBrush[0], defaultBrush[1], defaultBrush[2], defaultBrush[3]));

        public static AccentColorOptions ToAccentOptions(this object value, AccentColorOptions defaultOption) =>
            value is int val ? (AccentColorOptions)val : defaultOption;

        public static BackgroundColorOptions ToBackgroundOptions(this object value, BackgroundColorOptions defaultOption) =>
            value is int val ? (BackgroundColorOptions)val : defaultOption;

        public static int Area(this Rectangle r) =>
            r.Width * r.Height;

        public static bool IsPointInEllipse(this Rectangle rectangle, int x, int y)
        {
            double a = rectangle.Width / 2d;
            double b = rectangle.Height / 2d;

            double dx = x - rectangle.X - a;
            double dy = y - rectangle.Y - b;

            return (dx * dx / (a * a)) + (dy * dy / (b * b)) <= 1;
        }

        public static bool IsPointInPolygon(this Point[] polygon, int x, int y)
        {
            int polygonLength = polygon.Length, i = 0;
            bool inside = false;
            float pointX = x, pointY = y;
            float startX, startY, endX, endY;
            Point endPoint = polygon[polygonLength - 1];
            endX = endPoint.X;
            endY = endPoint.Y;

            while (i < polygonLength)
            {
                startX = endX; startY = endY;
                endPoint = polygon[i++];
                endX = endPoint.X; endY = endPoint.Y;

                inside ^= (endY > pointY ^ startY > pointY) &&
                          ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
            }

            return inside;
        }

        public static OxyColor ToOxyColor(this SolidColorBrush brush) =>
            OxyColor.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);

        public static SolidColorBrush ToAlphaSolidColorBrush(this Color color, int alpha) =>
            new(Color.FromArgb((byte)alpha, color.R, color.G, color.B));

        public static Color GetColorFromBrush(this Brush brush) =>
            brush is SolidColorBrush scb ? scb.Color : Colors.Transparent;

        public static string GetColorName(this string tag) =>
            Regex.Match(tag, ".+ \\((.+)\\)").Groups[1].Value;
    }
}
