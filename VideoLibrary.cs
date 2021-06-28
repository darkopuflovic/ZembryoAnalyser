using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = OpenCvSharp.Point;
using Shape = System.Windows.Shapes.Shape;
using ShapeRectangle = System.Windows.Shapes.Rectangle;
using ShapeEllipse = System.Windows.Shapes.Ellipse;
using ShapePolygon = System.Windows.Shapes.Polygon;
using System.Windows.Controls;

namespace ZembryoAnalyser
{
    public static class VideoLibrary
    {
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

        public static int GetFrameCount(string videoFileName)
        {
            OpenVideo(videoFileName);

            return lastVideo.FrameCount;
        }

        public static (int Width, int Height) GetWH(string videoFileName)
        {
            OpenVideo(videoFileName);

            return (Width: lastVideo.FrameWidth, Height: lastVideo.FrameHeight);
        }

        public static ImageSource GetFirstFrame(string videoFileName)
        {
            OpenVideo(videoFileName);

            using var frame = new Mat();

            if (lastVideo.Grab() && lastVideo.Retrieve(frame))
            {
                using var bit = frame.ToBitmap();
                IntPtr hbit = bit.GetHbitmap();

                BitmapSource bs = Imaging.CreateBitmapSourceFromHBitmap(
                        hbit,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromWidthAndHeight(bit.Width, bit.Height));

                _ = Interop.DeleteObject(hbit);

                return bs;
            }
            else
            {
                return null;
            }
        }

        public static Bitmap GetNextFrame(string videoFileName)
        {
            OpenVideo(videoFileName);

            using var frame = new Mat();

            if (lastVideo.Grab() && lastVideo.Retrieve(frame))
            {
                using var bit = frame.ToBitmap();
                return bit;
            }
            else
            {
                return null;
            }
        }

        public static ImageSource GetFrame(string videoFileName, int index)
        {
            OpenVideo(videoFileName);

            using var frame = new Mat();
            lastVideo.PosFrames = index - 1;

            if (lastVideo.Grab() && lastVideo.Retrieve(frame))
            {
                using var bit = frame.ToBitmap();
                IntPtr hbit = bit.GetHbitmap();

                BitmapSource bs = Imaging.CreateBitmapSourceFromHBitmap(
                        hbit,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromWidthAndHeight(bit.Width, bit.Height));

                _ = Interop.DeleteObject(hbit);

                return bs;
            }
            else
            {
                return null;
            }
        }

        public static List<Rectangle> GetRectangles(string videoFileName, MainWindow main)
        {
            OpenVideo(videoFileName);

            var rectangles = new List<Rectangle>();

            using var frame = new Mat();
            var lastFrame = new Mat();
            var sum = new Mat();

            int max = lastVideo.FrameCount;
            int current = 0;

            while (lastVideo.Grab() && lastVideo.Retrieve(frame))
            {
                main.Dispatcher.Invoke(() =>
                {
                    main.SetProgressValue((double)current / max * 100);
                });

                using var tempFrame = new Mat();
                using var result = new Mat();

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
                        using var newResult = new Mat();
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

            using var image = new Mat();
            sum.ConvertTo(image, MatType.CV_8UC3);

            Cv2.FindContours(image, out Point[][] points, out HierarchyIndex[] index, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            IEnumerable<OpenCvSharp.Rect> boundingRects = points.Select(p => Cv2.BoundingRect(p));

            foreach (OpenCvSharp.Rect br in boundingRects)
            {
                rectangles.Add(new Rectangle(br.X, br.Y, br.Width, br.Height));
            }

            lastFrame.Release();
            lastFrame.Dispose();

            sum.Release();
            sum.Dispose();

            main.SetNumberOfFrames(max, current + 1);
            lastVideo.PosFrames = 0;

            return rectangles;
        }

        public static List<List<double>> GetResults(string videoFileName, List<Shape> shapes, MainWindow main, int max = 0)
        {
            OpenVideo(videoFileName);

            var result = new List<List<double>>();

            if (shapes.Count <= 0)
            {
                return result;
            }

            using var image = new Mat();
            int current = 0;
            int maxFrames = max > 0 ? max : lastVideo.FrameCount;

            var shapeIndexes = new List<List<int>>();

            foreach (Shape s in shapes)
            {
                result.Add(new List<double>());

                if (s is ShapeRectangle rc)
                {
                    int x = 0, y = 0, width = 0, height = 0;
                    main.Dispatcher.Invoke(() =>
                    {
                        x = (int)Canvas.GetLeft(rc);
                        y = (int)Canvas.GetTop(rc);
                        width = (int)rc.Width;
                        height = (int)rc.Height;
                    });
                    List<int> indexes = new List<int>();
                    var r = new Rectangle(x, y, width, height);

                    for (int i = 0; i < lastVideo.FrameHeight; i++)
                    {
                        for (int j = 0; j < lastVideo.FrameWidth; j++)
                        {
                            if (r.Contains(j, i))
                            {
                                indexes.Add(i * lastVideo.FrameWidth + j);
                            }
                        }
                    }

                    shapeIndexes.Add(indexes);
                }
                else if (s is ShapeEllipse el)
                {
                    int x = 0, y = 0, width = 0, height = 0;
                    main.Dispatcher.Invoke(() =>
                    {
                        x = (int)Canvas.GetLeft(el);
                        y = (int)Canvas.GetTop(el);
                        width = (int)el.Width;
                        height = (int)el.Height;
                    });
                    List<int> indexes = new List<int>();
                    var r = new Rectangle(x, y, width, height);

                    for (int i = 0; i < lastVideo.FrameHeight; i++)
                    {
                        for (int j = 0; j < lastVideo.FrameWidth; j++)
                        {
                            if (r.IsPointInEllipse(j, i))
                            {
                                indexes.Add(i * lastVideo.FrameWidth + j);
                            }
                        }
                    }

                    shapeIndexes.Add(indexes);
                }
                else if (s is ShapePolygon sp)
                {
                    System.Drawing.Point[] polygon = default;

                    main.Dispatcher.Invoke(() =>
                    {
                        polygon = sp.Points.Select(p => new System.Drawing.Point((int)p.X, (int)p.Y)).ToArray();
                    });

                    List<int> indexes = new List<int>();

                    for (int i = 0; i < lastVideo.FrameHeight; i++)
                    {
                        for (int j = 0; j < lastVideo.FrameWidth; j++)
                        {
                            if (polygon.IsPointInPolygon(j, i))
                            {
                                indexes.Add(i * lastVideo.FrameWidth + j);
                            }
                        }
                    }

                    shapeIndexes.Add(indexes);
                }
            }

            int shapeCount = shapeIndexes.Count;

            while (lastVideo.Grab() && lastVideo.Retrieve(image))
            {
                main.Dispatcher.Invoke(() =>
                {
                    main.SetProgressValue((double)current / maxFrames * 100);
                });

                for (int i = 0; i < shapeCount; i++)
                {
                    if (image.GetArray<Vec3b>(out var array))
                    {
                        var indexes = shapeIndexes.ElementAt(i);
                        double sum = 0;

                        foreach (var ind in indexes)
                        {
                            var c = array[ind];
                            sum += ((c.Item0 + c.Item1 + c.Item2) / 3);
                        }

                        result.ElementAt(i).Add(sum / indexes.Count);
                    }
                }

                current++;
            }

            if (max <= 0)
            {
                main.SetNumberOfFrames(lastVideo.FrameCount, current + 1);
            }

            lastVideo.PosFrames = 0;

            return result;
        }
    }
}
