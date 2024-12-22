using Crystalbyte.Ribbon.UI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing;
using DrawingPoint = System.Drawing.Point;
using Media = System.Windows.Media;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using System.Globalization;
using Microsoft.Win32;
using OxyPlot.Legends;
using Shapes = System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Data;
using System.Text;
using System.Dynamic;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Windows.Media.Imaging;
using System.Text.Json;
using System.ComponentModel;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ZembryoAnalyser;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : RibbonWindow
{
    private readonly List<ResultSetHR> allHRResults;
    private readonly List<ResultSetMD> allMDResults;
    private readonly List<ResultSetET> allETResults;
    private ResultTypes resultType;
    private int bpm;
    private bool loadingComplete;
    private double videoWidth;
    private double videoHeight;
    private TimeSpan videoDuration;
    internal volatile bool canStartNewCommand;
    private string videoFileName;
    private readonly JsonSerializerOptions options;
    private readonly List<Rectangle> allRectangles;
    private readonly List<Rectangle> allRectanglesMotion;
    private readonly List<DrawingPoint> edgeContour;
    private readonly DispatcherTimer timer;

    public bool BackgroundWindowDark { get; set; }
    public bool BackgroundAppDark { get; set; }
    public AccentColorOptions AccentOptions { get; set; }
    public BackgroundColorOptions BackgroundOptions { get; set; }

    public bool VideoLoaded { get; set; }
    public bool WindowClosing { get; set; }
    public bool ShowDrawingButtons { get; set; }
    public bool ShowDataButtons { get; set; }
    public bool ShowMDButtons { get; set; }
    public bool ShowETButtons { get; set; }
    public bool CancelAction { get; set; }

    public MainWindow()
    {
        allHRResults = [];
        allMDResults = [];
        allETResults = [];
        resultType = ResultTypes.None;
        bpm = 0;
        videoDuration = default;
        loadingComplete = false;
        WindowClosing = false;
        timer = new();
        timer.Tick += Timer_Tick;

        allRectangles = [];
        allRectanglesMotion = [];
        edgeContour = [];

        InitializeComponent();

        BackgroundOptions = BackgroundColorOptions.SystemApps;
        SetBackground();
        AccentOptions = AccentColorOptions.AccentColor;

         options = new()
         {
             WriteIndented = true,
             PropertyNamingPolicy = null
         };

        _ = CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, (sender, e) =>
        {
            About r = new()
            {
                Owner = this
            };

            SetDialogBindings(r);
            _ = r.ShowDialog();
            r.Close();
        }));

        App.MainApplicationWindow = this;
    }

    public void CanRun(bool cR)
    {
        canStartNewCommand = cR;
        InvokeAction(() =>
        {
            videoSlider.IsEnabled = cR;

            videoOpenButton.IsEnabled = cR;
            videoCloseButton.IsEnabled = cR && VideoLoaded;

            autoDetectButton.IsEnabled = cR && VideoLoaded;
            calculateDataButton.IsEnabled = cR && VideoLoaded;

            autoDetectMotionButton.IsEnabled = cR && VideoLoaded;
            calculateMotionDataButton.IsEnabled = cR && VideoLoaded;

            autoDetectEdgeButton.IsEnabled = cR && VideoLoaded;
            calculateEdgeDataButton.IsEnabled = cR && VideoLoaded;

            viewVideoButton.IsEnabled = cR && VideoLoaded;

            viewBPMButton.IsEnabled = cR && ShowDataButtons;
            viewDataGridButton.IsEnabled = cR && ShowDataButtons;
            viewPlotButton.IsEnabled = cR && ShowDataButtons;

            viewMDDataButton.IsEnabled = cR && ShowMDButtons;
            viewMDImageButton.IsEnabled = cR && ShowMDButtons;
            viewMDPlotButton.IsEnabled = cR && ShowMDButtons;

            viewETDataButton.IsEnabled = cR && ShowETButtons;
            viewETPlotButton.IsEnabled = cR && ShowETButtons;

            addRectangle.IsEnabled = cR && ShowDrawingButtons;
            addEllipse.IsEnabled = cR && ShowDrawingButtons;
            addPolygon.IsEnabled = cR && ShowDrawingButtons;
            erase.IsEnabled = cR && ShowDrawingButtons;
            edit.IsEnabled = cR && ShowDrawingButtons;
            info.IsEnabled = cR && ShowDrawingButtons;
            lineMeasure.IsEnabled = cR && ShowDrawingButtons;
            polyLineMeasure.IsEnabled = cR && ShowDrawingButtons;
            rectangleMeasure.IsEnabled = cR && ShowDrawingButtons;
            ellipseMeasure.IsEnabled = cR && ShowDrawingButtons;
            polygonMeasure.IsEnabled = cR && ShowDrawingButtons;
            angleMeasure.IsEnabled = cR && ShowDrawingButtons;

            exportCSVButton.IsEnabled = cR && (ShowDataButtons || ShowMDButtons || ShowETButtons);
            exportJPGButton.IsEnabled = cR && (ShowDataButtons || ShowMDButtons || ShowETButtons);
            exportPNGButton.IsEnabled = cR && (ShowDataButtons || ShowMDButtons || ShowETButtons);
            exportSVGButton.IsEnabled = cR && (ShowDataButtons || ShowMDButtons || ShowETButtons);
            exportXLSXButton.IsEnabled = cR && (ShowDataButtons || ShowMDButtons || ShowETButtons);
            exportJSONButton.IsEnabled = cR && (ShowDataButtons || ShowMDButtons || ShowETButtons);
            exportPDFButton.IsEnabled = cR && (ShowDataButtons || ShowMDButtons || ShowETButtons);
        });
    }

    public void InvokeAction(Action action)
    {
        if (WindowClosing)
        {
            return;
        }
        else
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.Invoke(action, DispatcherPriority.Normal);
            }
        }
    }

    public void SetWaitingState(string info, TimeSpan? duration = null)
    {
        if (timer.IsEnabled)
        {
            timer.Stop();
            ReleaseWaitingState();
        }

        InvokeAction(new Action(() =>
        {
            GetStatusBar().SetColor(Media.Color.FromArgb(255, 200, 100, 0));
            SetStatusText(info);
        }));

        if (duration != null)
        {
            timer.Interval = duration.Value;
            timer.Start();
        }
    }

    public void ReleaseWaitingState()
    {
        InvokeAction(new Action(() =>
        {
            ResetStatusBarBrush();
            SetStatusText("Ready");
        }));
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        timer.Stop();
        ReleaseWaitingState();
    }

    public void ShowProgress(bool show)
    {
        statusProgressBarItem.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        statusCancelButtonItem.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    public void SetStatusText(string t)
    {
        if (timer.IsEnabled)
        {
            timer.Stop();
            ReleaseWaitingState();
        }

        statusText.Text = t;
    }

    public void SetStatusProgressValue(double value)
    {
        statusProgressBar.Value = value;
    }

    public void SetStatusProgressMaximumValue(double value)
    {
        statusProgressBar.Maximum = value;
    }

    public void WaitCancelClick(bool wait = true)
    {
        if (wait)
        {
            statusCancelButton.Click += Cancel_Click;
        }
        else
        {
            statusCancelButton.Click -= Cancel_Click;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        CancelAction = true;
    }

    public void LoadVideo(string fileName)
    {
        loadingComplete = false;
        canStartNewCommand = false;

        RemoveRectangles();

        videoFileName = fileName;

        videoDuration = VideoLibrary.GetDuration(videoFileName);

        InvokeAction(() =>
        {
            timeText.Text = $"00:00:00.000/{videoDuration.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
            filePathText.Text = fileName;
            drawingControl.canvas.Children.Clear();
            drawingControl.editCanvas.Children.Clear();
            drawingControl.Geometries.Clear();

            videoContent.Visibility = Visibility.Visible;
            dataContent.Visibility = Visibility.Collapsed;
            bpmContent.Visibility = Visibility.Collapsed;
            plotContent.Visibility = Visibility.Collapsed;
            videoSlider.Visibility = Visibility.Visible;
            videoSlider.Maximum = videoDuration.TotalMilliseconds;
            videoSlider.Value = 0;
            ShowDrawingButtons = true;

            (double width, double height) = VideoLibrary.GetWH(videoFileName);
            slika.Source = VideoLibrary.GetFirstFrame(videoFileName);

            videoWidth = width;
            videoHeight = height;

            double scale = CalculateScale(width, height);

            drawingControl.Width = width;
            drawingControl.Height = height;
            measureControl.Width = width;
            measureControl.Height = height;
            slika.Width = width;
            slika.Height = height;

            SetScaleTransform(scale);
        });

        VideoLibrary.CloseVideo();

        loadingComplete = true;
        CanRun(true);
    }

    private void SetScaleTransform(double scale)
    {
        ScaleTransform scaleTransform = new(scale, scale);

        slika.LayoutTransform = scaleTransform;
        drawingControl.LayoutTransform = scaleTransform;
        measureControl.LayoutTransform = scaleTransform;

        double inverse = 1 / scale;
        drawingControl.Scale = inverse;
        measureControl.Scale = inverse;
        VideoLibrary.Scale = inverse;
    }

    private double CalculateScale(double width, double height)
    {
        double scaleW = videoContent.ActualWidth / width;
        double scaleH = videoContent.ActualHeight / height;
        double scale = Math.Min(scaleW, scaleH);
        return scale;
    }

    public void CloseVideo()
    {
        InvokeAction(() =>
        {
            filePathText.Text = "";
            timeText.Text = "";
            slika.Source = null;
            RemoveRectangles();
            drawingControl.RemoveAllShapes();
            slika.Width = 0;
            slika.Height = 0;
            drawingControl.Width = 0;
            drawingControl.Height = 0;
            measureControl.Width = 0;
            measureControl.Height = 0;
            videoCloseButton.IsEnabled = false;

            dataContent.Visibility = Visibility.Collapsed;
            bpmContent.Visibility = Visibility.Collapsed;
            plotContent.Visibility = Visibility.Collapsed;
            
            videoSlider.Visibility = Visibility.Collapsed;
            heartRateResults.Visibility = Visibility.Collapsed;
            
            mdContent.Visibility = Visibility.Collapsed;
            mdImageContent.Visibility = Visibility.Collapsed;
            mdPlotContent.Visibility = Visibility.Collapsed;

            motionDetectionResults.Visibility = Visibility.Collapsed;

            etContent.Visibility = Visibility.Collapsed;
            etPlotContent.Visibility = Visibility.Collapsed;

            edgeTimeResults.Visibility = Visibility.Collapsed;

            autoDetectButton.IsEnabled = false;
            calculateDataButton.IsEnabled = false;
            ShowDataButtons = false;
            ShowMDButtons = false;
            ShowETButtons = false;
            ShowDrawingButtons = false;
            VideoLoaded = false;
        });
    }

    private void VideoSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (canStartNewCommand)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(videoSlider.Value);

            timeText.Text = $"{time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}/{videoDuration.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";

            ImageSource image = VideoLibrary.GetFrameFromMSec(videoFileName, (int)videoSlider.Value);

            if (image != null)
            {
                slika.Source = image;
            }
        }
    }

    public static int CompareColors(int r1, int g1, int b1, int r2, int g2, int b2)
    {
        return r1 + g1 + b1 - r2 - g2 - b2;
    }

    public void GetRegionResults()
    {
        List<List<(double result, int mSec)>> results = VideoLibrary.GetResults(videoFileName, drawingControl.Geometries, this);

        if (results != null && results.Count > 0)
        {
            resultType = ResultTypes.HeartRate;
            allHRResults.Clear();
            int i = 0;

            foreach (List<(double result, int mSec)> current in results)
            {
                if (current != null && current.Count > 0)
                {
                    string name = default;
                    SolidColorBrush color = default;

                    InvokeAction(() =>
                    {
                        Shapes.Shape el = drawingControl.Geometries.ElementAt(i++);
                        name = el.Tag.ToString();
                        color = (SolidColorBrush)el.Stroke;
                    });

                    allHRResults.Add(new ResultSetHR
                    {
                        Name = name,
                        Color = color,
                        Result = current.Select((p, i) =>
                        new HRData
                        {
                            Index = i + 1,
                            Time = ConvertToSeconds(p.mSec),
                            DataValue = p.result
                        }).ToList()
                    });
                }
            }
        }
    }

    public void DetectAutoRegion()
    {
        RemoveRectangles();
        allRectangles.AddRange(VideoLibrary.GetRectangles(videoFileName, this).OrderByDescending(p => p.Area()));
        Rectangle rect = allRectangles.FirstOrDefault();

        if (!rect.IsEmpty)
        {
            InvokeAction(() =>
            {
                drawingControl.RemoveAutoRectangle();
                drawingControl.InsertRectangle(Media.Brushes.Yellow, new SolidColorBrush(Media.Color.FromArgb(128, 255, 255, 0)), "Rectangle (Yellow)", rect.X, rect.Y, rect.Width, rect.Height);
            });
        }
    }

    public bool CalculateData()
    {
        if (allHRResults.Count > 0)
        {
            FillDataGrid(allHRResults);
            FillPlot(allHRResults);
            CalculateBPM(allHRResults);

            return true;
        }

        return false;
    }

    public bool CalculateMDData()
    {
        if (allMDResults.Count > 0)
        {
            CalculateDistance();
            DrawMDImage();
            DrawMDPlot();

            return true;
        }

        return false;
    }

    public bool CalculateETData()
    {
        if (allETResults.Count > 0)
        {
            CalculateEdgeTime();
            DrawETPlot();

            return true;
        }

        return false;
    }

    private void CalculateBPM(List<ResultSetHR> results)
    {
        List<KeyValuePair<string, int>> realBPMs = [];

        foreach (ResultSetHR setValues in results)
        {
            if (setValues != null && setValues.Result != null && setValues.Result.Count > 0)
            {
                List<double> values = setValues.Result.Select(p => p.DataValue).ToList();

                double avg = CalculateAverage(values);
                double last = values.FirstOrDefault();
                int bpmUp = 0;
                int bpmDown = 0;
                bool rising = true;

                foreach (double current in values)
                {
                    if (rising && last - current > avg)
                    {
                        bpmUp++;
                        rising = false;
                        last = current;
                    }
                    else if (rising && current > last)
                    {
                        last = current;
                    }

                    if (!rising && last - current < -avg)
                    {
                        bpmDown++;
                        rising = true;
                        last = current;
                    }
                    else if (!rising && last > current)
                    {
                        last = current;
                    }
                }

                bpm = (bpmUp + bpmDown) / 2;
                int realBPM = (int)Math.Round(bpm / videoDuration.TotalSeconds * 60);
                realBPMs.Add(new KeyValuePair<string, int>(setValues.Name, realBPM));
            }
        }

        StringBuilder sb = new();

        foreach (KeyValuePair<string, int> rbpm in realBPMs)
        {
            _ = sb.AppendLine($"Heart rate ({rbpm.Key}): {rbpm.Value} BPM");
        }

        bpmText.Text = sb.ToString();
    }

    private double CalculateAverage(List<double> list)
    {
        int listCount = list.Count;
        int count = listCount / (int)videoDuration.TotalSeconds;

        List<double> tempList = [];

        for (int i = 0; i < listCount - count; i++)
        {
            IEnumerable<double> els = list.Skip(i).Take(count);
            tempList.Add((els.Max() - els.Min()) / 2);
        }

        return tempList.Count > 0 ? tempList.Min() : 0;
    }

    private TimeSpan ConvertToSeconds(int posMSec)
    {
        double durationSeconds = videoDuration.TotalSeconds;
        return durationSeconds > 0 ? TimeSpan.FromMilliseconds(posMSec) : default;
    }

    private double ConvertToSecondsDouble(TimeSpan time)
    {
        double durationSeconds = videoDuration.TotalSeconds;
        return durationSeconds > 0 ? time.TotalSeconds : default;
    }

    private void FillDataGrid(List<ResultSetHR> data)
    {
        int elements = data.Count;

        titleGrid.ColumnDefinitions.Clear();
        dataGrid.Columns.Clear();
        titleGrid.Children.Clear();
        dataGrid.Items.Clear();

        for (int i = 0; i < elements * 3; i++)
        {
            DataGridTextColumn dgc = new() { Header = i % 3 == 0 ? "Index" : ((i % 3 == 1) ? "Time" : "Data value") };
            dataGrid.Columns.Add(dgc);
            dgc.Binding = new Binding($"Data{i}");

            ColumnDefinition columnDefinition = new();
            Binding binding = new("ActualWidth")
            {
                Source = dgc
            };
            _ = columnDefinition.SetBinding(ColumnDefinition.WidthProperty, binding);
            titleGrid.ColumnDefinitions.Add(columnDefinition);
        }

        for (int i = 0; i < elements; i++)
        {
            Border border = new()
            {
                Margin = new Thickness(2, 0, 0, 0),
                BorderBrush = (SolidColorBrush)FindResource("StandardBorderScrollBrush"),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
            };

            border.SetValue(Grid.ColumnProperty, i * 3);
            border.SetValue(Grid.ColumnSpanProperty, 3);
            _ = titleGrid.Children.Add(border);

            TextBlock textBlock = new()
            {
                Padding = new Thickness(5),
                TextAlignment = TextAlignment.Center,
                FontWeight = System.Windows.FontWeights.Bold,
                Text = data.ElementAt(i).Name
            };
            border.Child = textBlock;
        }

        int count = data.FirstOrDefault()?.Result?.Count ?? 0;

        for (int i = 0; i < count; i++)
        {
            int j = 0;
            dynamic row = new ExpandoObject();

            foreach (ResultSetHR el in data)
            {
                HRData dataRow = el.Result.ElementAt(i);

                ((IDictionary<string, object>)row)[$"Data{j * 3}"] = dataRow.Index;
                ((IDictionary<string, object>)row)[$"Data{(j * 3) + 1}"] = dataRow.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture);
                ((IDictionary<string, object>)row)[$"Data{(j * 3) + 2}"] = Math.Round(dataRow.DataValue, 2).ToString("N2", CultureInfo.InvariantCulture);
                j++;
            }

            dataGrid.Items.Add(row);
        }
    }

    private void FillPlot(List<ResultSetHR> data)
    {
        plot?.Model?.Series?.Clear();
        plot?.Model?.Axes?.Clear();

        Media.Color accentColor = default;
        Media.Color backColor = default;
        Media.Color foreColor = default;

        accentColor = AccentBrush is SolidColorBrush accentBrush ? accentBrush.Color : Colors.Crimson;
        backColor = Background is SolidColorBrush backBrush ? backBrush.Color : Colors.White;
        foreColor = Foreground is SolidColorBrush foreBrush ? foreBrush.Color : Colors.Black;

        OxyColor foreOxy = OxyColor.FromArgb(96, foreColor.R, foreColor.G, foreColor.B);

        PlotModel model = new()
        {
            Title = "Color intensity data",
            PlotType = PlotType.XY,
            Background = OxyColor.FromRgb(backColor.R, backColor.G, backColor.B),
            TextColor = OxyColor.FromRgb(foreColor.R, foreColor.G, foreColor.B),
            PlotAreaBorderColor = foreOxy,
            IsLegendVisible = true
        };

        Legend legend = new()
        {
            LegendBackground = OxyColor.FromArgb(64, 128, 128, 128),
            LegendBorder = OxyColor.FromArgb(255, 0, 0, 0),
            LegendBorderThickness = 1,
            IsLegendVisible = true,
            LegendPlacement = LegendPlacement.Inside,
            LegendPosition = LegendPosition.RightTop
        };
        model.Legends.Add(legend);

        foreach (ResultSetHR result in data)
        {
            OxyColor color = result.Color.ToOxyColor();
            OxyColor markerColor = OxyColor.FromArgb(128, color.R, color.G, color.B);

            OxyPlot.Series.LineSeries series = new()
            {
                Title = $"Color intensity - {result.Name}",
                StrokeThickness = 1,
                Color = color,
                MarkerSize = 3,
                MarkerFill = markerColor,
                MarkerType = MarkerType.Circle,
                TrackerFormatString = "{0}\n{1}:\t\t{2:0.00}\n{3}:\t{4:0.00}"
            };

            series.Points.AddRange(result.Result.Select(p => new DataPoint(Math.Round(ConvertToSecondsDouble(p.Time), 2), Math.Round(p.DataValue, 2))).ToList());
            model.Series.Add(series);
        }

        if (videoDuration.TotalSeconds > 0)
        {
            model.Axes.Add(new LinearAxis
            {
                Key = "X",
                Position = AxisPosition.Bottom,
                Title = "Time (s)",
                TitleFontSize = 12,
                TitleFontWeight = 700,
                MinorTicklineColor = foreOxy,
                TicklineColor = foreOxy
            });
        }
        else
        {
            model.Axes.Add(new LinearAxis
            {
                Key = "X",
                Position = AxisPosition.Bottom,
                Title = "Frames",
                TitleFontSize = 12,
                TitleFontWeight = 700,
                MinorTicklineColor = foreOxy,
                TicklineColor = foreOxy
            });
        }

        model.Axes.Add(new LinearAxis
        {
            Key = "Y",
            Position = AxisPosition.Left,
            Title = "Color intensity",
            TitleFontSize = 12,
            TitleFontWeight = 700,
            MinorTicklineColor = foreOxy,
            TicklineColor = foreOxy
        });

        plot.Model = model;
    }

    public void PlotColors()
    {
        Media.Color backColor = Background is SolidColorBrush backBrush ? backBrush.Color : Colors.White;
        Media.Color foreColor = Foreground is SolidColorBrush foreBrush ? foreBrush.Color : Colors.Black;

        OxyColor foreOxy = OxyColor.FromArgb(96, foreColor.R, foreColor.G, foreColor.B);

        if (plot?.Model != null)
        {
            plot.Model.Background = OxyColor.FromRgb(backColor.R, backColor.G, backColor.B);
            plot.Model.TextColor = OxyColor.FromRgb(foreColor.R, foreColor.G, foreColor.B);
            plot.Model.Legends.ToList().ForEach(p => p.LegendBorder = foreOxy);
            plot.Model.PlotAreaBorderColor = foreOxy;

            foreach (LinearAxis l in plot.Model.Axes.Cast<LinearAxis>())
            {
                l.MinorTicklineColor = foreOxy;
                l.TicklineColor = foreOxy;
            }
        }

        if (mdPlot?.Model != null)
        {
            mdPlot.Model.Background = OxyColor.FromRgb(backColor.R, backColor.G, backColor.B);
            mdPlot.Model.TextColor = OxyColor.FromRgb(foreColor.R, foreColor.G, foreColor.B);
            mdPlot.Model.Legends.ToList().ForEach(p => p.LegendBorder = foreOxy);
            mdPlot.Model.PlotAreaBorderColor = foreOxy;

            foreach (LinearAxis l in mdPlot.Model.Axes.Cast<LinearAxis>())
            {
                l.MinorTicklineColor = foreOxy;
                l.TicklineColor = foreOxy;
            }
        }

        if (etPlot?.Model != null)
        {
            etPlot.Model.Background = OxyColor.FromRgb(backColor.R, backColor.G, backColor.B);
            etPlot.Model.TextColor = OxyColor.FromRgb(foreColor.R, foreColor.G, foreColor.B);
            etPlot.Model.Legends.ToList().ForEach(p => p.LegendBorder = foreOxy);
            etPlot.Model.PlotAreaBorderColor = foreOxy;

            foreach (LinearAxis l in etPlot.Model.Axes.Cast<LinearAxis>())
            {
                l.MinorTicklineColor = foreOxy;
                l.TicklineColor = foreOxy;
            }
        }

        PlotRefresh();
    }

    public void PlotRefresh()
    {
        plot.InvalidatePlot();
        mdPlot.InvalidatePlot();
        etPlot.InvalidatePlot();
    }

    public void RemoveRectangles()
    {
        allRectangles.Clear();
        allRectanglesMotion.Clear();
        edgeContour.Clear();
        InvokeAction(drawingControl.RemoveAutoRectangle);
    }

    public void DetectMotionRegion()
    {
        RemoveRectangles();
        allRectanglesMotion.AddRange(VideoLibrary.GetMotionRectangles(videoFileName, this).OrderByDescending(p => p.Area()));
        Rectangle rect = allRectanglesMotion.FirstOrDefault();

        if (!rect.IsEmpty)
        {
            InvokeAction(() =>
            {
                drawingControl.RemoveAutoRectangle();
                drawingControl.InsertRectangle(Media.Brushes.Yellow, new SolidColorBrush(Media.Color.FromArgb(128, 255, 255, 0)), "Rectangle (Yellow)", rect.X, rect.Y, rect.Width, rect.Height);
            });
        }
    }

    public void DetectETRegion()
    {
        RemoveRectangles();
        edgeContour.AddRange(VideoLibrary.GetEdgeRectangles(videoFileName, this).Select(p => new DrawingPoint(p.X, p.Y)));

        allRectanglesMotion.AddRange(VideoLibrary.GetMotionRectangles(videoFileName, this).OrderByDescending(p => p.Area()));
        Rectangle rect = allRectanglesMotion.FirstOrDefault();

        if (!rect.IsEmpty)
        {
            InvokeAction(() =>
            {
                drawingControl.RemoveAutoRectangle();
                drawingControl.InsertPolygon(Media.Brushes.LightGreen, Media.Brushes.Transparent, "Edge", edgeContour);
                drawingControl.InsertRectangle(Media.Brushes.Yellow, new SolidColorBrush(Media.Color.FromArgb(128, 255, 255, 0)), "Rectangle (Yellow)", rect.X, rect.Y, rect.Width, rect.Height);
            });
        }
    }

    public void CalculateMotion()
    {
        List<List<(DrawingPoint dp, int mSec)>> results = VideoLibrary.GetMotionResults(videoFileName, drawingControl.Geometries, this);
        allMDResults.Clear();
        int i = 0;

        resultType = ResultTypes.MotionDetection;

        foreach (List<(DrawingPoint dp, int mSec)> current in results)
        {
            if (current != null && current.Count > 0)
            {
                string name = default;
                SolidColorBrush color = default;

                InvokeAction(() =>
                {
                    Shapes.Shape el = drawingControl.Geometries.ElementAt(i++);
                    name = el.Tag.ToString();
                    color = (SolidColorBrush)el.Stroke;
                });

                allMDResults.Add(new ResultSetMD
                {
                    Name = name,
                    Color = color,
                    Result = current.Select((p, i) =>
                    new MDData
                    {
                        Index = i + 1,
                        Time = ConvertToSeconds(p.mSec),
                        DataValue = (p.dp.X, p.dp.Y)
                    }).ToList()
                });
            }
        }

        InvokeAction(ResetRegionVisibility);
    }

    private void CalculateDistance()
    {
        StringBuilder sb = new();

        foreach (var el in allMDResults)
        {
            MDData lastResult = null;
            List<(double distance, double time)> distances = [];

            foreach (var r in el.Result)
            {
                if (lastResult != null)
                {
                    distances.Add((Math.Sqrt(Math.Pow(lastResult.DataValue.X - r.DataValue.X, 2) + Math.Pow(lastResult.DataValue.Y - r.DataValue.Y, 2)), (r.Time - lastResult.Time).TotalSeconds));
                }

                lastResult = r;
            }

            double distance = distances.Select(p => p.distance).Sum();

            var minDist = distances.Where(p => p.distance > 0).MinBy(p => p.distance);
            var maxDist = distances.MaxBy(p => p.distance);

            double minSpeed = minDist.distance / minDist.time;
            double maxSpeed = maxDist.distance / maxDist.time;

            sb.AppendLine($"Distance ({el.Name}): {distance:N2} px");
            sb.AppendLine();
            sb.AppendLine($"Average speed ({el.Name}): {distance / VideoLibrary.GetDuration(videoFileName).TotalSeconds:N2} px/s");
            sb.AppendLine();
            sb.AppendLine($"Maximal speed ({el.Name}): {maxSpeed:N2} px/s");
            sb.AppendLine();
            sb.AppendLine($"Minimal speed ({el.Name}): {minSpeed:N2} px/s");
            sb.AppendLine();
        }

        mdText.Text = sb.ToString();
    }

    private void DrawMDImage()
    {
        var image = VideoLibrary.GetFirstFrameWithMotionLines(videoFileName, allMDResults);
        mdImage.Source = image;
    }

    private void DrawMDPlot()
    {
        mdPlot?.Model?.Series?.Clear();
        mdPlot?.Model?.Axes?.Clear();
        Media.Color backColor = Background is SolidColorBrush backBrush ? backBrush.Color : Colors.White;
        Media.Color foreColor = Foreground is SolidColorBrush foreBrush ? foreBrush.Color : Colors.Black;
        OxyColor foreOxy = OxyColor.FromArgb(96, foreColor.R, foreColor.G, foreColor.B);

        PlotModel model = new()
        {
            Title = "Distance",
            PlotType = PlotType.XY,
            Background = OxyColor.FromRgb(backColor.R, backColor.G, backColor.B),
            TextColor = OxyColor.FromRgb(foreColor.R, foreColor.G, foreColor.B),
            PlotAreaBorderColor = foreOxy,
            IsLegendVisible = true
        };

        Legend legend = new()
        {
            LegendBackground = OxyColor.FromArgb(64, 128, 128, 128),
            LegendBorder = OxyColor.FromArgb(255, 0, 0, 0),
            LegendBorderThickness = 1,
            IsLegendVisible = true,
            LegendPlacement = LegendPlacement.Inside,
            LegendPosition = LegendPosition.RightTop
        };

        model.Legends.Add(legend);

        foreach (ResultSetMD result in allMDResults)
        {
            OxyColor color = result.Color.ToOxyColor();
            OxyColor markerColor = OxyColor.FromArgb(128, color.R, color.G, color.B);

            OxyPlot.Series.LineSeries series = new()
            {
                Title = $"Distance - {result.Name}",
                StrokeThickness = 1,
                Color = color,
                MarkerSize = 3,
                MarkerFill = markerColor,
                MarkerType = MarkerType.Circle,
                TrackerFormatString = "{0}\n{1}:\t\t{2:0.00}\n{3}:\t{4:0.00}"
            };

            DrawingPoint lastPoint = new();
            bool lastPointExist = false;

            foreach (var el in result.Result)
            {
                var point = new DrawingPoint((int)el.DataValue.X, (int)el.DataValue.Y);

                if (lastPointExist)
                {
                    double distance = Math.Sqrt(Math.Pow(lastPoint.X - point.X, 2) + Math.Pow(lastPoint.Y - point.Y, 2));
                    series.Points.Add(new DataPoint(Math.Round(ConvertToSecondsDouble(el.Time), 2), Math.Round(distance, 2)));
                }
                
                lastPointExist = true;
                lastPoint = point;
            }

            model.Series.Add(series);
        }

        if (videoDuration.TotalSeconds > 0)
        {
            model.Axes.Add(new LinearAxis
            {
                Key = "X",
                Position = AxisPosition.Bottom,
                Title = "Time (s)",
                TitleFontSize = 12,
                TitleFontWeight = 700,
                MinorTicklineColor = foreOxy,
                TicklineColor = foreOxy
            });
        }
        else
        {
            model.Axes.Add(new LinearAxis
            {
                Key = "X",
                Position = AxisPosition.Bottom,
                Title = "Frames",
                TitleFontSize = 12,
                TitleFontWeight = 700,
                MinorTicklineColor = foreOxy,
                TicklineColor = foreOxy
            });
        }

        model.Axes.Add(new LinearAxis
        {
            Key = "Y",
            Position = AxisPosition.Left,
            Title = "Distance (px)",
            TitleFontSize = 12,
            TitleFontWeight = 700,
            MinorTicklineColor = foreOxy,
            TicklineColor = foreOxy
        });

        mdPlot.Model = model;
    }

    public void CalculateET()
    {
        bool distanceFromEdge = false;
        int? skipFrames = null;

        InvokeAction(() =>
        {
            distanceFromEdge = saveDistanceFromEdge.IsChecked == true;

            if (skipFramesInVideo.IsChecked == true)
            {
                skipFrames = mdSkipFrames.SelectedIndex + 1;
            }
        });

        List<List<(bool closer, double minimalDistance, int mSec)>> results = VideoLibrary.GetEdgeResults(videoFileName, drawingControl.Geometries, edgeContour, distanceFromEdge, skipFrames, this);

        allETResults.Clear();
        int i = 0;

        resultType = ResultTypes.EdgeDetection;

        foreach (List<(bool dp, double md, int mSec)> current in results)
        {
            if (current != null && current.Count > 0)
            {
                string name = default;
                SolidColorBrush color = default;

                InvokeAction(() =>
                {
                    Shapes.Shape el = drawingControl.Geometries.ElementAt(i++);
                    name = el.Tag.ToString();
                    color = (SolidColorBrush)el.Stroke;
                });

                allETResults.Add(new ResultSetET
                {
                    Name = name,
                    Color = color,
                    Result = current.Select((p, i) =>
                    new ETData
                    {
                        Index = i + 1,
                        Time = ConvertToSeconds(p.mSec),
                        DataValue = p.dp,
                        MinimalDistance = distanceFromEdge ? p.md : double.NaN
                    }).ToList()
                });
            }
        }

        InvokeAction(ResetRegionVisibility);
    }

    private void CalculateEdgeTime()
    {
        StringBuilder sb = new();
        TimeSpan duration = VideoLibrary.GetDuration(videoFileName);

        foreach (var el in allETResults)
        {
            int closer = el.Result.Where(p => p.DataValue).Count();
            int further = el.Result.Where(p => !p.DataValue).Count();
            double percent = (double)closer / (closer + further);
            TimeSpan time = percent * duration;

            sb.AppendLine($"Closer to the edge then threshold ({el.Name}): {closer} frames");
            sb.AppendLine();
            sb.AppendLine($"Further away from the edge than threshold ({el.Name}): {further} frames");
            sb.AppendLine();
            sb.AppendLine($"Closer to the edge ({el.Name}) {percent * 100:N2} % of time");
            sb.AppendLine();
            sb.AppendLine($"Closer to the edge then treshold ({el.Name}): {time.TimeSpanToString()}");
            sb.AppendLine();
        }

        etText.Text = sb.ToString();
    }

    private void DrawETPlot()
    {
        bool distanceFE = saveDistanceFromEdge.IsChecked == true;
        int skipFrames = mdSkipFrames.SelectedIndex + 2;
        bool sfiv = skipFramesInVideo.IsChecked == true;

        etPlot?.Model?.Series?.Clear();
        etPlot?.Model?.Axes?.Clear();
        Media.Color backColor = Background is SolidColorBrush backBrush ? backBrush.Color : Colors.White;
        Media.Color foreColor = Foreground is SolidColorBrush foreBrush ? foreBrush.Color : Colors.Black;
        OxyColor foreOxy = OxyColor.FromArgb(96, foreColor.R, foreColor.G, foreColor.B);

        PlotModel model = new()
        {
            Title = distanceFE ? "Distance from edge" : "Closer to edge",
            PlotType = PlotType.XY,
            Background = OxyColor.FromRgb(backColor.R, backColor.G, backColor.B),
            TextColor = OxyColor.FromRgb(foreColor.R, foreColor.G, foreColor.B),
            PlotAreaBorderColor = foreOxy,
            IsLegendVisible = true
        };

        Legend legend = new()
        {
            LegendBackground = OxyColor.FromArgb(64, 128, 128, 128),
            LegendBorder = OxyColor.FromArgb(255, 0, 0, 0),
            LegendBorderThickness = 1,
            IsLegendVisible = true,
            LegendPlacement = LegendPlacement.Inside,
            LegendPosition = LegendPosition.RightTop
        };

        model.Legends.Add(legend);

        int index = 0;

        foreach (ResultSetET result in allETResults)
        {
            index++;

            if (sfiv && (index - 1) % skipFrames != 0)
            {
                continue;
            }

            OxyColor color = result.Color.ToOxyColor();
            OxyColor markerColor = OxyColor.FromArgb(128, color.R, color.G, color.B);

            OxyPlot.Series.LineSeries series = new()
            {
                Title = distanceFE ? $"Distance from edge - {result.Name}" : $"Closer to edge - {result.Name}",
                StrokeThickness = 1,
                Color = color,
                MarkerSize = 3,
                MarkerFill = markerColor,
                MarkerType = MarkerType.Circle,
                TrackerFormatString = distanceFE ? "{0}\n{1}:\t\t\t{2:0.00}\n{3}:\t{4:0.00}" : "{0}\n{1}:\t\t{2:0.00}\n{3}:\t{4:0.00}"
            };

            foreach (var el in result.Result)
            {
                series.Points.Add(new DataPoint(Math.Round(ConvertToSecondsDouble(el.Time), 2), distanceFE ? el.MinimalDistance : (el.DataValue ? 1 : 0)));
            }

            model.Series.Add(series);
        }

        if (videoDuration.TotalSeconds > 0)
        {
            model.Axes.Add(new LinearAxis
            {
                Key = "X",
                Position = AxisPosition.Bottom,
                Title = "Time (s)",
                TitleFontSize = 12,
                TitleFontWeight = 700,
                MinorTicklineColor = foreOxy,
                TicklineColor = foreOxy
            });
        }
        else
        {
            model.Axes.Add(new LinearAxis
            {
                Key = "X",
                Position = AxisPosition.Bottom,
                Title = "Frames",
                TitleFontSize = 12,
                TitleFontWeight = 700,
                MinorTicklineColor = foreOxy,
                TicklineColor = foreOxy
            });
        }

        model.Axes.Add(new LinearAxis
        {
            Key = "Y",
            Position = AxisPosition.Left,
            Title = distanceFE ? "Distance from edge" : "Closer to edge",
            TitleFontSize = 12,
            TitleFontWeight = 700,
            MinorTicklineColor = foreOxy,
            TicklineColor = foreOxy
        });

        etPlot.Model = model;
    }

    public void ExportCSV()
    {
        SaveFileDialog sfd = new()
        {
            Filter = "Csv file|*.csv"
        };

        if (sfd.ShowDialog() == true)
        {
            try
            {
                switch (resultType)
                {
                    case ResultTypes.HeartRate:
                        CsvExport.ExportCsv(sfd.FileName, allHRResults);
                        break;
                    case ResultTypes.MotionDetection:
                        CsvExport.ExportCsv(sfd.FileName, allMDResults);
                        break;
                    case ResultTypes.EdgeDetection:
                        CsvExport.ExportCsv(sfd.FileName, allETResults);
                        break;
                }
            }
            catch (Exception e)
            {
                RibbonMessageBox.Show(e.ToExceptionString(), "Unable to save CSV file");
            }
        }
    }

    public void ExportXLSX()
    {
        SaveFileDialog sfd = new()
        {
            Filter = "Excel file|*.xlsx"
        };

        if (sfd.ShowDialog() == true)
        {
            try
            {
                switch (resultType)
                {
                    case ResultTypes.HeartRate:
                        ExcelExport.ExportXLSX(sfd.FileName, allHRResults);
                        break;
                    case ResultTypes.MotionDetection:
                        ExcelExport.ExportXLSX(sfd.FileName, allMDResults);
                        break;
                    case ResultTypes.EdgeDetection:
                        ExcelExport.ExportXLSX(sfd.FileName, allETResults);
                        break;
                }
            }
            catch (Exception e)
            {
                RibbonMessageBox.Show(e.ToExceptionString(), "Unable to save Excel file");
            }
        }
    }

    public void ExportPDF()
    {
        Visibility plotVisibility = plotContent.Visibility;
        Visibility mdPlotVisibility = mdPlotContent.Visibility;
        Visibility etPlotVisibility = etPlotContent.Visibility;

        if (plotVisibility == Visibility.Collapsed)
        {
            plotContent.Visibility = Visibility.Hidden;
        }

        if (mdPlotVisibility == Visibility.Collapsed)
        {
            mdPlotContent.Visibility = Visibility.Hidden;
        }

        if (etPlotVisibility == Visibility.Collapsed)
        {
            etPlotContent.Visibility = Visibility.Hidden;
        }

        SaveFileDialog sfd = new()
        {
            Filter = "PDF file|*.pdf"
        };

        if (sfd.ShowDialog() == true)
        {
            try
            {
                switch (resultType)
                {
                    case ResultTypes.HeartRate:
                        {
                            OxyPlot.Wpf.PngExporter exporter = new()
                            {
                                Width = (int)plot.ActualWidth * 2,
                                Height = (int)plot.ActualHeight * 2
                            };

                            using MemoryStream image = new();
                            exporter.Export(plot.ActualModel, image);

                            PDFExporter.ExportPDF(sfd.FileName, allHRResults, image);
                        }
                        break;
                    case ResultTypes.MotionDetection:
                        {
                            OxyPlot.Wpf.PngExporter exporter = new()
                            {
                                Width = (int)mdPlot.ActualWidth * 2,
                                Height = (int)mdPlot.ActualHeight * 2
                            };

                            using MemoryStream image = new();
                            exporter.Export(mdPlot.ActualModel, image);

                            PDFExporter.ExportPDF(sfd.FileName, allMDResults, image);
                        }
                        break;
                    case ResultTypes.EdgeDetection:
                        {
                            OxyPlot.Wpf.PngExporter exporter = new()
                            {
                                Width = (int)etPlot.ActualWidth * 2,
                                Height = (int)etPlot.ActualHeight * 2
                            };

                            using MemoryStream image = new();
                            exporter.Export(etPlot.ActualModel, image);

                            PDFExporter.ExportPDF(sfd.FileName, allETResults, image);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                RibbonMessageBox.Show(e.ToExceptionString(), "Unable to save PDF file");
            }
        }

        plotContent.Visibility = plotVisibility;
        mdPlotContent.Visibility = mdPlotVisibility;
        etPlotContent.Visibility = etPlotVisibility;
    }

    public void ExportPNG()
    {
        Visibility plotVisibility = plotContent.Visibility;
        Visibility mdPlotVisibility = mdPlotContent.Visibility;
        Visibility etPlotVisibility = etPlotContent.Visibility;

        if (plotVisibility == Visibility.Collapsed)
        {
            plotContent.Visibility = Visibility.Hidden;
        }

        if (mdPlotVisibility == Visibility.Collapsed)
        {
            mdPlotContent.Visibility = Visibility.Hidden;
        }

        if (etPlotVisibility == Visibility.Collapsed)
        {
            etPlotContent.Visibility = Visibility.Hidden;
        }

        SaveFileDialog sfd = new()
        {
            Filter = "Png image|*.png"
        };

        if (sfd.ShowDialog() == true)
        {
            try
            {
                switch (resultType)
                {
                    case ResultTypes.HeartRate:
                        {
                            OxyPlot.Wpf.PngExporter exporter = new()
                            {
                                Width = (int)plot.ActualWidth * 2,
                                Height = (int)plot.ActualHeight * 2
                            };

                            using FileStream file = File.OpenWrite(sfd.FileName);
                            exporter.Export(plot.ActualModel, file);
                        }
                        break;
                    case ResultTypes.MotionDetection:
                        {
                            OxyPlot.Wpf.PngExporter exporter = new()
                            {
                                Width = (int)mdPlot.ActualWidth * 2,
                                Height = (int)mdPlot.ActualHeight * 2
                            };

                            using FileStream file = File.OpenWrite(sfd.FileName);
                            exporter.Export(mdPlot.ActualModel, file);
                        }
                        break;
                    case ResultTypes.EdgeDetection:
                        {
                            OxyPlot.Wpf.PngExporter exporter = new()
                            {
                                Width = (int)etPlot.ActualWidth * 2,
                                Height = (int)etPlot.ActualHeight * 2
                            };

                            using FileStream file = File.OpenWrite(sfd.FileName);
                            exporter.Export(etPlot.ActualModel, file);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                RibbonMessageBox.Show(e.ToExceptionString(), "Unable to save image");
            }
        }

        plotContent.Visibility = plotVisibility;
        mdPlotContent.Visibility = mdPlotVisibility;
        etPlotContent.Visibility = etPlotVisibility;
    }

    public void ExportJPG()
    {
        Visibility plotVisibility = plotContent.Visibility;
        Visibility mdPlotVisibility = mdPlotContent.Visibility;
        Visibility etPlotVisibility = etPlotContent.Visibility;

        if (plotVisibility == Visibility.Collapsed)
        {
            plotContent.Visibility = Visibility.Hidden;
        }

        if (mdPlotVisibility == Visibility.Collapsed)
        {
            mdPlotContent.Visibility = Visibility.Hidden;
        }

        if (etPlotVisibility == Visibility.Collapsed)
        {
            etPlotContent.Visibility = Visibility.Hidden;
        }

        SaveFileDialog sfd = new()
        {
            Filter = "Jpeg image|*.jpg;*.jpeg"
        };

        if (sfd.ShowDialog() == true)
        {
            try
            {
                switch (resultType)
                {
                    case ResultTypes.HeartRate:
                        {
                            OxyPlot.Wpf.PngExporter exporter = new()
                            {
                                Width = (int)plot.ActualWidth * 2,
                                Height = (int)plot.ActualHeight * 2
                            };

                            BitmapSource bitmap = exporter.ExportToBitmap(plot.ActualModel);

                            JpegBitmapEncoder encoder = new();
                            BitmapFrame outputFrame = BitmapFrame.Create(bitmap);
                            encoder.Frames.Add(outputFrame);

                            using FileStream file = File.OpenWrite(sfd.FileName);
                            encoder.Save(file);
                        }
                        break;
                    case ResultTypes.MotionDetection:
                        {
                            OxyPlot.Wpf.PngExporter exporter = new()
                            {
                                Width = (int)mdPlot.ActualWidth * 2,
                                Height = (int)mdPlot.ActualHeight * 2
                            };

                            BitmapSource bitmap = exporter.ExportToBitmap(mdPlot.ActualModel);

                            JpegBitmapEncoder encoder = new();
                            BitmapFrame outputFrame = BitmapFrame.Create(bitmap);
                            encoder.Frames.Add(outputFrame);

                            using FileStream file = File.OpenWrite(sfd.FileName);
                            encoder.Save(file);
                        }
                        break;
                    case ResultTypes.EdgeDetection:
                        {
                            OxyPlot.Wpf.PngExporter exporter = new()
                            {
                                Width = (int)etPlot.ActualWidth * 2,
                                Height = (int)etPlot.ActualHeight * 2
                            };

                            BitmapSource bitmap = exporter.ExportToBitmap(etPlot.ActualModel);

                            JpegBitmapEncoder encoder = new();
                            BitmapFrame outputFrame = BitmapFrame.Create(bitmap);
                            encoder.Frames.Add(outputFrame);

                            using FileStream file = File.OpenWrite(sfd.FileName);
                            encoder.Save(file);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                RibbonMessageBox.Show(e.ToExceptionString(), "Unable to save image");
            }
        }

        plotContent.Visibility = plotVisibility;
        mdPlotContent.Visibility = mdPlotVisibility;
        etPlotContent.Visibility = etPlotVisibility;
    }

    public void ExportSVG()
    {
        Visibility plotVisibility = plotContent.Visibility;
        Visibility mdPlotVisibility = mdPlotContent.Visibility;
        Visibility etPlotVisibility = etPlotContent.Visibility;

        if (plotVisibility == Visibility.Collapsed)
        {
            plotContent.Visibility = Visibility.Hidden;
        }

        if (mdPlotVisibility == Visibility.Collapsed)
        {
            mdPlotContent.Visibility = Visibility.Hidden;
        }

        if (etPlotVisibility == Visibility.Collapsed)
        {
            etPlotContent.Visibility = Visibility.Hidden;
        }

        SaveFileDialog sfd = new()
        {
            Filter = "Svg image|*.svg"
        };

        if (sfd.ShowDialog() == true)
        {
            try
            {
                switch (resultType)
                {
                    case ResultTypes.HeartRate:
                        {
                            OxyPlot.Wpf.SvgExporter exporter = new()
                            {
                                Width = plot.ActualWidth * 2,
                                Height = plot.ActualHeight * 2
                            };

                            using FileStream file = File.OpenWrite(sfd.FileName);
                            exporter.Export(plot.ActualModel, file);
                        }
                        break;
                    case ResultTypes.MotionDetection:
                        {
                            OxyPlot.Wpf.SvgExporter exporter = new()
                            {
                                Width = mdPlot.ActualWidth * 2,
                                Height = mdPlot.ActualHeight * 2
                            };

                            using FileStream file = File.OpenWrite(sfd.FileName);
                            exporter.Export(mdPlot.ActualModel, file);
                        }
                        break;
                    case ResultTypes.EdgeDetection:
                        {
                            OxyPlot.Wpf.SvgExporter exporter = new()
                            {
                                Width = etPlot.ActualWidth * 2,
                                Height = etPlot.ActualHeight * 2
                            };

                            using FileStream file = File.OpenWrite(sfd.FileName);
                            exporter.Export(etPlot.ActualModel, file);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                RibbonMessageBox.Show(e.ToExceptionString(), "Unable to save image");
            }
        }

        plotContent.Visibility = plotVisibility;
        mdPlotContent.Visibility = mdPlotVisibility;
        etPlotContent.Visibility = etPlotVisibility;
    }

    public void ExportJSON()
    {
        SaveFileDialog sfd = new()
        {
            Filter = "Json file|*.json"
        };

        if (sfd.ShowDialog() == true)
        {
            try
            {
                switch (resultType)
                {
                    case ResultTypes.HeartRate:
                        {
                            List<JsonResultSetHR> result = allHRResults.Select(p => new JsonResultSetHR
                            {
                                Name = p.Name,
                                Color = p.Color.Color.ToString(CultureInfo.InvariantCulture),
                                Data = p.Result.Select(p => new JsonDataHR
                                {
                                    Index = p.Index,
                                    Time = p.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture),
                                    Value = Math.Round(p.DataValue, 2)
                                }).ToList()
                            }).ToList();

                            string json = JsonSerializer.Serialize(result, options);
                            File.WriteAllText(sfd.FileName, json);
                        }
                        break;
                    case ResultTypes.MotionDetection:
                        {
                            List<JsonResultSetMD> result = allMDResults.Select(p => new JsonResultSetMD
                            {
                                Name = p.Name,
                                Color = p.Color.Color.ToString(CultureInfo.InvariantCulture),
                                Data = p.Result.Select(p => new JsonDataMD
                                {
                                    Index = p.Index,
                                    Time = p.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture),
                                    X = p.DataValue.X,
                                    Y = p.DataValue.Y
                                }).ToList()
                            }).ToList();

                            string json = JsonSerializer.Serialize(result, options);
                            File.WriteAllText(sfd.FileName, json);
                        }
                        break;
                    case ResultTypes.EdgeDetection:
                        {
                            List<JsonResultSetET> result = allETResults.Select(p => new JsonResultSetET
                            {
                                Name = p.Name,
                                Color = p.Color.Color.ToString(CultureInfo.InvariantCulture),
                                Data = p.Result.Select(p => new JsonDataET
                                {
                                    Index = p.Index,
                                    Time = p.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture),
                                    Value = p.DataValue
                                }).ToList()
                            }).ToList();

                            string json = JsonSerializer.Serialize(result, options);
                            File.WriteAllText(sfd.FileName, json);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                RibbonMessageBox.Show(e.ToExceptionString(), "Unable to save json file");
            }
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        SetWindowStyles(PresentationSource.FromVisual(this) as HwndSource);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

        GetThemeSettings();
        GetColorSettings();
        GetAccentColorOptions();
        GetApplicationSettings();

        loadingComplete = true;
        canStartNewCommand = true;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        WindowClosing = true;
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        GetThemeSettings();
        Media.Color? accent = WindowColors.GetAccentColor();

        if (accent.HasValue)
        {
            if (AccentOptions == AccentColorOptions.AccentColor)
            {
                AccentBrush = new SolidColorBrush(accent.Value);
                SaveAccentColor();
                AccentChanged();
            }
        }
    }

    public void SetActivatedDeactivated(RibbonWindow rw)
    {
        rw.Activated += Activated_Event;
        rw.Deactivated += Deactivated_Event;
    }

    public void UnsetActivatedDeactivated(RibbonWindow rw)
    {
        rw.Activated -= Activated_Event;
        rw.Deactivated -= Deactivated_Event;
    }

    private void Activated_Event(object sender, EventArgs e)
    {
        SetApplicationMenuBackground("AccentBrush");
        SetBorderBrush("AccentBrush");
    }

    private void Deactivated_Event(object sender, EventArgs e)
    {
        SetApplicationMenuBackground("InactiveBrush");
        SetBorderBrush("InactiveBrush");
    }

    private void EnableInactive_Checked(object sender, RoutedEventArgs e)
    {
        if (loadingComplete)
        {
            bool ei = enableInactive.IsChecked == true;

            if (ei)
            {
                SetActivatedDeactivated(this);
            }
            else
            {
                UnsetActivatedDeactivated(this);
            }

            ApplicationSettings.Settings.Set("Inactive", ei);
        }
    }

    private void GetThemeSettings()
    {
        BackgroundAppDark = WindowColors.AreAppsInDarkMode() ?? false;
        BackgroundWindowDark = WindowColors.AreWindowsInDarkMode() ?? false;

        bool darkBackground =
            BackgroundOptions == BackgroundColorOptions.Dark ||
            (BackgroundOptions == BackgroundColorOptions.SystemApps && BackgroundAppDark) ||
            (BackgroundOptions == BackgroundColorOptions.SystemWindows && BackgroundWindowDark);

        if (darkBackground)
        {
            Background = new SolidColorBrush(Media.Color.FromArgb(255, 30, 30, 30));
            Foreground = Media.Brushes.White;
            HoverBrush = new SolidColorBrush(Media.Color.FromArgb(255, 62, 62, 64));
        }
        else
        {
            Background = new SolidColorBrush(Media.Color.FromArgb(255, 214, 219, 233));
            Foreground = Media.Brushes.Black;
            HoverBrush = new SolidColorBrush(Media.Color.FromArgb(255, 192, 192, 194));
        }

        SetBackground();
    }

    private void GetColorSettings()
    {
        BackgroundOptions = ApplicationSettings.Settings.Get("BackgroundOptions").ToBackgroundOptions(BackgroundColorOptions.SystemApps);

        bool darkBackground =
            BackgroundOptions == BackgroundColorOptions.Dark ||
            (BackgroundOptions == BackgroundColorOptions.SystemApps && BackgroundAppDark) ||
            (BackgroundOptions == BackgroundColorOptions.SystemWindows && BackgroundWindowDark);

        if (!darkBackground)
        {
            Background = new SolidColorBrush(Media.Color.FromArgb(255, 214, 219, 233));
            Foreground = Media.Brushes.Black;
            HoverBrush = new SolidColorBrush(Media.Color.FromArgb(255, 192, 192, 194));
        }
        else
        {
            Background = new SolidColorBrush(Media.Color.FromArgb(255, 30, 30, 30));
            Foreground = Media.Brushes.White;
            HoverBrush = new SolidColorBrush(Media.Color.FromArgb(255, 62, 62, 64));
        }

        AccentBrush = ApplicationSettings.Settings.Get("AccentColor").ToBrush([255, 138, 43, 226]);
        AccentChanged();

        enableInactive.IsChecked = ApplicationSettings.Settings.Get("Inactive").ToBoolean(false);

        if (enableInactive.IsChecked == true)
        {
            SetActivatedDeactivated(this);
        }

        bool isCornered = ApplicationSettings.Settings.Get("CornerRadius").ToBoolean(false);
        bool isRounded = ApplicationSettings.Settings.Get("RoundedButtons").ToBoolean(false);

        if (!isCornered)
        {
            isRounded = false;
        }

        SetCornerRadius(isCornered, isRounded);
        cornerRadius.IsChecked = isCornered;
        roundedButtons.IsChecked = isRounded;


        if (ApplicationSettings.Settings.Get("TransparentChrome").ToBoolean(true))
        {
            SetStandardChrome();
            transparentChrome.IsChecked = true;
        }
        else
        {
            SetColoredChrome();
            transparentChrome.IsChecked = false;
        }

        SetBackground();
    }

    private void SetCornerRadius(bool isCornered, bool isColorButtonRounded)
    {
        Application.Current.Resources["Radius5"] = new CornerRadius(isCornered ? 5 : 0);
        Application.Current.Resources["Radius10"] = new CornerRadius(isCornered ? 10 : 0);
        Application.Current.Resources["RadiusColorButtons"] = new CornerRadius(isCornered ? (isColorButtonRounded ? 50 : 5) : 0);
        Application.Current.Resources["RadiusButton"] = new CornerRadius(isCornered ? 14 : 0);
        Application.Current.Resources["RadiusXY5"] = isCornered ? 5d : 0d;
        Application.Current.Resources["RadiusXY10"] = isCornered ? 10d : 0d;

        if (!isCornered)
        {
            roundedButtons.IsEnabled = false;
        }

        GetQuickAccessBar().QuickAccessBarCornerRadius = isCornered ? isColorButtonRounded ? 8 : 4 : 0;
    }

    private void GetAccentColorOptions()
    {
        AccentOptions = ApplicationSettings.Settings.Get("AccentColorOptions").ToAccentOptions(AccentOptions);

        if (AccentOptions == AccentColorOptions.AccentColor)
        {
            Media.Color? color = WindowColors.GetAccentColor();

            if (color.HasValue)
            {
                AccentBrush = new SolidColorBrush(color.Value);
            }

            AccentChanged();
        }
    }

    public void SetBackground()
    {
        Application.Current.Resources["BackgroundBrush"] = Background;
        Application.Current.Resources["HoverBrush"] = HoverBrush;
        Application.Current.Resources["ForegroundBrush"] = Foreground;
        WindowColors.SetInactiveColor(AccentBrush, Background);
        InactiveBrush = WindowColors.InactiveColor;
        Application.Current.Resources["InactiveBrush"] = InactiveBrush;
        Application.Current.Resources["HoverColor"] = Media.Color.FromArgb(255, 128, 128, 128);

        bool darkBackground =
            BackgroundOptions == BackgroundColorOptions.Dark ||
            (BackgroundOptions == BackgroundColorOptions.SystemApps && BackgroundAppDark) ||
            (BackgroundOptions == BackgroundColorOptions.SystemWindows && BackgroundWindowDark);

        if (darkBackground)
        {
            Application.Current.Resources["StandardBorderScrollBrush"] = new SolidColorBrush(Media.Color.FromArgb(255, 136, 136, 136));
            Application.Current.Resources["StandardScrollBrush"] = new SolidColorBrush(Media.Color.FromArgb(255, 136, 136, 136));
            Application.Current.Resources["PressedScrollBrush"] = new SolidColorBrush(Media.Color.FromArgb(255, 221, 221, 221));
            Application.Current.Resources["HoverScrollBrush"] = new SolidColorBrush(Media.Color.FromArgb(255, 170, 170, 170));
            Application.Current.Resources["ScrollBackgroundScrollBrush"] = new SolidColorBrush(Media.Color.FromArgb(255, 30, 30, 30));
            Application.Current.Resources["TextBrush"] = new SolidColorBrush(Colors.White);
            Application.Current.Resources["StandardButtonBrush"] = new SolidColorBrush(Media.Color.FromArgb(255, 69, 69, 69));
            Application.Current.Resources["HoverButtonBrush"] = new SolidColorBrush(Media.Color.FromArgb(255, 128, 128, 128));
            Application.Current.Resources["TrackerBackgroundBrush"] = new SolidColorBrush(Media.Color.FromArgb(230, 0, 0, 0));
        }
        else
        {
            Application.Current.Resources["StandardBorderScrollBrush"] = new SolidColorBrush(Colors.Gray);
            Application.Current.Resources["StandardScrollBrush"] = new SolidColorBrush(Colors.Gray);
            Application.Current.Resources["PressedScrollBrush"] = new SolidColorBrush(Media.Color.FromArgb(255, 34, 34, 34));
            Application.Current.Resources["HoverScrollBrush"] = new SolidColorBrush(Media.Color.FromArgb(255, 85, 85, 85));
            Application.Current.Resources["ScrollBackgroundScrollBrush"] = new SolidColorBrush(Media.Color.FromArgb(255, 214, 219, 233));
            Application.Current.Resources["TextBrush"] = new SolidColorBrush(Colors.Black);
            Application.Current.Resources["StandardButtonBrush"] = new SolidColorBrush(Media.Color.FromArgb(255, 168, 168, 168));
            Application.Current.Resources["HoverButtonBrush"] = new SolidColorBrush(Media.Color.FromArgb(255, 128, 128, 128));
            Application.Current.Resources["TrackerBackgroundBrush"] = new SolidColorBrush(Media.Color.FromArgb(230, 255, 255, 255));
        }

        PlotColors();
    }

    public void AccentChanged()
    {
        Application.Current.Resources["AccentBrush"] = AccentBrush;
        Resources["PressedButtonBrush"] = AccentBrush;

        WindowColors.SetInactiveColor(AccentBrush, Background);
        InactiveBrush = WindowColors.InactiveColor;
        Application.Current.Resources["InactiveBrush"] = InactiveBrush;

        if (AccentBrush is SolidColorBrush brush)
        {
            Media.Color aC = brush.Color;
            Application.Current.Resources["UnfocusedAccentColor"] = Media.Color.FromArgb(128, aC.R, aC.G, aC.B);
            Application.Current.Resources["AccentColor"] = aC;

            RibbonButtonPressedBrush = new SolidColorBrush(Media.Color.FromArgb(96, aC.R, aC.G, aC.G));
        }

        PlotColors();
    }

    private void PickColor_Click(object sender, RoutedEventArgs e)
    {
        var accentOptionsTemp = AccentOptions;
        var accentBrushTemp = AccentBrush;
        var backgroundTemp = Background;
        var backgroundOptionsTemp = ApplicationSettings.Settings.Get("BackgroundOptions").ToBackgroundOptions(BackgroundColorOptions.SystemApps);
        var hoverBrushTemp = HoverBrush;
        var foregroundTemp = Foreground;
        var inactiveBrushTemp = InactiveBrush;

        ColorDialog cd = new(this)
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        if (AccentBrush is SolidColorBrush brush)
        {
            Media.Color c = brush.Color;
            cd.SetColor(c, true);
        }

        SetDialogBindings(cd);
        bool? colorPicked = cd.ShowDialog();

        if (colorPicked == true)
        {
            AccentOptions = AccentColorOptions.Color;
            AccentBrush = cd.GetAccentBrush();
            AccentChanged();
            SaveAccentColor();
        }
        else
        {
            AccentOptions = accentOptionsTemp;
            AccentBrush = accentBrushTemp;
            Background = backgroundTemp;
            ApplicationSettings.Settings.Set("BackgroundOptions", (int)backgroundOptionsTemp);
            BackgroundOptions = backgroundOptionsTemp;
            HoverBrush = hoverBrushTemp;
            Foreground = foregroundTemp;
            InactiveBrush = inactiveBrushTemp;
            AccentChanged();
            SaveAccentColor();
            SetBackground();
        }

        cd.Close();
    }

    public void SetDialogBindings(RibbonWindow rw)
    {
        rw.SetResourceReference(AccentBrushProperty, "AccentBrush");
        rw.SetResourceReference(InactiveBrushProperty, "InactiveBrush");
        rw.SetResourceReference(BackgroundProperty, "BackgroundBrush");
        rw.SetResourceReference(ForegroundProperty, "ForegroundBrush");
        rw.SetResourceReference(HoverBrushProperty, "HoverBrush");
        rw.Resources = Resources;
        rw.Icon = Icon;

        if (ColoredChrome)
        {
            rw.SetColoredChrome();
        }
        else
        {
            rw.SetStandardChrome();
        }
    }

    public void SaveAccentColor()
    {
        if (AccentBrush is SolidColorBrush brush)
        {
            Media.Color c = brush.Color;
            ApplicationSettings.Settings.Set("AccentColor", new byte[] { c.A, c.R, c.G, c.B });
            ApplicationSettings.Settings.Set("AccentColorOptions", (int)AccentOptions);
        }
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        ApplicationSettings.Settings.Clear();

        AccentBrush = new SolidColorBrush(WindowColors.GetAccentColor() ?? Media.Color.FromArgb(255, 138, 43, 226));
        AccentOptions = AccentColorOptions.AccentColor;
        BackgroundOptions = BackgroundColorOptions.SystemApps;

        bool darkBackground =
            BackgroundOptions == BackgroundColorOptions.Dark ||
            (BackgroundOptions == BackgroundColorOptions.SystemApps && BackgroundAppDark) ||
            (BackgroundOptions == BackgroundColorOptions.SystemWindows && BackgroundWindowDark);

        if (darkBackground)
        {
            Background = new SolidColorBrush(Media.Color.FromArgb(255, 30, 30, 30));
            Foreground = Media.Brushes.White;
            HoverBrush = new SolidColorBrush(Media.Color.FromArgb(255, 62, 62, 64));
        }
        else
        {
            Background = new SolidColorBrush(Media.Color.FromArgb(255, 214, 219, 233));
            Foreground = Media.Brushes.Black;
            HoverBrush = new SolidColorBrush(Media.Color.FromArgb(255, 192, 192, 194));
        }

        enableInactive.IsChecked = false;
        SetBackground();
        AccentChanged();

        transparentChrome.IsChecked = true;
        SetStandardChrome();
        cornerRadius.IsChecked = false;
        roundedButtons.IsChecked = false;
        SetCornerRadius(false, false);
        mdSkipFrames.SelectedIndex = 0;
    }

    private void CornerRadius_Click(object sender, RoutedEventArgs e)
    {
        if (loadingComplete)
        {
            bool cr = cornerRadius.IsChecked == true;

            if (!cr)
            {
                roundedButtons.IsChecked = false;
                roundedButtons.IsEnabled = false;
            }
            else
            {
                roundedButtons.IsEnabled = true;
            }

            bool rb = roundedButtons.IsChecked == true;

            ApplicationSettings.Settings.Set("CornerRadius", cr);
            ApplicationSettings.Settings.Set("RoundedButtons", rb);

            SetCornerRadius(cr, rb);
        }
    }

    private void RoundedButtons_Click(object sender, RoutedEventArgs e)
    {
        if (loadingComplete)
        {
            bool rb = roundedButtons.IsChecked == true;
            bool cr = cornerRadius.IsChecked == true;

            ApplicationSettings.Settings.Set("RoundedButtons", rb);
            SetCornerRadius(cr, rb);
        }
    }

    private void TransparentChrome_Click(object sender, RoutedEventArgs e)
    {
        if (loadingComplete)
        {
            bool tc = transparentChrome.IsChecked == true;
            ApplicationSettings.Settings.Set("TransparentChrome", tc);

            if (tc)
            {
                SetStandardChrome();
            }
            else
            {
                SetColoredChrome();
            }
        }
    }

    private void DataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.HorizontalChange != 0)
        {
            titleGrid.Margin = new Thickness(titleGrid.Margin.Left - e.HorizontalChange, titleGrid.Margin.Top, titleGrid.Margin.Right, titleGrid.Margin.Bottom);
        }

        if (e.ViewportWidthChange != 0)
        {
            ScrollViewer scrollview = Helper.FindVisualChild<ScrollViewer>(dataGrid);

            if (scrollview != null)
            {
                int width = 0;

                if (scrollview.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                {
                    ScrollBar scroll = Helper.FindVisualChild<ScrollBar>(scrollview);

                    if (scroll != null && double.IsFinite(scroll.ActualWidth))
                    {
                        width = (int)scroll.ActualWidth;
                        width = width == 0 ? 0 : width + 1;
                    }
                }

                titleGridBehind.Margin = new Thickness(0, 0, width, 0);
                titleGrid.Margin = new Thickness(0, 0, width, 0);
            }
        }
    }

    internal void ResetApplicationAfterCrash()
    {
        SetStatusProgressValue(0);
        SetStatusProgressMaximumValue(100);
        ShowProgress(false);
        ReleaseWaitingState();
        CanRun(true);
        ResetRegionVisibility();
    }

    private void ResetRegionVisibility()
    {
        foreach (var shape in drawingControl.Geometries)
        {
            shape.Visibility = Visibility.Visible;
        }
    }

    private void Ribbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        drawingControl.GeometryType = GeometryType.None;
    }

    private void CanvasGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (loadingComplete && videoWidth > 0 && videoHeight > 0)
        {
            double scale = CalculateScale(videoWidth, videoHeight);
            SetScaleTransform(scale);
        }
    }

    private void RibbonWindow_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] fileList)
            {
                foreach (string fn in fileList)
                {
                    if (fn.ValidVideoFileName())
                    {
                        if (videoOpenButton.IsEnabled && canStartNewCommand)
                        {
                            CanRun(false);

                            filePathText.Text = "";
                            timeText.Text = "";
                            
                            if (!string.IsNullOrEmpty(fn))
                            {
                                filePathText.Text = "File in use: " + fn;

                                CloseVideo();
                                SetWaitingState("Opening document...");
                                LoadVideo(fn);
                                ReleaseWaitingState();
                                
                                VideoLoaded = true;
                            }

                            CanRun(true);
                        }

                        break;
                    }
                }
            }
        }
    }

    private void GetApplicationSettings()
    {
        var skipFramesObj = ApplicationSettings.Settings.Get("MDSkipFrames");

        if (skipFramesObj is int skipFrames)
        {
            skipFrames -= 1;
            mdSkipFrames.SelectedIndex = skipFrames;
        }

        var distanceT = ApplicationSettings.Settings.Get("ThresholdDistance");

        if (distanceT is int dT)
        {
            distanceThreshold.Value = dT;
        }

        var sfiv = ApplicationSettings.Settings.Get("SkipFramesInVideo");

        if (sfiv is bool sf)
        {
            skipFramesInVideo.IsChecked = sf;
        }

        var sdfe = ApplicationSettings.Settings.Get("SaveDistanceFromEdge");

        if (sdfe is bool sd)
        {
            saveDistanceFromEdge.IsChecked = sd;
        }
    }

    private void MDSkipFrames_SelectionChanged(object sender, EventArgs e)
    {
        if (loadingComplete)
        {
            ApplicationSettings.Settings.Set("MDSkipFrames", mdSkipFrames.SelectedIndex + 1);
        }
    }

    private void DistanceThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        distanceText.Text = ((int)distanceThreshold.Value).ToString();

        if (loadingComplete)
        {
            ApplicationSettings.Settings.Set("ThresholdDistance", (int)distanceThreshold.Value);
        }
    }

    private void SaveDistanceFromEdge_Checked(object sender, RoutedEventArgs e)
    {
        if (loadingComplete)
        {
            ApplicationSettings.Settings.Set("SaveDistanceFromEdge", saveDistanceFromEdge.IsChecked == true);
        }
    }

    private void SkipFramesInVideo_Checked(object sender, RoutedEventArgs e)
    {
        if (loadingComplete)
        {
            ApplicationSettings.Settings.Set("SkipFramesInVideo", skipFramesInVideo.IsChecked == true);
        }
    }
}
