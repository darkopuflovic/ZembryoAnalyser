using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ZembryoAnalyser
{
    public sealed class OpenDocumentCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public async void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoOpenButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);

                main.filePathText.Text = "";
                main.timeText.Text = "";

                var ofd = new OpenFileDialog
                {
                    Filter = "Video files | *.dat; *.wmv; *.3g2; *.3gp; *.3gp2; *.3gpp; *.amv; *.asf;  *.avi; *.bin; *.cue; *.divx; *.dv; *.flv; *.gxf; *.iso; *.m1v; *.m2v; *.m2t; *.m2ts; *.m4v; *.mkv; *.mov; *.mp2; *.mp2v; *.mp4; *.mp4v; *.mpa; *.mpe; *.mpeg; *.mpeg1; *.mpeg2; *.mpeg4; *.mpg; *.mpv2; *.mts; *.nsv; *.nuv; *.ogg; *.ogm; *.ogv; *.ogx; *.ps; *.rec; *.rm; *.rmvb; *.tod; *.ts; *.tts; *.vob; *.vro; *.webm; "
                };

                ofd.Multiselect = false;
                if (ofd.ShowDialog() == true && !string.IsNullOrEmpty(ofd.FileName))
                {
                    main.filePathText.Text = "File in use: " + ofd.FileName;

                    using var action = Task.Run(new Action(() =>
                    {
                        main.SetWaitingState("Opening document...");
                        main.LoadVideo(ofd.FileName);
                        main.ReleaseWaitingState();
                    }));
                    await action;
                    main.VideoLoaded = true;
                }

                main.CanRun(true);
            }
        }
    }

    public sealed class CloseDocumentCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);

                main.filePathText.Text = "";
                main.timeText.Text = "";
                main.slika.Source = null;
                main.RemoveRectangles();
                main.slika.Width = 0;
                main.slika.Height = 0;
                main.drawingControl.Width = 0;
                main.drawingControl.Height = 0;
                main.measureControl.Width = 0;
                main.measureControl.Height = 0;
                main.videoCloseButton.IsEnabled = false;
                main.calculateDataButton.IsEnabled = false;
                main.videoContent.Visibility = Visibility.Collapsed;
                main.dataContent.Visibility = Visibility.Collapsed;
                main.bpmContent.Visibility = Visibility.Collapsed;
                main.plotContent.Visibility = Visibility.Collapsed;
                main.videoSlider.Visibility = Visibility.Collapsed;
                main.ShowDrawingButtons = false;
                main.ShowDataButtons = false;
                main.VideoLoaded = false;
                main.CanRun(true);
            }
        }
    }

    public sealed class CalculateDataCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public async void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);

                main.SetWaitingState("Calculating...");
                main.ShowProgress(true);

                await Task.Run(() =>
                {
                    main.GetResults();
                });

                main.CalculateData();
                main.ShowDataButtons = true;
                main.ReleaseWaitingState();
                main.ShowProgress(false);
                main.CanRun(true);
            }
        }
    }

    public sealed class ViewVideoCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.videoCloseButton.IsEnabled = true;
                main.timeText.Visibility = Visibility.Visible;
                main.videoSlider.Visibility = Visibility.Visible;
                main.videoContent.Visibility = Visibility.Visible;
                main.dataContent.Visibility = Visibility.Collapsed;
                main.bpmContent.Visibility = Visibility.Collapsed;
                main.plotContent.Visibility = Visibility.Collapsed;
                main.CanRun(true);
            }
        }
    }

    public sealed class ViewBPMCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.videoCloseButton.IsEnabled = true;
                main.timeText.Visibility = Visibility.Collapsed;
                main.videoSlider.Visibility = Visibility.Collapsed;
                main.videoContent.Visibility = Visibility.Collapsed;
                main.dataContent.Visibility = Visibility.Collapsed;
                main.bpmContent.Visibility = Visibility.Visible;
                main.plotContent.Visibility = Visibility.Collapsed;
                main.CanRun(true);
            }
        }
    }

    public sealed class ViewDataCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.videoCloseButton.IsEnabled = true;
                main.timeText.Visibility = Visibility.Collapsed;
                main.videoSlider.Visibility = Visibility.Collapsed;
                main.videoContent.Visibility = Visibility.Collapsed;
                main.dataContent.Visibility = Visibility.Visible;
                main.bpmContent.Visibility = Visibility.Collapsed;
                main.plotContent.Visibility = Visibility.Collapsed;
                main.CanRun(true);
            }
        }
    }

    public sealed class ViewPlotCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.videoCloseButton.IsEnabled = true;
                main.timeText.Visibility = Visibility.Collapsed;
                main.videoSlider.Visibility = Visibility.Collapsed;
                main.videoContent.Visibility = Visibility.Collapsed;
                main.dataContent.Visibility = Visibility.Collapsed;
                main.bpmContent.Visibility = Visibility.Collapsed;
                main.plotContent.Visibility = Visibility.Visible;
                main.PlotRefresh();
                main.CanRun(true);
            }
        }
    }

    public sealed class AddRectangleCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.drawingControl.GeometryType = GeometryType.Rectangle;
                main.drawingControl.IsHitTestVisible = true;
                main.measureControl.IsHitTestVisible = false;
                main.CanRun(true);
            }
        }
    }

    public sealed class AddEllipseCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.drawingControl.GeometryType = GeometryType.Ellipse;
                main.drawingControl.IsHitTestVisible = true;
                main.measureControl.IsHitTestVisible = false;
                main.CanRun(true);
            }
        }
    }

    public sealed class AddPolygonCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.drawingControl.GeometryType = GeometryType.Polygon;
                main.drawingControl.IsHitTestVisible = true;
                main.measureControl.IsHitTestVisible = false;
                main.CanRun(true);
            }
        }
    }

    public sealed class EraseShapeCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.drawingControl.GeometryType = GeometryType.Erase;
                main.drawingControl.IsHitTestVisible = true;
                main.measureControl.IsHitTestVisible = false;
                main.CanRun(true);
            }
        }
    }

    public sealed class EditShapeCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.drawingControl.GeometryType = GeometryType.Edit;
                main.drawingControl.IsHitTestVisible = true;
                main.measureControl.IsHitTestVisible = false;
                main.CanRun(true);
            }
        }
    }

    public sealed class LineMeasureCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.measureControl.SetStatusBar((StatusBarContent)main.StatusBarItemsSource);
                main.measureControl.MeasureType = MeasureType.Line;
                main.drawingControl.IsHitTestVisible = false;
                main.measureControl.IsHitTestVisible = true;
                main.CanRun(true);
            }
        }
    }

    public sealed class PolyLineMeasureCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.measureControl.SetStatusBar((StatusBarContent)main.StatusBarItemsSource);
                main.measureControl.MeasureType = MeasureType.Polyline;
                main.drawingControl.IsHitTestVisible = false;
                main.measureControl.IsHitTestVisible = true;
                main.CanRun(true);
            }
        }
    }

    public sealed class RectangleMeasureCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.measureControl.SetStatusBar((StatusBarContent)main.StatusBarItemsSource);
                main.measureControl.MeasureType = MeasureType.Rectangle;
                main.drawingControl.IsHitTestVisible = false;
                main.measureControl.IsHitTestVisible = true;
                main.CanRun(true);
            }
        }
    }

    public sealed class EllipseMeasureCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.measureControl.SetStatusBar((StatusBarContent)main.StatusBarItemsSource);
                main.measureControl.MeasureType = MeasureType.Ellipse;
                main.drawingControl.IsHitTestVisible = false;
                main.measureControl.IsHitTestVisible = true;
                main.CanRun(true);
            }
        }
    }

    public sealed class PolygonMeasureCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.measureControl.SetStatusBar((StatusBarContent)main.StatusBarItemsSource);
                main.measureControl.MeasureType = MeasureType.Polygon;
                main.drawingControl.IsHitTestVisible = false;
                main.measureControl.IsHitTestVisible = true;
                main.CanRun(true);
            }
        }
    }

    public sealed class ExportCSVCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.ExportCSV();
                main.CanRun(true);
            }
        }
    }

    public sealed class ExportXLSXCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.ExportXLSX();
                main.CanRun(true);
            }
        }
    }

    public sealed class ExportPNGCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.ExportPNG();
                main.CanRun(true);
            }
        }
    }

    public sealed class ExportJPGCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.ExportJPG();
                main.CanRun(true);
            }
        }
    }

    public sealed class ExportSVGCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.ExportSVG();
                main.CanRun(true);
            }
        }
    }

    public sealed class ExportJSONCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void Execute(object parameter)
        {
            var main = (MainWindow)Application.Current.MainWindow;

            if (main.videoCloseButton.IsEnabled && main.canStartNewCommand)
            {
                main.CanRun(false);
                main.ExportJSON();
                main.CanRun(true);
            }
        }
    }
}
