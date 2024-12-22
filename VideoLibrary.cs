using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = OpenCvSharp.Point;
using DrawingPoint = System.Drawing.Point;
using Shape = System.Windows.Shapes.Shape;
using ShapeRectangle = System.Windows.Shapes.Rectangle;
using ShapeEllipse = System.Windows.Shapes.Ellipse;
using ShapePolygon = System.Windows.Shapes.Polygon;
using DrawingPen = System.Drawing.Pen;
using Visibility = System.Windows.Visibility;
using System.Windows.Controls;
using OpenCvSharp.Tracking;
using System.Drawing.Imaging;
using System.Globalization;

namespace ZembryoAnalyser;

public static class VideoLibrary
{
    public static double Scale { get; set; }

    private static string lastFileName;
    private static VideoCapture lastVideo;

    static VideoLibrary()
    {
        lastFileName = "";
        lastVideo = null;
    }

    private static void OpenVideo(string videoFileName)
    {
        if (lastFileName != videoFileName)
        {
            CloseVideo();

            lastFileName = videoFileName;
            lastVideo = VideoCapture.FromFile(videoFileName);
        }
    }

    public static void CloseVideo()
    {
        lastFileName = "";
        lastVideo?.Release();
        lastVideo?.Dispose();
        lastVideo = null;
    }

    public static TimeSpan GetDuration(string videoFileName)
    {
        OpenVideo(videoFileName);

        return TimeSpan.FromSeconds(lastVideo.FrameCount / lastVideo.Fps);
    }

    public static (int Width, int Height) GetWH(string videoFileName)
    {
        OpenVideo(videoFileName);

        return (Width: lastVideo.FrameWidth, Height: lastVideo.FrameHeight);
    }

    public static ImageSource GetFirstFrame(string videoFileName)
    {
        OpenVideo(videoFileName);

        using Mat frame = new();
        lastVideo.PosFrames = 0;

        if (lastVideo.Grab() && lastVideo.Retrieve(frame))
        {
            using Bitmap bitmap = frame.ToBitmap();

            var bitmapData = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            frame.Release();
            return bitmapSource;
        }
        else
        {
            frame.Release();
            return null;
        }
    }

    public static ImageSource GetFirstFrameWithMotionLines(string videoFileName, List<ResultSetMD> points)
    {
        OpenVideo(videoFileName);

        using Mat frame = new();
        lastVideo.PosFrames = 0;

        if (lastVideo.Grab() && lastVideo.Retrieve(frame))
        {
            using Bitmap bitmap = frame.ToBitmap();
            using Graphics graphics = Graphics.FromImage(bitmap);

            foreach (var element in points)
            {
                DrawingPoint lastPoint = new();
                bool lastPointExist = false;

                using var pen = new DrawingPen(element.Color.ToDrawingColor(), 2);

                foreach (var point in element.Result)
                {
                    var p = new DrawingPoint((int)point.DataValue.X, (int)point.DataValue.Y);

                    if (lastPointExist)
                    {
                        graphics.DrawLine(pen, lastPoint, p);
                    }

                    lastPoint = p;
                    lastPointExist = true;
                }
            }

            var bitmapData = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            frame.Release();
            return bitmapSource;
        }
        else
        {
            frame.Release();
            return null;
        }
    }

    public static ImageSource GetNextFrame(string videoFileName)
    {
        OpenVideo(videoFileName);

        using Mat frame = new();

        if (lastVideo.Grab() && lastVideo.Retrieve(frame))
        {
            using Bitmap bitmap = frame.ToBitmap();
            frame.Release();

            var bitmapData = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }
        else
        {
            frame.Release();
            return null;
        }
    }

    public static ImageSource GetFrameFromMSec(string videoFileName, int mSec)
    {
        OpenVideo(videoFileName);

        using Mat frame = new();
        lastVideo.PosMsec = mSec;

        if (lastVideo.Grab() && lastVideo.Retrieve(frame))
        {
            using Bitmap bitmap = frame.ToBitmap();

            var bitmapData = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            frame.Release();
            return bitmapSource;
        }
        else
        {
            frame.Release();
            return null;
        }
    }

    public static List<Rectangle> GetRectangles(string videoFileName, MainWindow main)
    {
        OpenVideo(videoFileName);

        List<Rectangle> rectangles = [];
        lastVideo.PosFrames = 0;

        using Mat frame = new();
        Mat lastFrame = new();
        Mat sum = new();

        int current = 0;

        main.InvokeAction(() =>
        {
            main.SetStatusProgressMaximumValue(lastVideo.FrameCount / lastVideo.Fps * 1000);
        });

        while (lastVideo.Grab() && lastVideo.Retrieve(frame))
        {
            if (main.CancelAction)
            {
                frame.Release();
                lastFrame.Release();
                lastFrame.Dispose();

                sum.Release();
                sum.Dispose();

                lastVideo.PosFrames = 0;

                main.InvokeAction(() =>
                {
                    main.timeText.Text = $"00:00:00.000/{GetDuration(videoFileName).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
                    main.videoSlider.Value = 0;
                    main.slika.Source = GetFirstFrame(videoFileName);
                });

                main.CancelAction = false;

                return [];
            }

            using Bitmap bitmap = frame.ToBitmap();

            main.InvokeAction(() =>
            {
                var bitmapData = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly, bitmap.PixelFormat);

                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    bitmap.HorizontalResolution, bitmap.VerticalResolution,
                    PixelFormats.Bgr24, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bitmap.UnlockBits(bitmapData);

                main.videoSlider.Value = lastVideo.PosMsec;
                main.timeText.Text = $"{TimeSpan.FromMilliseconds(lastVideo.PosMsec).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}/{TimeSpan.FromSeconds(lastVideo.FrameCount / lastVideo.Fps).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
                main.slika.Source = bitmapSource;
                main.SetStatusProgressValue(lastVideo.PosMsec);
            });

            using Mat tempFrame = new();
            using Mat result = new();

            Cv2.CvtColor(frame, tempFrame, ColorConversionCodes.RGB2GRAY);

            if (current > 0)
            {
                Cv2.Absdiff(tempFrame, lastFrame, result);

                if (sum.Empty())
                {
                    result.ConvertTo(sum, MatType.CV_64FC1);
                }
                else
                {
                    using Mat newResult = new();
                    result.ConvertTo(newResult, MatType.CV_64FC1);
                    sum = sum.Add(newResult);
                }
            }
            else
            {
                lastFrame = tempFrame.Clone();
            }

            if (current % 3 == 0)
            {
                lastFrame = tempFrame.Clone();
            }

            current++;
        }

        sum.MinMaxIdx(out double minVal, out double maxVal);
        _ = Cv2.Threshold(sum, sum, maxVal * 0.5, 255, ThresholdTypes.Binary);

        using Mat image = new();
        sum.ConvertTo(image, MatType.CV_8UC3);

        Cv2.FindContours(image, out Point[][] points, out HierarchyIndex[] index, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

        IEnumerable<Rect> boundingRects = points.Select(Cv2.BoundingRect);

        foreach (Rect br in boundingRects)
        {
            rectangles.Add(new Rectangle(br.X, br.Y, br.Width, br.Height));
        }

        frame.Release();
        lastFrame.Release();
        lastFrame.Dispose();

        sum.Release();
        sum.Dispose();

        lastVideo.PosFrames = 0;

        main.InvokeAction(() =>
        {
            main.timeText.Text = $"00:00:00.000/{GetDuration(videoFileName).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
            main.videoSlider.Value = 0;
            main.slika.Source = GetFirstFrame(videoFileName);
        });

        return rectangles;
    }

    public static List<List<(double result, int mSec)>> GetResults(string videoFileName, List<Shape> shapes, MainWindow main)
    {
        OpenVideo(videoFileName);

        List<List<(double result, int mSec)>> result = [];
        lastVideo.PosFrames = 0;

        if (shapes.Count <= 0)
        {
            return result;
        }

        using Mat image = new();
        int current = 0;
        
        List<List<int>> shapeIndexes = [];

        main.InvokeAction(() =>
        {
            main.SetStatusProgressMaximumValue(lastVideo.FrameCount / lastVideo.Fps * 1000);
        });

        foreach (Shape s in shapes)
        {
            result.Add([]);

            if (s is ShapeRectangle rc)
            {
                int x = 0, y = 0, width = 0, height = 0;
                main.InvokeAction(() =>
                {
                    x = (int)Canvas.GetLeft(rc);
                    y = (int)Canvas.GetTop(rc);
                    width = (int)rc.Width;
                    height = (int)rc.Height;
                });
                List<int> indexes = [];
                Rectangle r = new(x, y, width, height);

                for (int i = 0; i < lastVideo.FrameHeight; i++)
                {
                    for (int j = 0; j < lastVideo.FrameWidth; j++)
                    {
                        if (r.Contains(j, i))
                        {
                            indexes.Add((i * lastVideo.FrameWidth) + j);
                        }
                    }
                }

                shapeIndexes.Add(indexes);
            }
            else if (s is ShapeEllipse el)
            {
                int x = 0, y = 0, width = 0, height = 0;
                main.InvokeAction(() =>
                {
                    x = (int)Canvas.GetLeft(el);
                    y = (int)Canvas.GetTop(el);
                    width = (int)el.Width;
                    height = (int)el.Height;
                });
                List<int> indexes = [];
                Rectangle r = new(x, y, width, height);

                for (int i = 0; i < lastVideo.FrameHeight; i++)
                {
                    for (int j = 0; j < lastVideo.FrameWidth; j++)
                    {
                        if (r.IsPointInEllipse(j, i))
                        {
                            indexes.Add((i * lastVideo.FrameWidth) + j);
                        }
                    }
                }

                shapeIndexes.Add(indexes);
            }
            else if (s is ShapePolygon sp)
            {
                DrawingPoint[] polygon = default;

                main.InvokeAction(() =>
                {
                    polygon = sp.Points.Select(p => new DrawingPoint((int)p.X, (int)p.Y)).ToArray();
                });

                List<int> indexes = [];

                for (int i = 0; i < lastVideo.FrameHeight; i++)
                {
                    for (int j = 0; j < lastVideo.FrameWidth; j++)
                    {
                        if (polygon.IsPointInPolygon(j, i))
                        {
                            indexes.Add((i * lastVideo.FrameWidth) + j);
                        }
                    }
                }

                shapeIndexes.Add(indexes);
            }
        }

        int shapeCount = shapeIndexes.Count;

        while (lastVideo.Grab() && lastVideo.Retrieve(image))
        {
            if (main.CancelAction)
            {
                lastVideo.PosFrames = 0;
                image.Release();

                main.InvokeAction(() =>
                {
                    main.timeText.Text = $"00:00:00.000/{GetDuration(videoFileName).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
                    main.videoSlider.Value = 0;
                    main.slika.Source = GetFirstFrame(videoFileName);
                });

                main.CancelAction = false;

                return [];
            }

            using Bitmap bitmap = image.ToBitmap();

            main.InvokeAction(() =>
            {
                var bitmapData = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly, bitmap.PixelFormat);

                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    bitmap.HorizontalResolution, bitmap.VerticalResolution,
                    PixelFormats.Bgr24, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bitmap.UnlockBits(bitmapData);

                main.videoSlider.Value = lastVideo.PosMsec;
                main.timeText.Text = $"{TimeSpan.FromMilliseconds(lastVideo.PosMsec).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}/{TimeSpan.FromSeconds(lastVideo.FrameCount / lastVideo.Fps).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
                main.slika.Source = bitmapSource;
                main.SetStatusProgressValue(lastVideo.PosMsec);
            });

            for (int i = 0; i < shapeCount; i++)
            {
                if (image.GetArray(out Vec3b[] array))
                {
                    List<int> indexes = shapeIndexes.ElementAt(i);
                    double sum = 0;

                    foreach (int ind in indexes)
                    {
                        Vec3b c = array[ind];
                        sum += (c.Item0 + c.Item1 + c.Item2) / 3;
                    }

                    result.ElementAt(i).Add((sum / indexes.Count, lastVideo.PosMsec));
                }
            }

            current++;
        }

        lastVideo.PosFrames = 0;
        image.Release();

        main.InvokeAction(() =>
        {
            main.timeText.Text = $"00:00:00.000/{GetDuration(videoFileName).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
            main.videoSlider.Value = 0;
            main.slika.Source = GetFirstFrame(videoFileName);
        });

        return result;
    }

    public static List<Rectangle> GetMotionRectangles(string videoFileName, MainWindow main)
    {
        OpenVideo(videoFileName);

        List<Rectangle> rectangles = [];
        lastVideo.PosFrames = 0;

        int current = 0;

        using BackgroundSubtractorMOG2 backSub = BackgroundSubtractorMOG2.Create();
        using Mat kernel = Mat.Ones(MatType.CV_8UC1, [24, 24]);
        using Mat frame = new();
        using Mat tempFrame1 = new();
        using Mat tempFrame2 = new();

        main.InvokeAction(() =>
        {
            main.SetStatusProgressMaximumValue(lastVideo.FrameCount / lastVideo.Fps * 1000);
        });

        while (lastVideo.Grab() && lastVideo.Retrieve(frame))
        {
            if (main.CancelAction)
            {
                frame.Release();
                tempFrame1.Release();
                tempFrame2.Release();
                kernel.Release();

                lastVideo.PosFrames = 0;

                main.CancelAction = false;

                return [];
            }

            main.InvokeAction(() => main.SetStatusProgressValue(lastVideo.PosMsec));

            Cv2.Blur(frame, tempFrame1, new OpenCvSharp.Size(24, 24));
            backSub.Apply(tempFrame1, tempFrame2);

            Cv2.AdaptiveThreshold(tempFrame2, tempFrame1, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, 3, 1);

            Cv2.MorphologyEx(tempFrame1, tempFrame2, MorphTypes.Close, kernel);

            Cv2.FindContours(tempFrame2, out Point[][] points, out HierarchyIndex[] index, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            
            if (points.Length > 0)
            {
                IEnumerable<Rect> boundingRects = points.Select(Cv2.BoundingRect);

                foreach (Rect br in boundingRects)
                {
                    if (br.X >= 0 && br.Y >= 0 &&
                        br.Width >= 0 && br.Height >= 0 &&
                        br.X + br.Width <= frame.Cols &&
                        br.Y + br.Height <= frame.Rows)
                    {
                        rectangles.Add(new Rectangle(br.X, br.Y, br.Width, br.Height));
                    }
                }

                break;
            }

            current++;
        }

        frame.Release();
        tempFrame1.Release();
        tempFrame2.Release();
        kernel.Release();

        lastVideo.PosFrames = 0;

        return rectangles;
    }

    public static List<List<(DrawingPoint dp, int mSec)>> GetMotionResults(string videoFileName, List<Shape> shapes, MainWindow main)
    {
        OpenVideo(videoFileName);

        List<List<(DrawingPoint dp, int mSec)>> result = [];
        lastVideo.PosFrames = 0;

        if (shapes.Count <= 0)
        {
            return result;
        }
        
        List<(Tracker tracker, List<(DrawingPoint dp, int mSec)> results, Rect bbox, Scalar color)> trackers = [];

        using Mat image = new();
        using Mat tempImage = new();
        int current = 0;

        int skipFrames = 1;
        bool sfiv = false;

        main.InvokeAction(() =>
        {
            sfiv = main.skipFramesInVideo.IsChecked == true;
            skipFrames = main.mdSkipFrames.SelectedIndex + 2;
            main.SetStatusProgressMaximumValue(lastVideo.FrameCount / lastVideo.Fps * 1000);
        });

        lastVideo.PosFrames = 0;
        
        if (!lastVideo.Grab() || !lastVideo.Retrieve(image))
        {
            return result;
        }

        current++;

        foreach (Shape s in shapes)
        {
            Scalar color = Scalar.Yellow;
            Rect bbox = new();
            string name = "";

            main.InvokeAction(() =>
            {
                color = s.Stroke.GetScalarFromBrush();
                bbox = s.GetShapeBoundingBox();
                s.Visibility = Visibility.Collapsed;
                name = s.Tag.ToString();
            });

            Tracker tracker = TrackerCSRT.Create();
            tracker.Init(image, bbox);
            trackers.Add(
            (
                tracker,
                [(new DrawingPoint(bbox.X + bbox.Width / 2, bbox.Y + bbox.Height / 2), lastVideo.PosMsec)],
                bbox,
                color
            ));
        }

        Rect detectedBoundingBox = new();

        while (lastVideo.Grab() && lastVideo.Retrieve(image))
        {
            image.CopyTo(tempImage);

            if (main.CancelAction)
            {
                lastVideo.PosFrames = 0;
                image.Release();

                main.InvokeAction(() =>
                {
                    main.timeText.Text = $"00:00:00.000/{GetDuration(videoFileName).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
                    main.videoSlider.Value = 0;
                    main.slika.Source = GetFirstFrame(videoFileName);
                });

                main.CancelAction = false;

                return [];
            }

            main.InvokeAction(() => main.SetStatusProgressValue(lastVideo.PosMsec));

            current++;

            foreach ((Tracker tracker, List<(DrawingPoint dp, int mSec)> points, Rect boundingBox, Scalar boxColor) in trackers)
            {
                tracker.Update(image, ref detectedBoundingBox);

                DrawingPoint newLocation = new(detectedBoundingBox.X + detectedBoundingBox.Width / 2, detectedBoundingBox.Y + detectedBoundingBox.Height / 2);
                points.Add((newLocation, lastVideo.PosMsec));

                Cv2.Rectangle(tempImage, detectedBoundingBox, boxColor, (int)(2 * Scale));
            }

            using Bitmap bitmap = tempImage.ToBitmap();

            main.InvokeAction(() =>
            {
                var bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, bitmap.PixelFormat);
                    
                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    bitmap.HorizontalResolution, bitmap.VerticalResolution,
                    PixelFormats.Bgr24, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bitmap.UnlockBits(bitmapData);

                main.videoSlider.Value = lastVideo.PosMsec;
                main.timeText.Text = $"{TimeSpan.FromMilliseconds(lastVideo.PosMsec).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}/{TimeSpan.FromSeconds(lastVideo.FrameCount / lastVideo.Fps).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
                main.slika.Source = bitmapSource;
            });

            if (sfiv)
            {
                for (int i = 1; i < skipFrames; i++)
                {
                    lastVideo.Grab();
                }
            }
        }

        result.AddRange(trackers.Select(p => p.results.Where((p, i) => sfiv || i % skipFrames == 0).ToList()));
        
        lastVideo.PosFrames = 0;
        image.Release();
        tempImage.Release();

        main.InvokeAction(() =>
        {
            main.timeText.Text = $"00:00:00.000/{GetDuration(videoFileName).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
            main.videoSlider.Value = 0;
            main.slika.Source = GetFirstFrame(videoFileName);
        });

        return result;
    }

    private static bool EllipseLike(Point[] contour)
    {
        var rr = Cv2.FitEllipse(contour);
        double areaC = Cv2.ContourArea(contour);
        double areaE = Math.PI / 4 * rr.Size.Width * rr.Size.Height;
        return Math.Abs(areaC - areaE) < areaC * 0.1;
    }

    public static Point[] GetEdgeRectangles(string videoFileName, MainWindow main)
    {
        OpenVideo(videoFileName);

        List<Point[]> contourList = [];
        lastVideo.PosFrames = 0;

        using Mat frame = new();
        using Mat tempFrame1 = new();
        using Mat tempFrame2 = new();

        main.InvokeAction(() =>
        {
            main.SetStatusProgressMaximumValue(159);
        });

        while (lastVideo.Grab() && lastVideo.Retrieve(frame))
        {
            if (main.CancelAction)
            {
                frame.Release();
                tempFrame1.Release();
                tempFrame2.Release();

                lastVideo.PosFrames = 0;

                main.CancelAction = false;

                return [];
            }

            break;
        }

        double areaMin = frame.Width * frame.Height * 0.1;

        for (int averageColor = 96; averageColor < 255; averageColor++)
        {
            main.InvokeAction(() =>
            {
                main.SetStatusProgressValue(averageColor - 96);
            });

            Cv2.CvtColor(frame, tempFrame1, ColorConversionCodes.BGR2GRAY);
            Cv2.InRange(tempFrame1, 0, averageColor, tempFrame2);

            var se = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(24, 24));
            Cv2.MorphologyEx(tempFrame2, tempFrame2, MorphTypes.Close, se);

            Cv2.FindContours(tempFrame2, out var contours, out var hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxNone);

            foreach (var contour in contours)
            {
                double to = Cv2.ContourArea(contour);

                if (contour.Length > 5 && Cv2.ContourArea(contour) > areaMin && EllipseLike(contour))
                {
                    contourList.Add(contour);
                }
            }

            if (contourList.Count > 0)
            {
                break;
            }
        }

        frame.Release();
        tempFrame1.Release();
        tempFrame2.Release();

        lastVideo.PosFrames = 0;

        return contourList.OrderBy(p => Cv2.ContourArea(p)).FirstOrDefault();
    }

    private static (bool closer, double minimalDistance) Closer(DrawingPoint dp, List<DrawingPoint> edgeContour, int minDistance, bool md)
    {
        double minimalDistance = double.PositiveInfinity;
        bool closer = false;

        foreach (var contour in edgeContour)
        {
            double distance = Math.Sqrt(Math.Pow(contour.X - dp.X, 2) + Math.Pow(contour.Y - dp.Y, 2));

            if (md)
            {
                if (distance < minimalDistance)
                {
                    minimalDistance = distance;
                }
            }
            else
            {
                if (distance < minDistance)
                {
                    return (true, double.PositiveInfinity);
                }
            }
        }

        if (md && minimalDistance < minDistance)
        {
            closer = true;
        }

        return (closer, minimalDistance);
    }

    public static List<List<(bool closer, double minimalDistance, int mSec)>> GetEdgeResults(string videoFileName, List<Shape> shapes, List<DrawingPoint> edgeContour, bool minimalDistance, int? skipFrames, MainWindow main)
    {
        OpenVideo(videoFileName);

        List<List<(bool closer, double minimalDistance, int mSec)>> result = [];
        lastVideo.PosFrames = 0;

        if (shapes.Count <= 0)
        {
            return result;
        }

        List<(Tracker tracker, List<(bool closer, double minimalDistance, int mSec)> results, Rect bbox, Scalar color)> trackers = [];

        using Mat image = new();
        using Mat tempImage = new();
        int current = 0;

        int closerThreshold = 128;

        main.InvokeAction(() =>
        {
            main.SetStatusProgressMaximumValue(lastVideo.FrameCount / lastVideo.Fps * 1000);

            closerThreshold = (int)main.distanceThreshold.Value;
        });

        lastVideo.PosFrames = 0;

        if (!lastVideo.Grab() || !lastVideo.Retrieve(image))
        {
            return result;
        }

        current++;

        foreach (Shape s in shapes)
        {
            Scalar color = Scalar.Yellow;
            Rect bbox = new();
            string name = "";

            main.InvokeAction(() =>
            {
                color = s.Stroke.GetScalarFromBrush();
                bbox = s.GetShapeBoundingBox();
                s.Visibility = Visibility.Collapsed;
                name = s.Tag.ToString();
            });

            Tracker tracker = TrackerCSRT.Create();
            tracker.Init(image, bbox);
            trackers.Add(
            (
                tracker,
                [],
                bbox,
                color
            ));
        }

        Rect detectedBoundingBox = new();

        while (lastVideo.Grab() && lastVideo.Retrieve(image))
        {
            image.CopyTo(tempImage);

            if (main.CancelAction)
            {
                lastVideo.PosFrames = 0;
                image.Release();

                main.InvokeAction(() =>
                {
                    main.timeText.Text = $"00:00:00.000/{GetDuration(videoFileName).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
                    main.videoSlider.Value = 0;
                    main.slika.Source = GetFirstFrame(videoFileName);
                });

                main.CancelAction = false;

                return [];
            }

            main.InvokeAction(() => main.SetStatusProgressValue(lastVideo.PosMsec));

            current++;

            foreach ((Tracker tracker, List<(bool closer, double minimalDistance, int mSec)> points, Rect boundingBox, Scalar boxColor) in trackers)
            {
                tracker.Update(image, ref detectedBoundingBox);

                DrawingPoint newLocation = new(detectedBoundingBox.X + detectedBoundingBox.Width / 2, detectedBoundingBox.Y + detectedBoundingBox.Height / 2);

                (bool closer, double minimalDistanceValue) = Closer(newLocation, edgeContour, closerThreshold, minimalDistance);
                points.Add((closer, minimalDistanceValue, lastVideo.PosMsec));

                Cv2.Rectangle(tempImage, detectedBoundingBox, closer ? Scalar.Red : boxColor, (int)(2 * Scale));
            }

            using Bitmap bitmap = tempImage.ToBitmap();

            main.InvokeAction(() =>
            {
                var bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, bitmap.PixelFormat);

                var bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    bitmap.HorizontalResolution, bitmap.VerticalResolution,
                    PixelFormats.Bgr24, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bitmap.UnlockBits(bitmapData);

                main.videoSlider.Value = lastVideo.PosMsec;
                main.timeText.Text = $"{TimeSpan.FromMilliseconds(lastVideo.PosMsec).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}/{TimeSpan.FromSeconds(lastVideo.FrameCount / lastVideo.Fps).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
                main.slika.Source = bitmapSource;
            });

            if (skipFrames != null)
            {
                for (int i = 0; i < skipFrames; i++)
                {
                    lastVideo.Grab();
                }
            }
        }

        result.AddRange(trackers.Select(p => p.results));

        lastVideo.PosFrames = 0;
        image.Release();
        tempImage.Release();

        main.InvokeAction(() =>
        {
            main.timeText.Text = $"00:00:00.000/{GetDuration(videoFileName).ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
            main.videoSlider.Value = 0;
            main.slika.Source = GetFirstFrame(videoFileName);
        });

        return result;
    }
}
