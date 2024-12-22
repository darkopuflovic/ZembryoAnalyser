using Crystalbyte.Ribbon.UI;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace ZembryoAnalyser;

/// <summary>
/// Interaction logic for RibbonMessageBox.xaml
/// </summary>
public partial class RibbonMessageBox : RibbonWindow
{
    public RibbonMessageBox()
    {
        InitializeComponent();
        MaxHeight = SystemParameters.WorkArea.Height;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        SetWindowStyles(PresentationSource.FromVisual(this) as HwndSource);
    }

    public static void Show(string message, string title, string fontFamily = "")
    {
        RibbonMessageBox instance = new()
        {
            Title = title
        };

        if (!string.IsNullOrWhiteSpace(fontFamily))
        {
            instance.message.FontFamily = new FontFamily(fontFamily);
        }

        instance.Owner = App.MainApplicationWindow;
        App.MainApplicationWindow?.SetDialogBindings(instance);

        instance.Width = MeasureStringWidth(message, instance.message) + 60;
        instance.message.Text = message;
        instance.ShowDialog();
        instance.Close();
    }

    private static double MeasureStringWidth(string text, TextBlock textBlock)
    {
        var formattedText = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
            textBlock.FontSize,
            textBlock.Foreground,
            new NumberSubstitution(),
            VisualTreeHelper.GetDpi(textBlock).PixelsPerDip);

        return formattedText.Width;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        Close();
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
