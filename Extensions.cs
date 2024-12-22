using OpenCVScalar = OpenCvSharp.Scalar;
using OxyPlot;
using Rect = OpenCvSharp.Rect;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Shapes = System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using DrawingColor = System.Drawing.Color;
using System.Windows.Controls;
using System.Linq;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ZembryoAnalyser;

public static partial class Extensions
{
    public static bool ValidVideoFileName(this string fileName)
    {
        string ext = Path.GetExtension(fileName).ToLower();
        return ext switch
        {
            ".dat" or ".wmv" or ".3g2" or ".3gp" or ".3gp2" or ".3gpp" or ".amv" or ".asf" or ".avi" or ".bin" or ".cue" or ".divx" or ".dv" or ".flv" or ".gxf" or ".iso" or ".m1v" or ".m2v" or ".m2t" or ".m2ts" or ".m4v" or ".mkv" or ".mov" or ".mp2" or ".mp2v" or ".mp4" or ".mp4v" or ".mpa" or ".mpe" or ".mpeg" or ".mpeg1" or ".mpeg2" or ".mpeg4" or ".mpg" or ".mpv2" or ".mts" or ".nsv" or ".nuv" or ".ogg" or ".ogm" or ".ogv" or ".ogx" or ".ps" or ".rec" or ".rm" or ".rmvb" or ".tod" or ".ts" or ".tts" or ".vob" or ".vro" or ".webm" => true,
            _ => false,
        };
    }

    public static string ToExceptionString(this Exception exception)
    {
        StringBuilder sb = new();
        Exception temp = exception;
        bool innerException = false;

        while (temp != null)
        {
            sb.AppendLine($"{(innerException ? "---> " : "")}[{temp?.GetType()?.Name ?? "Exception"}]:");
            sb.AppendLine($"{(innerException ? "     " : "")}{temp?.Message}");
            innerException = true;
            temp = temp?.InnerException;
        }

        return sb.ToString();
    }

    public static bool ToBoolean(this object value, bool defaultValue = false)
    {
        return value is bool val ? val : defaultValue;
    }

    public static Brush ToBrush(this object value, byte[] defaultBrush = null)
    {
        return value is byte[] val
            ? new SolidColorBrush(Color.FromArgb(val[0], val[1], val[2], val[3]))
            : new SolidColorBrush(Color.FromArgb(defaultBrush[0], defaultBrush[1], defaultBrush[2], defaultBrush[3]));
    }

    public static AccentColorOptions ToAccentOptions(this object value, AccentColorOptions defaultOption)
    {
        return value is int val ? (AccentColorOptions)val : defaultOption;
    }

    public static BackgroundColorOptions ToBackgroundOptions(this object value, BackgroundColorOptions defaultOption)
    {
        return value is int val ? (BackgroundColorOptions)val : defaultOption;
    }

    public static int Area(this Rectangle r)
    {
        return r.Width * r.Height;
    }

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

    public static string TimeSpanToString(this TimeSpan time)
    {
        if (time.Days > 0)
        {
            return $"{time:d'.'hh':'mm':'ss'.'fff} days";
        }
        else if (time.Hours > 0)
        {
            return $"{time:hh':'mm':'ss'.'fff} hours";
        }
        else if (time.Minutes > 0)
        {
            return $"{time:mm':'ss'.'fff} minutes";
        }
        else if (time.Seconds > 0)
        {
            return $"{time:ss'.'fff} s";
        }
        else
        {
            return $"{time:d'.'hh':'mm':'ss'.'fff} days";
        }
    }

    public static DrawingColor ToDrawingColor(this SolidColorBrush brush)
    {
        return DrawingColor.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
    }

    public static OxyColor ToOxyColor(this SolidColorBrush brush)
    {
        return OxyColor.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
    }

    public static SolidColorBrush ToAlphaSolidColorBrush(this Color color, int alpha)
    {
        return new(Color.FromArgb((byte)alpha, color.R, color.G, color.B));
    }

    public static Color GetColorFromBrush(this Brush brush)
    {
        return brush is SolidColorBrush scb ? scb.Color : Colors.Transparent;
    }

    public static OpenCVScalar GetScalarFromBrush(this Brush brush)
    {
        Color color = brush is SolidColorBrush scb ? scb.Color : Colors.Transparent;
        return OpenCVScalar.FromRgb(color.R, color.G, color.B);
    }

    public static Rect GetShapeBoundingBox(this Shapes.Shape shape)
    {
        if (shape is Shapes.Rectangle rect)
        {
            return new Rect
            (
                (int)Canvas.GetLeft(rect),
                (int)Canvas.GetTop(rect),
                (int)rect.Width,
                (int)rect.Height
            );
        }
        else if (shape is Shapes.Ellipse ellipse)
        {
            return new Rect
            (
                (int)Canvas.GetLeft(ellipse),
                (int)Canvas.GetTop(ellipse),
                (int)ellipse.Width,
                (int)ellipse.Height
            );
        }
        else if (shape is Shapes.Polygon polygon)
        {
            int minX = (int)polygon.Points.Min(p => p.X);
            int maxX = (int)polygon.Points.Max(p => p.X);
            int minY = (int)polygon.Points.Min(p => p.Y);
            int maxY = (int)polygon.Points.Max(p => p.Y);

            return new Rect
            (
                minX,
                minY,
                maxX - minX,
                maxY - minY
            );
        }
        else
        {
            return new Rect
            (
                (int)Canvas.GetLeft(shape),
                (int)Canvas.GetTop(shape),
                (int)shape.Width,
                (int)shape.Height
            );
        }
    }

    public static string GetColorName(this string tag)
    {
        return ColorRegex().Match(tag).Groups[1].Value;
    }

    [GeneratedRegex(".+ \\((.+)\\)")]
    private static partial Regex ColorRegex();
}
