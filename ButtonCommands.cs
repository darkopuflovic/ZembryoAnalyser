using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ZembryoAnalyser;

public sealed class OpenDocumentCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public async void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoOpenButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);

            App.MainApplicationWindow.filePathText.Text = "";
            App.MainApplicationWindow.timeText.Text = "";

            OpenFileDialog ofd = new()
            {
                Filter = "Video files | *.dat; *.wmv; *.3g2; *.3gp; *.3gp2; *.3gpp; *.amv; *.asf;  *.avi; *.bin; *.cue; *.divx; *.dv; *.flv; *.gxf; *.iso; *.m1v; *.m2v; *.m2t; *.m2ts; *.m4v; *.mkv; *.mov; *.mp2; *.mp2v; *.mp4; *.mp4v; *.mpa; *.mpe; *.mpeg; *.mpeg1; *.mpeg2; *.mpeg4; *.mpg; *.mpv2; *.mts; *.nsv; *.nuv; *.ogg; *.ogm; *.ogv; *.ogx; *.ps; *.rec; *.rm; *.rmvb; *.tod; *.ts; *.tts; *.vob; *.vro; *.webm; ",
                Multiselect = false
            };

            if (ofd.ShowDialog() == true && !string.IsNullOrEmpty(ofd.FileName))
            {
                App.MainApplicationWindow.filePathText.Text = "File in use: " + ofd.FileName;

                using Task action = Task.Run(new Action(() =>
                {
                    App.MainApplicationWindow.CloseVideo();
                    App.MainApplicationWindow.SetWaitingState("Opening document...");
                    App.MainApplicationWindow.LoadVideo(ofd.FileName);
                    App.MainApplicationWindow.ReleaseWaitingState();
                }));
                await action;
                App.MainApplicationWindow.VideoLoaded = true;
            }

            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class CloseDocumentCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.CloseVideo();
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class AutoDetectRegionCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public async void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);

            App.MainApplicationWindow.SetWaitingState("Detecting...");
            App.MainApplicationWindow.ShowProgress(true);
            App.MainApplicationWindow.WaitCancelClick(true);

            await Task.Run(App.MainApplicationWindow.DetectAutoRegion);

            App.MainApplicationWindow.ReleaseWaitingState();
            App.MainApplicationWindow.WaitCancelClick(false);
            App.MainApplicationWindow.ShowProgress(false);
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class CalculateDataCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public async void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);

            App.MainApplicationWindow.SetWaitingState("Calculating...");
            App.MainApplicationWindow.ShowProgress(true);
            App.MainApplicationWindow.WaitCancelClick(true);

            await Task.Run(App.MainApplicationWindow.GetRegionResults);

            bool dataAvailable = App.MainApplicationWindow.CalculateData();
            App.MainApplicationWindow.ShowDataButtons = dataAvailable;
            App.MainApplicationWindow.ShowMDButtons = false;
            App.MainApplicationWindow.ShowETButtons = false;
            App.MainApplicationWindow.heartRateResults.Visibility = dataAvailable ? Visibility.Visible : Visibility.Collapsed;
            App.MainApplicationWindow.motionDetectionResults.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.edgeTimeResults.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.ReleaseWaitingState();
            App.MainApplicationWindow.WaitCancelClick(false);
            App.MainApplicationWindow.ShowProgress(false);
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class AutoDetectMotionCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public async void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);

            App.MainApplicationWindow.SetWaitingState("Detecting...");
            App.MainApplicationWindow.ShowProgress(true);
            App.MainApplicationWindow.WaitCancelClick(true);

            await Task.Run(App.MainApplicationWindow.DetectMotionRegion);

            App.MainApplicationWindow.ReleaseWaitingState();
            App.MainApplicationWindow.WaitCancelClick(false);
            App.MainApplicationWindow.ShowProgress(false);
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class CalculateMotionCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public async void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);

            App.MainApplicationWindow.SetWaitingState("Calculating...");
            App.MainApplicationWindow.ShowProgress(true);
            App.MainApplicationWindow.WaitCancelClick(true);

            await Task.Run(App.MainApplicationWindow.CalculateMotion);

            bool dataAvailable = App.MainApplicationWindow.CalculateMDData();
            App.MainApplicationWindow.ShowDataButtons = false;
            App.MainApplicationWindow.ShowETButtons = false;
            App.MainApplicationWindow.ShowMDButtons = dataAvailable;
            App.MainApplicationWindow.motionDetectionResults.Visibility = dataAvailable ? Visibility.Visible : Visibility.Collapsed;
            App.MainApplicationWindow.heartRateResults.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.edgeTimeResults.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.ReleaseWaitingState();
            App.MainApplicationWindow.WaitCancelClick(false);
            App.MainApplicationWindow.ShowProgress(false);
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class AutoDetectEdgeCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public async void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);

            App.MainApplicationWindow.SetWaitingState("Detecting...");
            App.MainApplicationWindow.ShowProgress(true);
            App.MainApplicationWindow.WaitCancelClick(true);

            await Task.Run(App.MainApplicationWindow.DetectETRegion);

            App.MainApplicationWindow.ReleaseWaitingState();
            App.MainApplicationWindow.WaitCancelClick(false);
            App.MainApplicationWindow.ShowProgress(false);
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class CalculateEdgeCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public async void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);

            App.MainApplicationWindow.SetWaitingState("Calculating...");
            App.MainApplicationWindow.ShowProgress(true);
            App.MainApplicationWindow.WaitCancelClick(true);

            await Task.Run(App.MainApplicationWindow.CalculateET);

            bool dataAvailable = App.MainApplicationWindow.CalculateETData();
            App.MainApplicationWindow.ShowDataButtons = false;
            App.MainApplicationWindow.ShowMDButtons = false;
            App.MainApplicationWindow.ShowETButtons = dataAvailable;
            App.MainApplicationWindow.edgeTimeResults.Visibility = dataAvailable ? Visibility.Visible : Visibility.Collapsed;
            App.MainApplicationWindow.heartRateResults.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.motionDetectionResults.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.ReleaseWaitingState();
            App.MainApplicationWindow.WaitCancelClick(false);
            App.MainApplicationWindow.ShowProgress(false);
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ViewVideoCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.videoCloseButton.IsEnabled = true;
            App.MainApplicationWindow.timeText.Visibility = Visibility.Visible;
            App.MainApplicationWindow.videoSlider.Visibility = Visibility.Visible;
            App.MainApplicationWindow.videoContent.Visibility = Visibility.Visible;

            App.MainApplicationWindow.dataContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.bpmContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.plotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.mdContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdImageContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.etContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.etPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ViewBPMCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.videoCloseButton.IsEnabled = true;
            App.MainApplicationWindow.timeText.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoSlider.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.dataContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.bpmContent.Visibility = Visibility.Visible;
            App.MainApplicationWindow.plotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.mdContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdImageContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.etContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.etPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ViewMDDataCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.videoCloseButton.IsEnabled = true;
            App.MainApplicationWindow.timeText.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoSlider.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoContent.Visibility = Visibility.Collapsed;
            
            App.MainApplicationWindow.dataContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.bpmContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.plotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.mdContent.Visibility = Visibility.Visible;
            App.MainApplicationWindow.mdImageContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.etContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.etPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ViewETDataCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.videoCloseButton.IsEnabled = true;
            App.MainApplicationWindow.timeText.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoSlider.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.dataContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.bpmContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.plotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.mdContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdImageContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.etContent.Visibility = Visibility.Visible;
            App.MainApplicationWindow.etPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ViewDataCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.videoCloseButton.IsEnabled = true;
            App.MainApplicationWindow.timeText.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoSlider.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.dataContent.Visibility = Visibility.Visible;
            App.MainApplicationWindow.bpmContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.plotContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ViewMDImageCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.videoCloseButton.IsEnabled = true;
            App.MainApplicationWindow.timeText.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoSlider.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.dataContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.bpmContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.plotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.mdContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdImageContent.Visibility = Visibility.Visible;
            App.MainApplicationWindow.mdPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.etContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.etPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ViewPlotCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.videoCloseButton.IsEnabled = true;
            App.MainApplicationWindow.timeText.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoSlider.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.dataContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.bpmContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.plotContent.Visibility = Visibility.Visible;

            App.MainApplicationWindow.mdContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdImageContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.etContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.etPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.PlotRefresh();
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ViewMDPlotCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.videoCloseButton.IsEnabled = true;
            App.MainApplicationWindow.timeText.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoSlider.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.dataContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.bpmContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.plotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.mdContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdImageContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdPlotContent.Visibility = Visibility.Visible;

            App.MainApplicationWindow.etContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.etPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.PlotRefresh();
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ViewETPlotCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.videoCloseButton.IsEnabled = true;
            App.MainApplicationWindow.timeText.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoSlider.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.videoContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.dataContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.bpmContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.plotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.mdContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdImageContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.mdPlotContent.Visibility = Visibility.Collapsed;

            App.MainApplicationWindow.etContent.Visibility = Visibility.Collapsed;
            App.MainApplicationWindow.etPlotContent.Visibility = Visibility.Visible;

            App.MainApplicationWindow.PlotRefresh();
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class AddRectangleCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.drawingControl.GeometryType = GeometryType.Rectangle;
            App.MainApplicationWindow.drawingControl.IsHitTestVisible = true;
            App.MainApplicationWindow.measureControl.IsHitTestVisible = false;
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class AddEllipseCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.drawingControl.GeometryType = GeometryType.Ellipse;
            App.MainApplicationWindow.drawingControl.IsHitTestVisible = true;
            App.MainApplicationWindow.measureControl.IsHitTestVisible = false;
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class AddPolygonCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.drawingControl.GeometryType = GeometryType.Polygon;
            App.MainApplicationWindow.drawingControl.IsHitTestVisible = true;
            App.MainApplicationWindow.measureControl.IsHitTestVisible = false;
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class EraseShapeCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.drawingControl.GeometryType = GeometryType.Erase;
            App.MainApplicationWindow.drawingControl.IsHitTestVisible = true;
            App.MainApplicationWindow.measureControl.IsHitTestVisible = false;
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class EditShapeCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.drawingControl.GeometryType = GeometryType.Edit;
            App.MainApplicationWindow.drawingControl.IsHitTestVisible = true;
            App.MainApplicationWindow.measureControl.IsHitTestVisible = false;
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class InfoShapeCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.drawingControl.GeometryType = GeometryType.Info;
            App.MainApplicationWindow.drawingControl.IsHitTestVisible = true;
            App.MainApplicationWindow.measureControl.IsHitTestVisible = false;
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class LineMeasureCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.measureControl.SetMainWindow(App.MainApplicationWindow);
            App.MainApplicationWindow.measureControl.MeasureType = MeasureType.Line;
            App.MainApplicationWindow.drawingControl.IsHitTestVisible = false;
            App.MainApplicationWindow.measureControl.IsHitTestVisible = true;
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class PolyLineMeasureCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.measureControl.SetMainWindow(App.MainApplicationWindow);
            App.MainApplicationWindow.measureControl.MeasureType = MeasureType.Polyline;
            App.MainApplicationWindow.drawingControl.IsHitTestVisible = false;
            App.MainApplicationWindow.measureControl.IsHitTestVisible = true;
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class RectangleMeasureCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.measureControl.SetMainWindow(App.MainApplicationWindow);
            App.MainApplicationWindow.measureControl.MeasureType = MeasureType.Rectangle;
            App.MainApplicationWindow.drawingControl.IsHitTestVisible = false;
            App.MainApplicationWindow.measureControl.IsHitTestVisible = true;
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class EllipseMeasureCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.measureControl.SetMainWindow(App.MainApplicationWindow);
            App.MainApplicationWindow.measureControl.MeasureType = MeasureType.Ellipse;
            App.MainApplicationWindow.drawingControl.IsHitTestVisible = false;
            App.MainApplicationWindow.measureControl.IsHitTestVisible = true;
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class PolygonMeasureCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.measureControl.SetMainWindow(App.MainApplicationWindow);
            App.MainApplicationWindow.measureControl.MeasureType = MeasureType.Polygon;
            App.MainApplicationWindow.drawingControl.IsHitTestVisible = false;
            App.MainApplicationWindow.measureControl.IsHitTestVisible = true;
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class AngleMeasureCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.measureControl.SetMainWindow(App.MainApplicationWindow);
            App.MainApplicationWindow.measureControl.MeasureType = MeasureType.Angle;
            App.MainApplicationWindow.drawingControl.IsHitTestVisible = false;
            App.MainApplicationWindow.measureControl.IsHitTestVisible = true;
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ExportCSVCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.ExportCSV();
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ExportXLSXCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.ExportXLSX();
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ExportPNGCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.ExportPNG();
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ExportJPGCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.ExportJPG();
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ExportSVGCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.ExportSVG();
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ExportJSONCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.ExportJSON();
            App.MainApplicationWindow.CanRun(true);
        }
    }
}

public sealed class ExportPDFCommand : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Execute(object parameter)
    {
        if (App.MainApplicationWindow != null &&
            App.MainApplicationWindow.videoCloseButton.IsEnabled &&
            App.MainApplicationWindow.canStartNewCommand)
        {
            App.MainApplicationWindow.CanRun(false);
            App.MainApplicationWindow.ExportPDF();
            App.MainApplicationWindow.CanRun(true);
        }
    }
}
