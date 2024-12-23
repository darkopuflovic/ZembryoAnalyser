﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ZembryoAnalyser;

/// <summary>
/// Interaction logic for DrawingControl.xaml
/// </summary>
public partial class DrawingControl : UserControl
{
    public List<Shape> Geometries { get; set; }

    public GeometryType GeometryType
    {
        get => type;
        set
        {
            type = value;
            GeometryFinished();
        }
    }

    public double Scale
    {
        get => scale;
        set
        {
            scale = value;
            ShapeThickness = 2 * scale;
        }
    }

    internal double ShapeThickness
    {
        get => (double)GetValue(ShapeThicknessProperty);
        set => SetValue(ShapeThicknessProperty, value);
    }

    internal static readonly DependencyProperty ShapeThicknessProperty =
        DependencyProperty.Register("ShapeThickness", typeof(double), typeof(DrawingControl), new PropertyMetadata(2d));

    private Shape editShape;
    private Ellipse centroid;
    private Point centroidPoint;
    private HitType mouseHitType;
    private Point lastPoint;
    private bool dragInProgress;
    private bool centroidSelected;
    private GeometryType type;
    private Point anchorPoint;
    private Ellipse ellipse;
    private Rectangle rectangle;
    private Polygon polygon;
    private Polygon edgePolygon;
    private int dotIndex;
    private double scale;
    private Stack<(string name, Color color)> colors;
    private readonly List<Ellipse> tempPoints;
    private readonly Polyline segment;

    public DrawingControl()
    {
        InitializeComponent();
        type = GeometryType.None;
        segment = new Polyline();
        Geometries = [];
        mouseHitType = HitType.None;
        dragInProgress = false;
        centroidSelected = false;
        lastPoint = new Point();
        tempPoints = [];
        editShape = null;
        centroid = null;
        centroidPoint = default;
        scale = 1;
        FillColors();
    }

    private void FillColors()
    {
        colors = new Stack<(string name, Color color)>(
        [
            ("Aquamarine", Colors.Aquamarine),
            ("Bisque", Colors.Bisque),
            ("BlueViolet", Colors.BlueViolet),
            ("Coral", Colors.Coral),
            ("CornflowerBlue", Colors.CornflowerBlue),
            ("Crimson", Colors.Crimson),
            ("DarkOrchid", Colors.DarkOrchid),
            ("Salmon", Colors.Salmon),
            ("LightSeaGreen", Colors.LightSeaGreen),
            ("DarkTurquoise", Colors.DarkTurquoise),
            ("DeepPink", Colors.DeepPink),
            ("DeepSkyBlue", Colors.DeepSkyBlue),
            ("Gold", Colors.Gold),
            ("MediumSpringGreen", Colors.MediumSpringGreen),
            ("CadetBlue", Colors.CadetBlue),
            ("MediumSlateBlue", Colors.MediumSlateBlue)
        ]);
    }

    public void RemoveAllShapes()
    {
        canvas.Children.Clear();
        Geometries.Clear();
        FillColors();
    }

    public void RemoveAutoRectangle()
    {
        for (int i = Geometries.Count - 1; i >= 0; i--)
        {
            if (Geometries[i].Tag.ToString() == "Rectangle (Yellow)")
            {
                Shape geometry = Geometries[i];
                canvas.Children.Remove(geometry);
                Geometries.RemoveAt(i);
                break;
            }
        }

        if (edgePolygon != null)
        {
            canvas.Children.Remove(edgePolygon);
            edgePolygon = null;
        }
    }

    public void InsertRectangle(Brush stroke, Brush fill, string name, int left, int top, int width, int height)
    {
        Rectangle rect = new()
        {
            Fill = fill,
            Stroke = stroke,
            Tag = name
        };

        Canvas.SetLeft(rect, left);
        Canvas.SetTop(rect, top);

        rect.Width = width;
        rect.Height = height;

        _ = canvas.Children.Add(rect);
        Geometries.Add(rect);
    }

    public void InsertEllipse(Brush stroke, Brush fill, string name, int left, int top, int width, int height)
    {
        Ellipse eli = new()
        {
            Fill = fill,
            Stroke = stroke,
            Tag = name
        };

        Canvas.SetLeft(eli, left);
        Canvas.SetTop(eli, top);

        eli.Width = width;
        eli.Height = height;

        _ = canvas.Children.Add(eli);
        Geometries.Add(eli);
    }

    public void InsertPolygon(Brush stroke, Brush fill, string name, List<System.Drawing.Point> points)
    {
        Polygon polygon = new()
        {
            Fill = fill,
            Stroke = stroke,
            Tag = name
        };

        SetShapeThicknessBinding(polygon);

        foreach (var p in points)
        {
            polygon.Points.Add(new Point(p.X, p.Y));
        }

        edgePolygon = polygon;
        _ = canvas.Children.Add(edgePolygon);
    }

    private (string name, Color color)? GetNextColor()
    {
        return colors.Count > 0 ? colors.Pop() : null;
    }

    private void ReturnColor((string name, Color color) c)
    {
        if (c.name != "Yellow")
        {
            colors.Push(c);
        }
    }

    private static void SetShapeThicknessBinding(Shape shape)
    {
        shape.SetBinding(Shape.StrokeThicknessProperty, new Binding
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DrawingControl), 1),
            Path = new PropertyPath("ShapeThickness")
        });
    }

    private void GeometryFinished()
    {
        switch (type)
        {
            case GeometryType.Ellipse:
                if (ellipse != null && ellipse.ActualHeight >= 5 && ellipse.ActualWidth >= 5)
                {
                    Geometries.Add(ellipse);
                }
                else
                {
                    canvas.Children.Remove(ellipse);

                    if (ellipse != null && !string.IsNullOrWhiteSpace(ellipse.Tag.ToString()) && ellipse.Stroke != null)
                    {
                        ReturnColor((name: ellipse.Tag.ToString().GetColorName(), color: ellipse.Stroke.GetColorFromBrush()));
                    }
                }
                ellipse = null;
                break;
            case GeometryType.Rectangle:
                if (rectangle != null && rectangle.ActualHeight >= 5 && rectangle.ActualWidth >= 5)
                {
                    Geometries.Add(rectangle);
                }
                else
                {
                    canvas.Children.Remove(rectangle);

                    if (rectangle != null && !string.IsNullOrWhiteSpace(rectangle.Tag.ToString()) && rectangle.Stroke != null)
                    {
                        ReturnColor((name: rectangle.Tag.ToString().GetColorName(), color: rectangle.Stroke.GetColorFromBrush()));
                    }
                }
                rectangle = null;
                break;
            case GeometryType.Polygon:
                if (polygon != null && polygon.ActualHeight >= 5 && polygon.ActualWidth >= 5 && polygon.Points.Count > 2)
                {
                    Geometries.Add(polygon);
                }
                else
                {
                    canvas.Children.Remove(polygon);

                    if (polygon != null && !string.IsNullOrWhiteSpace(polygon.Tag.ToString()) && polygon.Stroke != null)
                    {
                        ReturnColor((name: polygon.Tag.ToString().GetColorName(), color: polygon.Stroke.GetColorFromBrush()));
                    }
                }
                polygon = null;
                segment.Points.Clear();
                canvas.Children.Remove(segment);
                break;
            case GeometryType.Erase:
            case GeometryType.Edit:
            case GeometryType.Info:
            case GeometryType.None:
            default:
                break;
        }
    }

    private async void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        switch (type)
        {
            case GeometryType.Ellipse:
                if (ellipse == null)
                {
                    (string name, Color color)? col = GetNextColor();

                    if (col.HasValue)
                    {
                        _ = canvas.CaptureMouse();
                        anchorPoint = ReturnInsideImage(e.GetPosition(canvas));
                        (string name, Color color) = col.Value;

                        ellipse = new Ellipse
                        {
                            Stroke = new SolidColorBrush(color),
                            Fill = color.ToAlphaSolidColorBrush(128),
                            Tag = $"Ellipse ({name})"
                        };

                        SetShapeThicknessBinding(ellipse);

                        _ = canvas.Children.Add(ellipse);
                    }
                    else
                    {
                        await ShowStatusBarInfo("Sorry, no more colors.");
                    }
                }
                break;
            case GeometryType.Rectangle:
                if (rectangle == null)
                {
                    (string name, Color color)? col = GetNextColor();

                    if (col.HasValue)
                    {
                        _ = canvas.CaptureMouse();
                        anchorPoint = ReturnInsideImage(e.GetPosition(canvas));
                        (string name, Color color) = col.Value;

                        rectangle = new Rectangle
                        {
                            Stroke = new SolidColorBrush(color),
                            Fill = color.ToAlphaSolidColorBrush(128),
                            Tag = $"Rectangle ({name})"
                        };

                        SetShapeThicknessBinding(rectangle);

                        _ = canvas.Children.Add(rectangle);
                    }
                    else
                    {
                        await ShowStatusBarInfo("Sorry, no more colors.");
                    }
                }
                break;
            case GeometryType.Polygon:
                if (polygon == null)
                {
                    (string name, Color color)? col = GetNextColor();

                    if (col.HasValue)
                    {
                        _ = canvas.CaptureMouse();
                        Point point = ReturnInsideImage(e.GetPosition(canvas));
                        (string name, Color color) = col.Value;

                        polygon = new Polygon
                        {
                            Stroke = new SolidColorBrush(color),
                            Fill = color.ToAlphaSolidColorBrush(128),
                            Tag = $"Polygon ({name})"
                        };

                        SetShapeThicknessBinding(polygon);

                        polygon.Points.Add(point);
                        _ = canvas.Children.Add(polygon);

                        segment.Points.Add(point);
                        segment.Points.Add(point);
                        segment.Stroke = new SolidColorBrush(Color.FromArgb(64, 0, 0, 0));
                        _ = canvas.Children.Add(segment);
                    }
                    else
                    {
                        await ShowStatusBarInfo("Sorry, no more colors.");
                    }
                }
                else if (polygon != null)
                {
                    Point point = ReturnInsideImage(e.GetPosition(canvas));
                    segment.Points[1] = point;

                    polygon.Points.Add(segment.Points[1]);
                    segment.Points[0] = segment.Points[1];
                }
                break;
            case GeometryType.Erase:
                Point p = ReturnInsideImage(e.GetPosition(canvas));

                for (int i = Geometries.Count - 1; i >= 0; i--)
                {
                    if (VisualTreeHelper.HitTest(canvas, p).VisualHit == Geometries[i])
                    {
                        Shape geometry = Geometries[i];
                        canvas.Children.Remove(geometry);
                        Geometries.RemoveAt(i);
                        ReturnColor((name: geometry.Tag.ToString().GetColorName(), color: geometry.Stroke.GetColorFromBrush()));
                        break;
                    }
                }
                break;
            case GeometryType.Edit:
                Point po = ReturnInsideImage(e.GetPosition(canvas));
                int index = -1;

                for (int i = Geometries.Count - 1; i >= 0; i--)
                {
                    if (VisualTreeHelper.HitTest(canvas, po).VisualHit == Geometries[i])
                    {
                        index = i;
                        break;
                    }
                }

                if (index > -1)
                {
                    canvas.IsHitTestVisible = false;
                    editShape = Geometries[index];

                    if (editShape is Ellipse or Rectangle)
                    {
                        EditRectangleAndEllipse(editShape is Rectangle);
                    }
                    else if (editShape is Polygon)
                    {
                        EditPolygon();
                    }
                }
                break;
            case GeometryType.Info:
                Point pos = ReturnInsideImage(e.GetPosition(canvas));
                
                for (int i = Geometries.Count - 1; i >= 0; i--)
                {
                    if (VisualTreeHelper.HitTest(canvas, pos).VisualHit == Geometries[i])
                    {
                        Shape geometry = Geometries[i];
                        string info = geometry.Tag.ToString();
                        var array = info.Split('(', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(p => p.Trim(['(', ')', ' ']))
                                        .ToList();

                        if (array.Count == 2)
                        {
                            await ShowStatusBarInfo($"Type: {array[0]}, Color: {array[1]}");
                        }
                        break;
                    }
                }
                break;
            case GeometryType.None:
            default:
                break;
        }
    }

    private static async Task ShowStatusBarInfo(string info)
    {
        await Task.Run(() =>
        {
            App.MainApplicationWindow?.SetWaitingState(info, TimeSpan.FromSeconds(2));
        });
    }

    private void EditPolygon()
    {
        grid.Children.Remove(editCanvas);
        editCanvas = new Canvas();
        _ = grid.Children.Add(editCanvas);
        _ = editCanvas.CaptureMouse();

        double gap = 9 * scale;
        double elipseHalf = gap / 2;

        tempPoints.Clear();
        PointCollection points = (editShape as Polygon).Points;

        for (int i = 0; i < points.Count; i++)
        {
            Point el = points[i];

            Ellipse eli = new()
            {
                Height = gap,
                Width = gap,
                Stroke = Brushes.OrangeRed,
                Fill = new SolidColorBrush(Color.FromArgb(192, 255, 128, 0))
            };

            SetShapeThicknessBinding(eli);

            Canvas.SetLeft(eli, el.X - elipseHalf);
            Canvas.SetTop(eli, el.Y - elipseHalf);

            _ = editCanvas.Children.Add(eli);
            tempPoints.Add(eli);
        }

        centroid = new Ellipse
        {
            Height = gap,
            Width = gap,
            Stroke = Brushes.Green,
            Fill = new SolidColorBrush(Color.FromArgb(192, 34, 139, 34))
        };

        SetShapeThicknessBinding(centroid);

        centroidPoint = GetCentroid([.. points]);
        Canvas.SetLeft(centroid, centroidPoint.X - elipseHalf);
        Canvas.SetTop(centroid, centroidPoint.Y - elipseHalf);

        _ = editCanvas.Children.Add(centroid);

        editCanvas.MouseDown += (p, q) =>
        {
            if (Cursor == Cursors.SizeAll)
            {
                dragInProgress = true;
                lastPoint = ReturnInsideImage(q.GetPosition(editCanvas));
            }
            else if (Cursor == Cursors.Cross)
            {
                Point el = ReturnInsideImage(q.GetPosition(editCanvas));

                int ind = FindIndexToInsertTo(points, el);
                points.Insert(ind, el);

                Ellipse eli = new()
                {
                    Height = gap,
                    Width = gap,
                    Stroke = Brushes.OrangeRed,
                    Fill = new SolidColorBrush(Color.FromArgb(192, 255, 128, 0))
                };

                SetShapeThicknessBinding(eli);

                Canvas.SetLeft(eli, el.X - elipseHalf);
                Canvas.SetTop(eli, el.Y - elipseHalf);

                _ = editCanvas.Children.Add(eli);
                tempPoints.Insert(ind, eli);
                Cursor = Cursors.SizeAll;
                dotIndex = ind;
            }
        };

        editCanvas.MouseDown += (send, q) =>
        {
            if (q.RightButton == MouseButtonState.Pressed)
            {
                editCanvas.Children.Clear();
                centroid = null;
                canvas.IsHitTestVisible = true;
                editCanvas.ReleaseMouseCapture();
                editShape = null;
                Cursor = Cursors.Arrow;
            }
        };

        editCanvas.MouseUp += (p, q) =>
        {
            dragInProgress = false;
        };

        editCanvas.MouseMove += (p, q) =>
        {
            if (dragInProgress)
            {
                Point current = ReturnInsideImage(q.GetPosition(editCanvas));

                if (centroidSelected)
                {
                    if (CheckPolygonInsideImage([.. points], current, lastPoint))
                    {
                        for (int i = 0; i < tempPoints.Count; i++)
                        {
                            Ellipse el = tempPoints[i];
                            Point po = points[i];
                            po = new Point(po.X + (current.X - lastPoint.X), po.Y + (current.Y - lastPoint.Y));
                            Canvas.SetLeft(el, po.X - elipseHalf);
                            Canvas.SetTop(el, po.Y - elipseHalf);
                            points[i] = po;
                        }
                    }
                }
                else
                {
                    Ellipse el = tempPoints[dotIndex];
                    Point po = points[dotIndex];
                    po = new Point(po.X + (current.X - lastPoint.X), po.Y + (current.Y - lastPoint.Y));
                    Canvas.SetLeft(el, po.X - elipseHalf);
                    Canvas.SetTop(el, po.Y - elipseHalf);
                    points[dotIndex] = po;
                }

                lastPoint = current;

                centroidPoint = GetCentroid([.. points]);
                Canvas.SetLeft(centroid, centroidPoint.X - elipseHalf);
                Canvas.SetTop(centroid, centroidPoint.Y - elipseHalf);
            }
            else
            {
                bool presek = false;
                centroidSelected = false;

                for (int i = 0; i < tempPoints.Count; i++)
                {
                    Point elka = ReturnInsideImage(q.GetPosition(tempPoints[i]));

                    if (elka.X > 0 && elka.X < gap && elka.Y > 0 && elka.Y < gap)
                    {
                        presek = true;
                        dotIndex = i;
                    }
                }

                Point c = ReturnInsideImage(q.GetPosition(this));

                if (Math.Abs(c.X - centroidPoint.X) < gap && Math.Abs(c.Y - centroidPoint.Y) < gap)
                {
                    centroidSelected = true;
                }

                if (centroidSelected)
                {
                    Cursor = Cursors.SizeAll;
                }
                else if (presek)
                {
                    Cursor = Cursors.SizeAll;
                }
                else
                {
                    Point current = ReturnInsideImage(q.GetPosition(editCanvas));
                    Pen pen = new(Brushes.Transparent, 5 * scale);

                    if (editShape.RenderedGeometry.StrokeContains(pen, current))
                    {
                        Cursor = Cursors.Cross;
                    }
                    else
                    {
                        if (Cursor != Cursors.Arrow)
                        {
                            Cursor = Cursors.Arrow;
                        }
                    }
                }
            }
        };
    }

    private bool CheckPolygonInsideImage(List<Point> tempPoints, Point current, Point lastPoint)
    {
        double deltaX = current.X - lastPoint.X;
        double deltaY = current.Y - lastPoint.Y;

        foreach (Point temp in tempPoints)
        {
            double newX = temp.X + deltaX;
            double newY = temp.Y + deltaY;

            if (newX < 0 || newX > Width || newY < 0 || newY > Height)
            {
                return false;
            }
        }

        return true;
    }

    private static Point GetCentroid(List<Point> poly)
    {
        double accumulatedArea = 0.0f;
        double centerX = 0.0f;
        double centerY = 0.0f;

        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            double temp = (poly[i].X * poly[j].Y) - (poly[j].X * poly[i].Y);
            accumulatedArea += temp;
            centerX += (poly[i].X + poly[j].X) * temp;
            centerY += (poly[i].Y + poly[j].Y) * temp;
        }

        if (Math.Abs(accumulatedArea) < 1E-7f)
        {
            return new Point();
        }

        accumulatedArea *= 3f;
        return new Point((int)(centerX / accumulatedArea), (int)(centerY / accumulatedArea));
    }

    private static int FindIndexToInsertTo(PointCollection points, Point el)
    {
        int ind = -1;
        double min = double.MaxValue;

        for (int i = 0; i < points.Count; i++)
        {
            if (i == points.Count - 1)
            {
                double dist = FindDistanceToSegment(el, points[i], points[0]);

                if (min > dist)
                {
                    min = dist;
                    ind = 0;
                }
            }
            else
            {
                double dist = FindDistanceToSegment(el, points[i], points[i + 1]);

                if (min > dist)
                {
                    min = dist;
                    ind = i + 1;
                }
            }
        }

        return ind;
    }

    private static double FindDistanceToSegment(Point pt, Point p1, Point p2)
    {
        double dx = p2.X - p1.X;
        double dy = p2.Y - p1.Y;

        if ((dx == 0) && (dy == 0))
        {
            dx = pt.X - p1.X;
            dy = pt.Y - p1.Y;
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        double t = (((pt.X - p1.X) * dx) + ((pt.Y - p1.Y) * dy)) / ((dx * dx) + (dy * dy));

        if (t < 0)
        {
            dx = pt.X - p1.X;
            dy = pt.Y - p1.Y;
        }
        else if (t > 1)
        {
            dx = pt.X - p2.X;
            dy = pt.Y - p2.Y;
        }
        else
        {
            Point closest = new(p1.X + (t * dx), p1.Y + (t * dy));
            dx = pt.X - closest.X;
            dy = pt.Y - closest.Y;
        }

        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private void EditRectangleAndEllipse(bool rectangle)
    {
        Rectangle tempRectangle = new()
        {
            Fill = Brushes.Transparent,
            Stroke = rectangle ? Brushes.Indigo : Brushes.Firebrick,
            StrokeDashArray = [5, 2, 1, 2],
        };

        SetShapeThicknessBinding(tempRectangle);

        grid.Children.Remove(editCanvas);
        editCanvas = new Canvas();
        _ = grid.Children.Add(editCanvas);
        _ = editCanvas.CaptureMouse();

        Canvas.SetTop(tempRectangle, Canvas.GetTop(editShape));
        Canvas.SetLeft(tempRectangle, Canvas.GetLeft(editShape));
        tempRectangle.Height = editShape.Height;
        tempRectangle.Width = editShape.Width;

        editCanvas.MouseDown += (send, q) =>
        {
            if (q.LeftButton == MouseButtonState.Pressed)
            {
                mouseHitType = SetHitType(tempRectangle, q.GetPosition(canvas));
                SetMouseCursor();

                if (mouseHitType == HitType.None)
                {
                    return;
                }

                lastPoint = ReturnInsideImage(q.GetPosition(canvas));
                dragInProgress = true;
            }
            else if (q.RightButton == MouseButtonState.Pressed)
            {
                editCanvas.Children.Clear();
                canvas.IsHitTestVisible = true;
                editCanvas.ReleaseMouseCapture();
                editShape = null;
                Cursor = Cursors.Arrow;
            }
        };

        editCanvas.MouseUp += (send, q) =>
        {
            dragInProgress = false;
        };

        editCanvas.MouseMove += (send, q) =>
        {
            if (!dragInProgress)
            {
                mouseHitType = SetHitType(tempRectangle, q.GetPosition(canvas));
                SetMouseCursor();
            }
            else
            {
                Point point = ReturnInsideImage(q.GetPosition(canvas));
                double offsetX = point.X - lastPoint.X;
                double offsetY = point.Y - lastPoint.Y;

                double newX = Canvas.GetLeft(tempRectangle);
                double newY = Canvas.GetTop(tempRectangle);

                double newWidth = tempRectangle.Width;
                double newHeight = tempRectangle.Height;

                switch (mouseHitType)
                {
                    case HitType.Body:
                        newX += offsetX;
                        newY += offsetY;
                        break;
                    case HitType.UL:
                        newX += offsetX;
                        newY += offsetY;
                        newWidth -= offsetX;
                        newHeight -= offsetY;
                        break;
                    case HitType.UR:
                        newY += offsetY;
                        newWidth += offsetX;
                        newHeight -= offsetY;
                        break;
                    case HitType.LR:
                        newWidth += offsetX;
                        newHeight += offsetY;
                        break;
                    case HitType.LL:
                        newX += offsetX;
                        newWidth -= offsetX;
                        newHeight += offsetY;
                        break;
                    case HitType.L:
                        newX += offsetX;
                        newWidth -= offsetX;
                        break;
                    case HitType.R:
                        newWidth += offsetX;
                        break;
                    case HitType.B:
                        newHeight += offsetY;
                        break;
                    case HitType.T:
                        newY += offsetY;
                        newHeight -= offsetY;
                        break;
                    case HitType.None:
                    default:
                        break;
                }

                if ((newWidth > 0) && (newHeight > 0))
                {
                    if (InsideImage(newX, newWidth, editCanvas.ActualWidth))
                    {
                        Canvas.SetLeft(tempRectangle, newX);
                        tempRectangle.Width = newWidth;
                        Canvas.SetLeft(editShape, newX);
                        editShape.Width = newWidth;
                        lastPoint = point;
                    }

                    if (InsideImage(newY, newHeight, editCanvas.ActualHeight))
                    {
                        Canvas.SetTop(tempRectangle, newY);
                        tempRectangle.Height = newHeight;
                        Canvas.SetTop(editShape, newY);
                        editShape.Height = newHeight;
                        lastPoint = point;
                    }
                }
            }
        };

        _ = editCanvas.Children.Add(tempRectangle);
        mouseHitType = SetHitType(tempRectangle, Mouse.GetPosition(canvas));
        SetMouseCursor();
    }

    private static bool InsideImage(double newPos, double newSize, double imageSize)
    {
        return newPos >= 0 && newPos + newSize <= imageSize;
    }

    private bool IsPointInsideImage(Point p)
    {
        return p.X > 0 && p.X < Width && p.Y > 0 && p.Y < Height;
    }

    private Point ReturnInsideImage(Point p)
    {
        if (!IsPointInsideImage(p))
        {
            p.X = p.X < 0 ? 0 : p.X;
            p.X = p.X > Width ? Width : p.X;
            p.Y = p.Y < 0 ? 0 : p.Y;
            p.Y = p.Y > Height ? Height : p.Y;
        }

        return p;
    }

    private HitType SetHitType(Rectangle rect, Point point)
    {
        double left = Canvas.GetLeft(rect);
        double top = Canvas.GetTop(rect);
        double right = left + rect.Width;
        double bottom = top + rect.Height;

        if (point.X < left)
        {
            return HitType.None;
        }

        if (point.X > right)
        {
            return HitType.None;
        }

        if (point.Y < top)
        {
            return HitType.None;
        }

        if (point.Y > bottom)
        {
            return HitType.None;
        }

        double gap = 4 * scale;

        return point.X - left < gap
            ? point.Y - top < gap ? HitType.UL : bottom - point.Y < gap ? HitType.LL : HitType.L
            : right - point.X < gap
            ? point.Y - top < gap ? HitType.UR : bottom - point.Y < gap ? HitType.LR : HitType.R
            : point.Y - top < gap ? HitType.T : bottom - point.Y < gap ? HitType.B : HitType.Body;
    }

    private void SetMouseCursor()
    {
        Cursor cursor = Cursors.Arrow;

        switch (mouseHitType)
        {
            case HitType.None:
                cursor = Cursors.Arrow;
                break;
            case HitType.Body:
                cursor = Cursors.SizeAll;
                break;
            case HitType.UL:
            case HitType.LR:
                cursor = Cursors.SizeNWSE;
                break;
            case HitType.LL:
            case HitType.UR:
                cursor = Cursors.SizeNESW;
                break;
            case HitType.T:
            case HitType.B:
                cursor = Cursors.SizeNS;
                break;
            case HitType.L:
            case HitType.R:
                cursor = Cursors.SizeWE;
                break;
            default:
                break;
        }

        if (Cursor != cursor)
        {
            Cursor = cursor;
        }
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        switch (type)
        {
            case GeometryType.Ellipse:
                MoveRectangleOrEllipse(ReturnInsideImage(e.GetPosition(canvas)), ellipse);
                break;
            case GeometryType.Rectangle:
                MoveRectangleOrEllipse(ReturnInsideImage(e.GetPosition(canvas)), rectangle);
                break;
            case GeometryType.Polygon:
                if (polygon != null)
                {
                    segment.Points[1] = ReturnInsideImage(e.GetPosition(canvas));
                }
                break;
            case GeometryType.Erase:
            case GeometryType.Edit:
            case GeometryType.Info:
            case GeometryType.None:
            default:
                break;
        }
    }

    private void MoveRectangleOrEllipse(Point point, Shape shape)
    {
        if (shape != null)
        {
            double minX = Math.Min(point.X, anchorPoint.X);
            double minY = Math.Min(point.Y, anchorPoint.Y);
            double maxX = Math.Max(point.X, anchorPoint.X);
            double maxY = Math.Max(point.Y, anchorPoint.Y);

            Canvas.SetTop(shape, minY);
            Canvas.SetLeft(shape, minX);

            double height = maxY - minY;
            double width = maxX - minX;

            shape.Height = Math.Abs(height);
            shape.Width = Math.Abs(width);
        }
    }

    private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        GeometryFinished();
        canvas.ReleaseMouseCapture();
    }
    private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        switch (type)
        {
            case GeometryType.Ellipse:
                GeometryFinished();
                canvas.ReleaseMouseCapture();
                break;
            case GeometryType.Rectangle:
                GeometryFinished();
                canvas.ReleaseMouseCapture();
                break;
            case GeometryType.Polygon:
                break;
            case GeometryType.Erase:
                break;
            case GeometryType.Edit:
                break;
            case GeometryType.Info:
                break;
            case GeometryType.None:
                break;
            default:
                break;
        }
    }

    private void UserControl_LostMouseCapture(object sender, MouseEventArgs e)
    {
        GeometryFinished();
        editCanvas.Children.Clear();
        canvas.IsHitTestVisible = true;
        editShape = null;
        Cursor = Cursors.Arrow;
    }
}
