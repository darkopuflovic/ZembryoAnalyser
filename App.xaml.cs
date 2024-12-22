using System.Threading.Tasks;
using System;
using System.Windows;

namespace ZembryoAnalyser;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static MainWindow MainApplicationWindow { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        SetupExceptionHandling();
    }

    private void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e != null && e.ExceptionObject != null)
            {
                Exception ex = e.ExceptionObject as Exception;
                string exceptionString = ex?.ToExceptionString() ?? "Empty exception!";
                RibbonMessageBox.Show(exceptionString, ex?.GetType()?.Name ?? "UnhandledException", "Consolas");
            }
            else
            {
                RibbonMessageBox.Show("Unhandled exception!", "UnhandledException", "Consolas");
            }

            if (MainApplicationWindow != null)
            {
                MainApplicationWindow.ResetApplicationAfterCrash();
            }
            else
            {
                Current.Shutdown();
            }
        };

        DispatcherUnhandledException += (s, e) =>
        {
            string exceptionString = e?.Exception?.ToExceptionString() ?? "Empty exception!";
            RibbonMessageBox.Show(exceptionString, e?.Exception?.GetType()?.Name ?? "DispatcherUnhandledException", "Consolas");
            e.Handled = true;

            if (MainApplicationWindow != null)
            {
                MainApplicationWindow.ResetApplicationAfterCrash();
            }
            else
            {
                Current.Shutdown();
            }
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            string exceptionString = e?.Exception?.ToExceptionString() ?? "Empty exception!";
            RibbonMessageBox.Show(exceptionString, e?.Exception?.GetType()?.Name ?? "UnobservedTaskException", "Consolas");
            e.SetObserved();

            if (MainApplicationWindow != null)
            {
                MainApplicationWindow.ResetApplicationAfterCrash();
            }
            else
            {
                Current.Shutdown();
            }
        };
    }
}
