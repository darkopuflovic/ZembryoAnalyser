using Crystalbyte.Ribbon.UI;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Navigation;

namespace ZembryoAnalyser;

/// <summary>
/// Interaction logic for About.xaml
/// </summary>
public partial class About : RibbonWindow
{
    public string Year { get; set; }

    public About()
    {
        Year = $"2019-{DateTime.Now.Year}.";
        InitializeComponent();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        SetWindowStyles(PresentationSource.FromVisual(this) as HwndSource);
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ProcessStartInfo info = new(e.Uri.AbsoluteUri)
        {
            UseShellExecute = true
        };
        using Process _ = Process.Start(info);
        e.Handled = true;
    }

    private void RibbonWindow_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}
