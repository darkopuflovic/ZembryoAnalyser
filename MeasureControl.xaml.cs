using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ZembryoAnalyser;

/// <summary>
/// Interaction logic for MeasureControl.xaml
/// </summary>
public partial class MeasureControl : UserControl
{
    public MeasureType MeasureType { get; set; }

    public double Scale
    {
        get => scale;
        set
        {
            scale = value;
            ShapeThickness = 3 * scale;
        }
    }

    internal double ShapeThickness
    {
        get => (double)GetValue(ShapeThicknessProperty);
        set => SetValue(ShapeThicknessProperty, value);
    }

    internal static readonly DependencyProperty ShapeThicknessProperty =
        DependencyProperty.Register("ShapeThickness", typeof(double), typeof(MeasureControl), new PropertyMetadata(3d));

    private MainWindow mainWindow;
    private Point lastPoint;
    private Line line;
    private Polyline polyline;
    private Rectangle rectangle;
    private Ellipse ellipse;
    private Polygon polygon;
    private Polyline angle;
    private double scale;

    public MeasureControl()
    {
        InitializeComponent();
        lastPoint = new Point();
        line = null;
        polyline = null;
        rectangle = null;
        ellipse = null;
        polygon = null;
        angle = null;
        scale = 1;
    }

    public void SetMainWindow(MainWindow main)
    {
        mainWindow = main;
    }

    private static void SetShapeThicknessBinding(Shape shape)
    {
        shape.SetBinding(Shape.StrokeThicknessProperty, new Binding
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MeasureControl), 1),
            Path = new PropertyPath("ShapeThickness")
        });
    }

    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        lastPoint = e.GetPosition(canvas);

        switch (MeasureType)
        {
            case MeasureType.Line:
                if (line == null)
                {
                    _ = canvas.CaptureMouse();
                    line = new Line
                    {
                        Stroke = Brushes.Crimson,
                        X1 = lastPoint.X,
                        Y1 = lastPoint.Y,
                        X2 = lastPoint.X,
                        Y2 = lastPoint.Y
                    };

                    SetShapeThicknessBinding(line);

                    _ = canvas.Children.Add(line);
                }
                break;
            case MeasureType.Polyline:
                if (polyline == null)
                {
                    _ = canvas.CaptureMouse();
                    polyline = new Polyline
                    {
                        Stroke = Brushes.Crimson
                    };

                    SetShapeThicknessBinding(polyline);

                    polyline.Points.Add(lastPoint);

                    _ = canvas.Children.Add(polyline);
                }
                else
                {
                    polyline.Points.Add(lastPoint);

                    double len = 0;

                    for (int i = 0; i < polyline.Points.Count - 1; i++)
                    {
                        len += Length(polyline.Points[i], polyline.Points[i + 1]);
                    }

                    mainWindow.SetStatusText($"Length: {Math.Round(len, 2)} px");
                }
                break;
            case MeasureType.Rectangle:
                if (rectangle == null)
                {
                    _ = canvas.CaptureMouse();
                    rectangle = new Rectangle
                    {
                        Stroke = Brushes.Crimson
                    };

                    SetShapeThicknessBinding(rectangle);

                    _ = canvas.Children.Add(rectangle);
                }
                break;
            case MeasureType.Ellipse:
                if (ellipse == null)
                {
                    _ = canvas.CaptureMouse();
                    ellipse = new Ellipse
                    {
                        Stroke = Brushes.Crimson
                    };

                    SetShapeThicknessBinding(ellipse);

                    _ = canvas.Children.Add(ellipse);
                }
                break;
            case MeasureType.Polygon:
                if (polygon == null)
                {
                    _ = canvas.CaptureMouse();
                    polygon = new Polygon
                    {
                        Stroke = Brushes.Crimson
                    };

                    SetShapeThicknessBinding(polygon);

                    polygon.Points.Add(lastPoint);

                    _ = canvas.Children.Add(polygon);
                }
                else
                {
                    polygon.Points.Add(lastPoint);

                    double len = 0;

                    for (int i = 0; i < polygon.Points.Count; i++)
                    {
                        if (i == polygon.Points.Count - 1)
                        {
                            len += Length(polygon.Points[i], polygon.Points[0]);
                        }
                        else
                        {
                            len += Length(polygon.Points[i], polygon.Points[i + 1]);
                        }
                    }

                    mainWindow.SetStatusText($"Perimeter = {Math.Round(len, 2)} px, Area = {Math.Round(PolygonArea([.. polygon.Points]), 2)} px²");
                }
                break;
            case MeasureType.Angle:
                if (angle == null)
                {
                    _ = canvas.CaptureMouse();
                    angle = new Polyline
                    {
                        Stroke = Brushes.Crimson
                    };

                    SetShapeThicknessBinding(angle);

                    angle.Points.Add(lastPoint);

                    _ = canvas.Children.Add(angle);
                }
                else
                {
                    if (angle.Points.Count < 3)
                    {
                        angle.Points.Add(lastPoint);
                    }
                    else
                    {
                        angle.Points.Clear();
                        angle = null;

                        mainWindow.SetStatusText("Ready");
                        canvas.ReleaseMouseCapture();
                    }

                    if (angle?.Points?.Count == 3)
                    {
                        mainWindow.SetStatusText($"Angle: {Math.Round(Math.Abs(CalculateAngle([.. angle.Points])), 2)}°");
                    }
                }
                break;
            case MeasureType.None:
            default:
                break;
        }
    }

    private static double PolygonArea(List<Point> points)
    {
        int num_points = points.Count;
        Point[] pts = new Point[num_points + 1];
        points.CopyTo(pts, 0);
        pts[num_points] = points[0];

        double area = 0;

        for (int i = 0; i < num_points; i++)
        {
            area += (pts[i + 1].X - pts[i].X) * (pts[i + 1].Y + pts[i].Y) / 2;
        }

        return Math.Abs(area);
    }

    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        switch (MeasureType)
        {
            case MeasureType.Line:
            case MeasureType.Rectangle:
            case MeasureType.Ellipse:
                lastPoint = new Point();

                canvas.Children.Clear();

                line = null;
                polyline = null;
                rectangle = null;
                ellipse = null;
                polygon = null;
                angle = null;

                mainWindow.SetStatusText("Ready");
                canvas.ReleaseMouseCapture();
                break;
            case MeasureType.Polyline:
            case MeasureType.Polygon:
            case MeasureType.Angle:
                if (e.ChangedButton == MouseButton.Right)
                {
                    lastPoint = new Point();

                    canvas.Children.Clear();

                    line = null;
                    polyline = null;
                    rectangle = null;
                    ellipse = null;
                    polygon = null;
                    angle = null;

                    mainWindow.SetStatusText("Ready");
                    canvas.ReleaseMouseCapture();
                }
                break;
            case MeasureType.None:
            default:
                break;
        }
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        Point point = e.GetPosition(canvas);

        switch (MeasureType)
        {
            case MeasureType.Line:
                if (line != null)
                {
                    line.X2 = point.X;
                    line.Y2 = point.Y;

                    mainWindow.SetStatusText($"Length = {Length(line)} px");
                }
                break;
            case MeasureType.Rectangle:
                if (rectangle != null)
                {
                    Canvas.SetLeft(rectangle, Math.Min(lastPoint.X, point.X));
                    Canvas.SetTop(rectangle, Math.Min(lastPoint.Y, point.Y));

                    rectangle.Width = Math.Abs(point.X - lastPoint.X);
                    rectangle.Height = Math.Abs(point.Y - lastPoint.Y);

                    RectangleGeometry rg = (RectangleGeometry)rectangle.RenderedGeometry;
                    mainWindow.SetStatusText($"Perimeter = {Math.Round((2 * rg.Rect.Width) + (2 * rg.Rect.Height), 2)} px, a = {Math.Round(rg.Rect.Width, 2)}, b = {Math.Round(rg.Rect.Height, 2)}, Area = {Math.Round(rg.GetArea(), 2)} px²");
                }
                break;
            case MeasureType.Ellipse:
                if (ellipse != null)
                {
                    Canvas.SetLeft(ellipse, Math.Min(lastPoint.X, point.X));
                    Canvas.SetTop(ellipse, Math.Min(lastPoint.Y, point.Y));

                    ellipse.Width = Math.Abs(point.X - lastPoint.X);
                    ellipse.Height = Math.Abs(point.Y - lastPoint.Y);

                    EllipseGeometry rg = (EllipseGeometry)ellipse.RenderedGeometry;
                    mainWindow.SetStatusText($"Perimeter = {Math.Round(2 * Math.PI * Math.Sqrt(((rg.RadiusX * rg.RadiusX) + (rg.RadiusY * rg.RadiusY)) / (2 * 1.0)), 2)} px, a = {Math.Round(rg.RadiusX, 2)}, b = {Math.Round(rg.RadiusY, 2)}, Area = {Math.Round(rg.GetArea(), 2)} px²");
                }
                break;
            case MeasureType.Angle:
            case MeasureType.Polyline:
            case MeasureType.Polygon:
            case MeasureType.None:
            default:
                break;
        }
    }

    private static double Length(Point p1, Point p2)
    {
        return Math.Round(Math.Sqrt(Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.X - p1.X, 2)), 2);
    }

    private static string Length(Line line)
    {
        return Math.Round(Math.Sqrt(Math.Pow(line.Y2 - line.Y1, 2) + Math.Pow(line.X2 - line.X1, 2)), 2).ToString("N2", CultureInfo.InvariantCulture);
    }

    private static double CalculateAngle(Point[] points)
    {
        if (points.Length == 3)
        {
            Vector v1 = new(points[0].X - points[1].X, points[0].Y - points[1].Y);
            Vector v2 = new(points[2].X - points[1].X, points[2].Y - points[1].Y);

            return Vector.AngleBetween(v1, v2);
        }
        else
        {
            return 0;
        }
    }
}
