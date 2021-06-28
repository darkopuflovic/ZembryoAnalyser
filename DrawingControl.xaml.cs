using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ZembryoAnalyser
{
    /// <summary>
    /// Interaction logic for DrawingControl.xaml
    /// </summary>
    public partial class DrawingControl : UserControl
    {
        public List<Shape> Geometries { get; set; }

        public GeometryType GeometryType
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
                GeometryFinished();
            }
        }

        private Shape editShape;
        private HitType mouseHitType;
        private Point lastPoint;
        private bool dragInProgress;
        private GeometryType type;
        private Point anchorPoint;
        private Ellipse ellipse;
        private Rectangle rectangle;
        private Polygon polygon;
        private int dotIndex;
        private readonly List<Ellipse> tempPoints;
        private readonly Polyline segment;
        private readonly Queue<(string name, Color color)> colors;

        public DrawingControl()
        {
            InitializeComponent();
            type = GeometryType.None;
            segment = new Polyline();
            Geometries = new List<Shape>();
            mouseHitType = HitType.None;
            dragInProgress = false;
            lastPoint = new Point();
            tempPoints = new List<Ellipse>();
            editShape = null;
            colors = new Queue<(string name, Color color)>(new List<(string name, Color color)>
            {
                ("Crimson", Colors.Crimson),
                ("DodgerBlue", Colors.DodgerBlue),
                ("Lime", Colors.Lime),
                ("DeepPink", Colors.DeepPink),
                ("SteelBlue", Colors.SteelBlue),
                ("SlateBlue", Colors.SlateBlue),
                ("Tomato", Colors.Tomato),
                ("MediumVioletRed", Colors.MediumVioletRed),
                ("Gold", Colors.Gold),
                ("Cyan", Colors.Cyan),
                ("CornflowerBlue", Colors.CornflowerBlue),
                ("Red", Colors.Red),
                ("Green", Colors.Green),
                ("Blue", Colors.Blue),
                ("Chocolate", Colors.Chocolate),
                ("DarkBlue", Colors.DarkBlue)
            });
        }

        public void InsertRectangle(Brush stroke, Brush fill, string name, int left, int top, int width, int height)
        {
            var rect = new Rectangle
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
            var eli = new Ellipse
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

        private (string name, Color color)? GetNextColor() =>
            colors.Count > 0 ? colors.Dequeue() : null;

        private void ReturnColor((string name, Color color) c) =>
            colors.Enqueue(c);

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
                    }
                    rectangle = null;
                    break;
                case GeometryType.Polygon:
                    if (polygon != null && polygon.ActualHeight >= 5 && polygon.ActualWidth >= 5)
                    {
                        Geometries.Add(polygon);
                    }
                    else
                    {
                        canvas.Children.Remove(polygon);
                    }
                    polygon = null;
                    segment.Points.Clear();
                    canvas.Children.Remove(segment);
                    break;
                case GeometryType.Erase:
                case GeometryType.Edit:
                case GeometryType.None:
                default:
                    break;
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

                            _ = canvas.Children.Add(ellipse);
                        }
                        else
                        {
                            ShowNoMoreColorsInfo();
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

                            _ = canvas.Children.Add(rectangle);
                        }
                        else
                        {
                            ShowNoMoreColorsInfo();
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

                            polygon.Points.Add(point);
                            _ = canvas.Children.Add(polygon);

                            segment.Points.Add(point);
                            segment.Points.Add(point);
                            segment.Stroke = new SolidColorBrush(Color.FromArgb(64, 0, 0, 0));
                            _ = canvas.Children.Add(segment);
                        }
                        else
                        {
                            ShowNoMoreColorsInfo();
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
                            EditRectangleAndEllipse();
                        }
                        else if (editShape is Polygon)
                        {
                            EditPolygon();
                        }
                    }
                    break;
                case GeometryType.None:
                default:
                    break;
            }
        }

        private static void ShowNoMoreColorsInfo()
        {
            var main = (MainWindow)Application.Current.MainWindow;

            _ = Task.Run(async () =>
            {
                main.SetWaitingState("Sorry, no more colors.");
                await Task.Delay(2000);
                main.ReleaseWaitingState();
            });
        }

        private void EditPolygon()
        {
            grid.Children.Remove(editCanvas);
            editCanvas = new Canvas();
            _ = grid.Children.Add(editCanvas);
            _ = editCanvas.CaptureMouse();

            editCanvas.MouseDown += (send, q) =>
            {
                if (q.RightButton == MouseButtonState.Pressed)
                {
                    editCanvas.Children.Clear();
                    canvas.IsHitTestVisible = true;
                    editCanvas.ReleaseMouseCapture();
                    editShape = null;
                    Cursor = Cursors.Arrow;
                }
            };

            tempPoints.Clear();
            PointCollection points = (editShape as Polygon).Points;

            for (int i = 0; i < points.Count; i++)
            {
                Point el = points[i];

                var eli = new Ellipse
                {
                    Height = 9,
                    Width = 9,
                    Stroke = Brushes.OrangeRed,
                    Fill = new SolidColorBrush(Color.FromArgb(192, 255, 128, 0))
                };

                Canvas.SetLeft(eli, el.X - 4);
                Canvas.SetTop(eli, el.Y - 4);

                _ = editCanvas.Children.Add(eli);
                tempPoints.Add(eli);
            }

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

                    var eli = new Ellipse
                    {
                        Height = 9,
                        Width = 9,
                        Stroke = Brushes.OrangeRed,
                        Fill = new SolidColorBrush(Color.FromArgb(192, 255, 128, 0))
                    };

                    Canvas.SetLeft(eli, el.X - 4);
                    Canvas.SetTop(eli, el.Y - 4);

                    _ = editCanvas.Children.Add(eli);
                    tempPoints.Insert(ind, eli);
                    Cursor = Cursors.SizeAll;
                    dotIndex = ind;
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
                    Ellipse el = tempPoints[dotIndex];
                    Point po = points[dotIndex];
                    points[dotIndex] = new Point(po.X + (current.X - lastPoint.X), po.Y + (current.Y - lastPoint.Y));
                    Canvas.SetLeft(el, points[dotIndex].X - 4);
                    Canvas.SetTop(el, points[dotIndex].Y - 4);
                    lastPoint = current;
                }
                else
                {
                    bool presek = false;

                    for (int i = 0; i < tempPoints.Count; i++)
                    {
                        Point elka = ReturnInsideImage(q.GetPosition(tempPoints[i]));

                        if (elka.X > 0 && elka.X < 9 && elka.Y > 0 && elka.Y < 9)
                        {
                            presek = true;
                            dotIndex = i;
                        }
                    }

                    if (presek)
                    {
                        Cursor = Cursors.SizeAll;
                    }
                    else
                    {
                        Point current = ReturnInsideImage(q.GetPosition(editCanvas));
                        var pen = new Pen(Brushes.Transparent, 5);

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
                var closest = new Point(p1.X + (t * dx), p1.Y + (t * dy));
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        private void EditRectangleAndEllipse()
        {
            var tempRectangle = new Rectangle
            {
                Fill = Brushes.Transparent,
                Stroke = Brushes.OrangeRed,
                StrokeDashArray = new DoubleCollection { 5, 5, 1, 5 },
                StrokeThickness = 2
            };

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

        private static bool InsideImage(double newPos, double newSize, double imageSize) =>
            newPos >= 0 && newPos + newSize <= imageSize;

        private bool IsPointInsideImage(Point p) =>
            p.X > 0 && p.X < Width && p.Y > 0 && p.Y < Height;

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

        private static HitType SetHitType(Rectangle rect, Point point)
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

            const double gap = 4;

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
                    if (ellipse != null)
                    {
                        Point point = ReturnInsideImage(e.GetPosition(canvas));

                        double minX = Math.Min(point.X, anchorPoint.X);
                        double minY = Math.Min(point.Y, anchorPoint.Y);
                        double maxX = Math.Max(point.X, anchorPoint.X);
                        double maxY = Math.Max(point.Y, anchorPoint.Y);

                        Canvas.SetTop(ellipse, minY);
                        Canvas.SetLeft(ellipse, minX);

                        double height = maxY - minY;
                        double width = maxX - minX;

                        ellipse.Height = Math.Abs(height);
                        ellipse.Width = Math.Abs(width);
                    }
                    break;
                case GeometryType.Rectangle:
                    if (rectangle != null)
                    {
                        Point point = ReturnInsideImage(e.GetPosition(canvas));

                        double minX = Math.Min(point.X, anchorPoint.X);
                        double minY = Math.Min(point.Y, anchorPoint.Y);
                        double maxX = Math.Max(point.X, anchorPoint.X);
                        double maxY = Math.Max(point.Y, anchorPoint.Y);

                        Canvas.SetTop(rectangle, minY);
                        Canvas.SetLeft(rectangle, minX);

                        double height = maxY - minY;
                        double width = maxX - minX;

                        rectangle.Height = Math.Abs(height);
                        rectangle.Width = Math.Abs(width);
                    }
                    break;
                case GeometryType.Polygon:
                    if (polygon != null)
                    {
                        segment.Points[1] = ReturnInsideImage(e.GetPosition(canvas));
                    }
                    break;
                case GeometryType.Erase:
                case GeometryType.Edit:
                case GeometryType.None:
                default:
                    break;
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
                case GeometryType.None:
                    break;
                default:
                    break;
            }
        }
    }
}
