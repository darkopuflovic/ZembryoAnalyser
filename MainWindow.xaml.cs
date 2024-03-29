﻿using Crystalbyte.Ribbon.UI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing;
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
using System.Threading.Tasks;
using System.Text.Json;
using System.ComponentModel;
using System.Windows.Interop;

namespace ZembryoAnalyser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        private readonly List<ResultSet> allResults;
        private int bpm;
        private bool loadingComplete;
        private TimeSpan videoDuration;
        private int numberOfFrames;
        private int realNumberOfFrames;
        internal volatile bool canStartNewCommand;
        private string videoFileName;
        private readonly List<Rectangle> allRectangles;

        public bool BackgroundWindowDark { get; set; }
        public bool BackgroundAppDark { get; set; }
        public AccentColorOptions AccentOptions { get; set; }
        public BackgroundColorOptions BackgroundOptions { get; set; }

        public bool VideoLoaded { get; set; }
        public bool WindowClosing { get; set; }
        public bool ShowDrawingButtons { get; set; }
        public bool ShowDataButtons { get; set; }

        public MainWindow()
        {
            allResults = new List<ResultSet>();
            bpm = 0;
            videoDuration = default;
            numberOfFrames = 0;
            realNumberOfFrames = 0;
            loadingComplete = false;
            WindowClosing = false;

            allRectangles = new List<Rectangle>();

            InitializeComponent();

            BackgroundOptions = BackgroundColorOptions.SystemApps;
            SetBackground();
            AccentOptions = AccentColorOptions.AccentColor;

            _ = CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, (sender, e) =>
            {
                About r = new()
                {
                    Owner = this
                };

                SetDialogBindings(r);
                SetActivatedDeactivated(r);
                _ = r.ShowDialog();
                r.Close();
                UnsetActivatedDeactivated(r);
            }));
        }

        public void CanRun(bool cR)
        {
            canStartNewCommand = cR;
            InvokeAction(() =>
            {
                videoSlider.IsEnabled = cR;
                videoOpenButton.IsEnabled = cR;
                videoCloseButton.IsEnabled = cR && VideoLoaded;
                calculateDataButton.IsEnabled = cR && VideoLoaded;
                viewVideoButton.IsEnabled = cR && ShowDataButtons;
                viewBPMButton.IsEnabled = cR && ShowDataButtons;
                viewDataGridButton.IsEnabled = cR && ShowDataButtons;
                viewPlotButton.IsEnabled = cR && ShowDataButtons;
                addRectangle.IsEnabled = cR && ShowDrawingButtons;
                addEllipse.IsEnabled = cR && ShowDrawingButtons;
                addPolygon.IsEnabled = cR && ShowDrawingButtons;
                erase.IsEnabled = cR && ShowDrawingButtons;
                edit.IsEnabled = cR && ShowDrawingButtons;
                lineMeasure.IsEnabled = cR && ShowDrawingButtons;
                polyLineMeasure.IsEnabled = cR && ShowDrawingButtons;
                rectangleMeasure.IsEnabled = cR && ShowDrawingButtons;
                ellipseMeasure.IsEnabled = cR && ShowDrawingButtons;
                polygonMeasure.IsEnabled = cR && ShowDrawingButtons;
                angleMeasure.IsEnabled = cR && ShowDrawingButtons;
                exportCSVButton.IsEnabled = cR && ShowDataButtons;
                exportJPGButton.IsEnabled = cR && ShowDataButtons;
                exportPNGButton.IsEnabled = cR && ShowDataButtons;
                exportSVGButton.IsEnabled = cR && ShowDataButtons;
                exportXLSXButton.IsEnabled = cR && ShowDataButtons;
                exportJSONButton.IsEnabled = cR && ShowDataButtons;
                exportPDFButton.IsEnabled = cR && ShowDataButtons;
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
                Dispatcher.Invoke(() => action());
            }
        }

        public void SetWaitingState(string info)
        {
            InvokeAction(new Action(() =>
            {
                GetStatusBar().SetColor(Media.Color.FromArgb(255, 200, 100, 0));
                ((StatusBarContent)StatusBarItemsSource).SetText(info);
            }));
        }

        public void ReleaseWaitingState()
        {
            InvokeAction(new Action(() =>
            {
                ResetStatusBarBrush();
                ((StatusBarContent)StatusBarItemsSource).SetText("Ready");
            }));
        }

        public void ShowZoom(bool show)
        {
            StatusBarContent status = (StatusBarContent)StatusBarItemsSource;
            status.SetSliderVisibility(show);

            if (show)
            {
                status.SetSliderBackground(Background);
            }
        }

        public void ShowProgress(bool show)
        {
            StatusBarContent status = (StatusBarContent)StatusBarItemsSource;
            status.SetProgressVisibility(show);

            if (show)
            {
                status.SetProgressBackground(Background);
                status.MoveProgress(ActualWidth);
            }
        }

        public void SetProgressValue(double value)
        {
            ((StatusBarContent)StatusBarItemsSource).SetProgress(value);
        }

        public void SetNumberOfFrames(int frames)
        {
            numberOfFrames = frames;
        }

        public void SetNumberOfFrames(int frames, int realFrames)
        {
            numberOfFrames = frames;
            realNumberOfFrames = realFrames;
        }

        public void LoadVideo(string fileName)
        {
            loadingComplete = false;
            canStartNewCommand = false;

            RemoveRectangles();

            videoFileName = fileName;

            videoDuration = VideoLibrary.GetDuration(videoFileName);

            bool detect = true;

            InvokeAction(() =>
            {
                timeText.Text = $"00:00:00.000/{videoDuration.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";
                drawingControl.canvas.Children.Clear();
                drawingControl.editCanvas.Children.Clear();
                drawingControl.Geometries.Clear();
                detect = regionDetection.IsChecked == true;

                SetNumberOfFrames(VideoLibrary.GetFrameCount(videoFileName));
                dataContent.Visibility = Visibility.Collapsed;
                bpmContent.Visibility = Visibility.Collapsed;
                plotContent.Visibility = Visibility.Collapsed;
                videoContent.Visibility = Visibility.Visible;
                videoSlider.Visibility = Visibility.Visible;
                videoSlider.Maximum = numberOfFrames;
                videoSlider.Value = 0;
                ShowDrawingButtons = true;

                (double width, double height) = VideoLibrary.GetWH(videoFileName);
                slika.Source = VideoLibrary.GetFirstFrame(videoFileName);

                drawingControl.Width = width;
                drawingControl.Height = height;
                measureControl.Width = width;
                measureControl.Height = height;
                slika.Width = width;
                slika.Height = height;
            });

            if (detect)
            {
                CalculateRectangleOnLoad();
            }

            VideoLibrary.CloseVideo();

            loadingComplete = true;
            CanRun(true);
        }

        public void CloseVideo()
        {
            InvokeAction(() =>
            {
                filePathText.Text = "";
                timeText.Text = "";
                slika.Source = null;
                RemoveRectangles();
                slika.Width = 0;
                slika.Height = 0;
                drawingControl.Width = 0;
                drawingControl.Height = 0;
                measureControl.Width = 0;
                measureControl.Height = 0;
                videoCloseButton.IsEnabled = false;
                calculateDataButton.IsEnabled = false;
                videoContent.Visibility = Visibility.Collapsed;
                dataContent.Visibility = Visibility.Collapsed;
                bpmContent.Visibility = Visibility.Collapsed;
                plotContent.Visibility = Visibility.Collapsed;
                videoSlider.Visibility = Visibility.Collapsed;
                ShowDrawingButtons = false;
                ShowDataButtons = false;
                VideoLoaded = false;
            });
        }

        private void VideoSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int frame = (int)videoSlider.Value;
            int maxFrames = (int)videoSlider.Maximum;

            double percent = (double)frame / maxFrames;
            TimeSpan time = TimeSpan.FromMilliseconds(videoDuration.TotalMilliseconds * percent);

            timeText.Text = $"{time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}/{videoDuration.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)}";

            ImageSource image = VideoLibrary.GetFrame(videoFileName, frame);

            if (image != null)
            {
                slika.Source = image;
            }
        }

        public static int CompareColors(int r1, int g1, int b1, int r2, int g2, int b2)
        {
            return r1 + g1 + b1 - r2 - g2 - b2;
        }

        public void GetResults()
        {
            List<List<double>> results = VideoLibrary.GetResults(videoFileName, drawingControl.Geometries, this, realNumberOfFrames);

            if (results != null && results.Count > 0)
            {
                allResults.Clear();
                int i = 0;

                foreach (List<double> current in results)
                {
                    if (current != null && current.Count > 0)
                    {
                        int numOfFrames = realNumberOfFrames <= 0 ? numberOfFrames : realNumberOfFrames;
                        string name = default;
                        SolidColorBrush color = default;

                        InvokeAction(() =>
                        {
                            Shapes.Shape el = drawingControl.Geometries.ElementAt(i++);
                            name = el.Tag.ToString();
                            color = (SolidColorBrush)el.Stroke;
                        });

                        allResults.Add(new ResultSet
                        {
                            Name = name,
                            Color = color,
                            Result = current.Select((p, i) =>
                            new Data
                            {
                                Index = i + 1,
                                Time = ConvertToSeconds(i + 1, numOfFrames),
                                DataValue = p
                            }).ToList()
                        });
                    }
                }
            }
        }

        private void CalculateRectangleOnLoad()
        {
            InvokeAction(() =>
            {
                ShowProgress(true);
            });

            allRectangles.AddRange(VideoLibrary.GetRectangles(videoFileName, this).OrderByDescending(p => p.Area()));
            Rectangle rect = allRectangles.FirstOrDefault();

            if (!rect.IsEmpty)
            {
                InvokeAction(() =>
                {
                    drawingControl.InsertRectangle(Media.Brushes.Yellow, new SolidColorBrush(Media.Color.FromArgb(128, 255, 255, 0)), "Rectangle (Yellow)", rect.X, rect.Y, rect.Width, rect.Height);
                });
            }

            InvokeAction(() =>
            {
                ShowProgress(false);
            });
        }

        public void CalculateData()
        {
            FillDataGrid(allResults);
            FillPlot(allResults);
            CalculateBPM(allResults);
        }

        private void CalculateBPM(List<ResultSet> results)
        {
            List<KeyValuePair<string, int>> realBPMs = new();

            foreach (ResultSet setValues in results)
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

            List<double> tempList = new();

            for (int i = 0; i < listCount - count; i++)
            {
                IEnumerable<double> els = list.Skip(i).Take(count);
                tempList.Add((els.Max() - els.Min()) / 2);
            }

            return tempList.Count > 0 ? tempList.Min() : 0;
        }

        private TimeSpan ConvertToSeconds(int frame, int totalFrames)
        {
            double durationSeconds = videoDuration.TotalSeconds;

            return durationSeconds > 0 ? TimeSpan.FromSeconds((double)frame / totalFrames * durationSeconds) : default;
        }

        private double ConvertToSecondsDouble(int frame, int totalFrames)
        {
            double durationSeconds = videoDuration.TotalSeconds;

            return durationSeconds > 0 ? (double)frame / totalFrames * durationSeconds : frame;
        }

        private void FillDataGrid(List<ResultSet> data)
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

                foreach (ResultSet el in data)
                {
                    Data dataRow = el.Result.ElementAt(i);

                    ((IDictionary<string, object>)row)[$"Data{j * 3}"] = dataRow.Index;
                    ((IDictionary<string, object>)row)[$"Data{(j * 3) + 1}"] = dataRow.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture);
                    ((IDictionary<string, object>)row)[$"Data{(j * 3) + 2}"] = Math.Round(dataRow.DataValue, 2).ToString("N2", CultureInfo.InvariantCulture);
                    j++;
                }

                dataGrid.Items.Add(row);
            }
        }

        private void FillPlot(List<ResultSet> data)
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

            foreach (ResultSet result in data)
            {
                OxyColor color = result.Color.ToOxyColor();
                OxyColor markerColor = OxyColor.FromArgb(128, color.R, color.G, color.B);

                OxyPlot.Series.LineSeries series = new()
                {
                    Title = $"Color intensity ({result.Name})",
                    StrokeThickness = 1,
                    Color = color,
                    MarkerSize = 3,
                    MarkerFill = markerColor,
                    MarkerType = MarkerType.Circle,
                    TrackerFormatString = "{0}\n{1}:\t\t{2:0.00}\n{3}:\t{4:0.00}"
                };

                int numOfFrames = realNumberOfFrames <= 0 ? numberOfFrames : realNumberOfFrames;

                series.Points.AddRange(result.Result.Select(p => new DataPoint(Math.Round(ConvertToSecondsDouble(p.Index, numOfFrames), 2), Math.Round(p.DataValue, 2))).ToList());
                model.Series.Add(series);
            }

            if (videoDuration.TotalSeconds > 0)
            {
                model.Axes.Add(new LinearAxis
                {
                    Key = "X",
                    Position = AxisPosition.Bottom,
                    Title = "Seconds",
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
            if (plot?.Model == null)
            {
                return;
            }

            Media.Color backColor = Background is SolidColorBrush backBrush ? backBrush.Color : Colors.White;
            Media.Color foreColor = Foreground is SolidColorBrush foreBrush ? foreBrush.Color : Colors.Black;

            OxyColor foreOxy = OxyColor.FromArgb(96, foreColor.R, foreColor.G, foreColor.B);

            plot.Model.Background = OxyColor.FromRgb(backColor.R, backColor.G, backColor.B);
            plot.Model.TextColor = OxyColor.FromRgb(foreColor.R, foreColor.G, foreColor.B);
            plot.Model.Legends.ToList().ForEach(p => p.LegendBorder = foreOxy);
            plot.Model.PlotAreaBorderColor = foreOxy;

            foreach (LinearAxis l in plot.Model.Axes.Cast<LinearAxis>())
            {
                l.MinorTicklineColor = foreOxy;
                l.TicklineColor = foreOxy;
            }

            PlotRefresh();
        }

        public void PlotRefresh()
        {
            plot.InvalidatePlot();
        }

        public void RemoveRectangles()
        {
            allRectangles.Clear();
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
                    CsvExport.ExportCsv(sfd.FileName, allResults);
                }
                catch (Exception e)
                {
                    ErrorMessage($"Unable to save CSV file. (Message: {e.Message})");
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
                    ExcelExport.ExportXLSX(sfd.FileName, allResults);
                }
                catch (Exception e)
                {
                    ErrorMessage($"Unable to save Excel file. (Message: {e.Message})");
                }
            }
        }

        public void ExportPDF()
        {
            Visibility visibility = plotContent.Visibility;

            if (visibility == Visibility.Collapsed)
            {
                plotContent.Visibility = Visibility.Hidden;
            }

            SaveFileDialog sfd = new()
            {
                Filter = "PDF file|*.pdf"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    OxyPlot.Wpf.PngExporter exporter = new()
                    {
                        Width = (int)plot.ActualWidth * 2,
                        Height = (int)plot.ActualHeight * 2
                    };

                    using MemoryStream image = new();
                    exporter.Export(plot.ActualModel, image);

                    PDFExporter.ExportPDF(sfd.FileName, allResults, image);
                }
                catch (Exception e)
                {
                    ErrorMessage($"Unable to save PDF file. (Message: {e.Message})");
                }
            }

            plotContent.Visibility = visibility;
        }

        public void ExportPNG()
        {
            Visibility visibility = plotContent.Visibility;

            if (visibility == Visibility.Collapsed)
            {
                plotContent.Visibility = Visibility.Hidden;
            }

            SaveFileDialog sfd = new()
            {
                Filter = "Png image|*.png"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    OxyPlot.Wpf.PngExporter exporter = new()
                    {
                        Width = (int)plot.ActualWidth * 2,
                        Height = (int)plot.ActualHeight * 2
                    };

                    using FileStream file = File.OpenWrite(sfd.FileName);
                    exporter.Export(plot.ActualModel, file);
                }
                catch (Exception e)
                {
                    ErrorMessage($"Unable to save image. (Message: {e.Message})");
                }
            }

            plotContent.Visibility = visibility;
        }

        public void ExportJPG()
        {
            Visibility visibility = plotContent.Visibility;

            if (visibility == Visibility.Collapsed)
            {
                plotContent.Visibility = Visibility.Hidden;
            }

            SaveFileDialog sfd = new()
            {
                Filter = "Jpeg image|*.jpg;*.jpeg"
            };

            if (sfd.ShowDialog() == true)
            {
                try
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
                catch (Exception e)
                {
                    ErrorMessage($"Unable to save image. (Message: {e.Message})");
                }
            }

            plotContent.Visibility = visibility;
        }

        public void ExportSVG()
        {
            Visibility visibility = plotContent.Visibility;

            if (visibility == Visibility.Collapsed)
            {
                plotContent.Visibility = Visibility.Hidden;
            }

            SaveFileDialog sfd = new()
            {
                Filter = "Svg image|*.svg"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    OxyPlot.Wpf.SvgExporter exporter = new()
                    {
                        Width = plot.ActualWidth * 2,
                        Height = plot.ActualHeight * 2
                    };

                    using FileStream file = File.OpenWrite(sfd.FileName);
                    exporter.Export(plot.ActualModel, file);
                }
                catch (Exception e)
                {
                    ErrorMessage($"Unable to save image. (Message: {e.Message})");
                }
            }

            plotContent.Visibility = visibility;
        }

        public void ExportJSON()
        {
            SaveFileDialog sfd = new()
            {
                Filter = "Json file|*.json"
            };

            if (sfd.ShowDialog() == true)
            {
                JsonSerializerOptions options = new()
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = null
                };

                try
                {
                    List<JsonResultSet> result = allResults.Select(p => new JsonResultSet
                    {
                        Name = p.Name,
                        Color = p.Color.Color.ToString(CultureInfo.InvariantCulture),
                        Data = p.Result.Select(p => new JsonData
                        {
                            Index = p.Index,
                            Time = p.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture),
                            Value = Math.Round(p.DataValue, 2)
                        }).ToList()
                    }).ToList();

                    string json = JsonSerializer.Serialize(result, options);
                    File.WriteAllText(sfd.FileName, json);
                }
                catch (Exception e)
                {
                    ErrorMessage($"Unable to save json file. (Message: {e.Message})");
                }
            }
        }

        private void ErrorMessage(string message)
        {
            _ = Task.Run(async () =>
            {
                SetWaitingState(message);
                await Task.Delay(2000);
                ReleaseWaitingState();
            });
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

        private void Deactivated_Event(object sender, EventArgs e)
        {
            SetApplicationMenuBackground("InactiveBrush");
            SetBorderBrush("InactiveBrush");
        }

        private void Activated_Event(object sender, EventArgs e)
        {
            SetApplicationMenuBackground("AccentBrush");
            SetBorderBrush("AccentBrush");
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

        private void RibbonWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((StatusBarContent)StatusBarItemsSource).MoveSlider(e.NewSize.Width);
            ((StatusBarContent)StatusBarItemsSource).MoveProgress(e.NewSize.Width);
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

            AccentBrush = ApplicationSettings.Settings.Get("AccentColor").ToBrush(new byte[] { 255, 138, 43, 226 });
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

            regionDetection.IsChecked = ApplicationSettings.Settings.Get("RegionDetection").ToBoolean(true);
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
            SetActivatedDeactivated(cd);
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
            UnsetActivatedDeactivated(cd);
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
            regionDetection.IsChecked = true;
            SetStandardChrome();
            cornerRadius.IsChecked = false;
            roundedButtons.IsChecked = false;
            SetCornerRadius(false, false);
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

        private void RegionDetection_Click(object sender, RoutedEventArgs e)
        {
            if (loadingComplete)
            {
                ApplicationSettings.Settings.Set("RegionDetection", regionDetection.IsChecked == true);
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

        private void Ribbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            drawingControl.GeometryType = GeometryType.None;
        }
    }
}
