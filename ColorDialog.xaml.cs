using Crystalbyte.UI;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System;
using System.Globalization;
using Microsoft.Win32;

namespace ZembryoAnalyser
{
    /// <summary>
    /// Interaction logic for ColorDialog.xaml
    /// </summary>
    public partial class ColorDialog : RibbonWindow
    {
        private static readonly List<DefinedColor> definedColors;
        private static readonly List<ColorListStructure> allDefinedColors;
        private static readonly int windowsVersion;
        private bool clicked;
        private readonly bool loaded;
        private readonly MainWindow main;

        static ColorDialog()
        {
            windowsVersion = Environment.OSVersion.Version.Major;

            definedColors = new List<DefinedColor>();
            allDefinedColors = new List<ColorListStructure>();

            var colors = typeof(Colors).GetProperties().Select(p => p.Name).ToList();

            int i = 0;

            foreach (string s in colors)
            {
                var c = (Color)ColorConverter.ConvertFromString(s);
                definedColors.Add(new DefinedColor { Index = i, A = c.A, B = c.B, G = c.G, R = c.R });
                var cls = new ColorListStructure { Name = s, Color = new SolidColorBrush(c), HexText = c.ToString(CultureInfo.InvariantCulture), RGBText = $"A: {c.A}    R: {c.R}    G: {c.G}    B: {c.B}" };
                allDefinedColors.Add(cls);
                i++;
            }
        }

        public ColorDialog(MainWindow mainWindow)
        {
            main = mainWindow;
            InitializeComponent();

            colorPicker.ColorChanged += (p, q) =>
            {
                main.AccentOptions = AccentColorOptions.Color;
                AccentBrush = colorPicker.selectedColor.Background;
                main.AccentBrush = colorPicker.selectedColor.Background;
                main.AccentChanged();
                main.SaveAccentColor();
            };

            clicked = false;
            loaded = false;

            colorList.ItemsSource = allDefinedColors;

            colorPicker.alphaSlider.ValueChanged += Slider_ValueChanged;
            colorPicker.redSlider.ValueChanged += Slider_ValueChanged;
            colorPicker.greenSlider.ValueChanged += Slider_ValueChanged;
            colorPicker.blueSlider.ValueChanged += Slider_ValueChanged;
            loaded = true;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            byte a = (byte)colorPicker.alphaSlider.Value;
            byte r = (byte)colorPicker.redSlider.Value;
            byte g = (byte)colorPicker.greenSlider.Value;
            byte b = (byte)colorPicker.blueSlider.Value;

            IEnumerable<DefinedColor> all = definedColors.Where(p => p.A == a && p.R == r && p.G == g && p.B == b);

            if (all.Any())
            {
                if (!clicked)
                {
                    colorList.SelectedIndex = all.FirstOrDefault().Index;
                    colorList.ScrollIntoView(colorList.SelectedItem);
                }
                else
                {
                    colorList.SelectedIndex = all.FirstOrDefault().Index;
                }
            }
            else
            {
                colorList.SelectedIndex = -1;
            }

            clicked = false;
        }

        public Brush GetAccentBrush() =>
            colorPicker.GetSelectedBrush();

        public void SetColor(Color color) =>
            colorPicker.SetColor(color);

        public void SetColor(Color color, bool automatically) =>
            colorPicker.SetColor(color, automatically);

        private void ColorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            clicked = true;
            var list = (ColorListStructure)colorList.SelectedItem;

            if (list != null && loaded)
            {
                colorPicker.SetColor(list.Color.Color);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
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

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            themeApp.Visibility = WindowColors.AreAppsInDarkMode() != null ? Visibility.Visible : Visibility.Collapsed;
            themeWindow.Visibility = WindowColors.AreWindowsInDarkMode() != null ? Visibility.Visible : Visibility.Collapsed;

            SetDarkButtons();
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

            if (windowsVersion >= 7)
            {
                Color? accent = WindowColors.GetAccentColor();

                if (accent.HasValue)
                {
                    var brush = new SolidColorBrush(accent.Value);
                    colorizationColor.Visibility = Visibility.Visible;
                    colorizationColor.Background = brush;
                }
            }

            BackgroundColorOptions backgroundOptions = ApplicationSettings.Settings.Get("BackgroundOptions").ToBackgroundOptions(BackgroundColorOptions.SystemApps);

            bool darkBackground =
                main.BackgroundOptions == BackgroundColorOptions.Dark ||
                (main.BackgroundOptions == BackgroundColorOptions.SystemApps && main.BackgroundAppDark) ||
                (main.BackgroundOptions == BackgroundColorOptions.SystemWindows && main.BackgroundWindowDark);

            PopulateColors(IsAppDark(), darkBackground);

            SetColor(((SolidColorBrush)AccentBrush).Color, true);
        }

        private bool? IsAppDark() =>
            main.BackgroundOptions switch
            {
                BackgroundColorOptions.SystemApps => true,
                BackgroundColorOptions.SystemWindows => false,
                BackgroundColorOptions.Light => null,
                BackgroundColorOptions.Dark => null,
                _ => null
            };

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            SetDarkButtons();
            Color? accent = WindowColors.GetAccentColor();

            if (accent.HasValue)
            {
                colorizationColor.Background = new SolidColorBrush(accent.Value);

                if (main.AccentOptions == AccentColorOptions.AccentColor)
                {
                    SetColor(accent.Value, true);
                }
            }
        }

        private void SetDarkButtons()
        {
            themeApp.Background = (WindowColors.AreAppsInDarkMode() ?? false) ? dark.Background : light.Background;
            themeWindow.Background = (WindowColors.AreWindowsInDarkMode() ?? false) ? dark.Background : light.Background;
        }

        private void PopulateColors(bool? appsDark, bool darkBackground)
        {
            if (appsDark == true)
            {
                col1.Background = new SolidColorBrush(Color.FromArgb(255, 255, 185, 0));
                col2.Background = new SolidColorBrush(Color.FromArgb(255, 218, 59, 1));
                col3.Background = new SolidColorBrush(Color.FromArgb(255, 231, 72, 86));
                col4.Background = new SolidColorBrush(Color.FromArgb(255, 227, 0, 140));
                col5.Background = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
                col6.Background = new SolidColorBrush(Color.FromArgb(255, 0, 99, 177));
                col7.Background = new SolidColorBrush(Color.FromArgb(255, 107, 105, 214));
                col8.Background = new SolidColorBrush(Color.FromArgb(255, 136, 23, 152));
            }
            else if (appsDark == false)
            {
                col1.Background = new SolidColorBrush(Color.FromArgb(255, 0, 153, 188));
                col2.Background = new SolidColorBrush(Color.FromArgb(255, 0, 183, 195));
                col3.Background = new SolidColorBrush(Color.FromArgb(255, 0, 178, 148));
                col4.Background = new SolidColorBrush(Color.FromArgb(255, 0, 204, 106));
                col5.Background = new SolidColorBrush(Color.FromArgb(255, 122, 117, 116));
                col6.Background = new SolidColorBrush(Color.FromArgb(255, 104, 118, 138));
                col7.Background = new SolidColorBrush(Color.FromArgb(255, 73, 130, 5));
                col8.Background = new SolidColorBrush(Color.FromArgb(255, 132, 117, 69));
            }
            else if (darkBackground)
            {
                col1.Background = new SolidColorBrush(Color.FromArgb(255, 138, 43, 226));
                col2.Background = new SolidColorBrush(Color.FromArgb(255, 100, 149, 237));
                col3.Background = new SolidColorBrush(Color.FromArgb(255, 220, 20, 60));
                col4.Background = new SolidColorBrush(Color.FromArgb(255, 0, 100, 0));
                col5.Background = new SolidColorBrush(Color.FromArgb(255, 255, 20, 147));
                col6.Background = new SolidColorBrush(Color.FromArgb(255, 50, 205, 50));
                col7.Background = new SolidColorBrush(Color.FromArgb(255, 255, 69, 0));
                col8.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            }
            else
            {
                col1.Background = new SolidColorBrush(Color.FromArgb(255, 255, 127, 80));
                col2.Background = new SolidColorBrush(Color.FromArgb(255, 100, 149, 237));
                col3.Background = new SolidColorBrush(Color.FromArgb(255, 255, 20, 147));
                col4.Background = new SolidColorBrush(Color.FromArgb(255, 0, 191, 255));
                col5.Background = new SolidColorBrush(Color.FromArgb(255, 50, 205, 50));
                col6.Background = new SolidColorBrush(Color.FromArgb(255, 255, 69, 0));
                col7.Background = new SolidColorBrush(Color.FromArgb(255, 0, 139, 139));
                col8.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            }
        }

        private void Light_Click(object sender, RoutedEventArgs e)
        {
            Background = light.Background;
            main.Background = light.Background;
            ApplicationSettings.Settings.Set("BackgroundOptions", (int)BackgroundColorOptions.Light);
            main.BackgroundOptions = BackgroundColorOptions.Light;
            SetBrushes();
            main.SetBackground();
        }

        private void Dark_Click(object sender, RoutedEventArgs e)
        {
            Background = dark.Background;
            main.Background = dark.Background;
            ApplicationSettings.Settings.Set("BackgroundOptions", (int)BackgroundColorOptions.Dark);
            main.BackgroundOptions = BackgroundColorOptions.Dark;
            SetBrushes();
            main.SetBackground();
        }

        private void ThemeWindow_Click(object sender, RoutedEventArgs e)
        {
            Background = themeWindow.Background;
            main.Background = themeWindow.Background;
            ApplicationSettings.Settings.Set("BackgroundOptions", (int)BackgroundColorOptions.SystemWindows);
            main.BackgroundOptions = BackgroundColorOptions.SystemWindows;
            SetBrushes();
            main.SetBackground();
        }

        private void ThemeApp_Click(object sender, RoutedEventArgs e)
        {
            Background = themeApp.Background;
            main.Background = themeApp.Background;
            ApplicationSettings.Settings.Set("BackgroundOptions", (int)BackgroundColorOptions.SystemApps);
            main.BackgroundOptions = BackgroundColorOptions.SystemApps;
            SetBrushes();
            main.SetBackground();
        }

        private void SetBrushes()
        {
            bool darkBackground =
                main.BackgroundOptions == BackgroundColorOptions.Dark ||
                (main.BackgroundOptions == BackgroundColorOptions.SystemApps && main.BackgroundAppDark) ||
                (main.BackgroundOptions == BackgroundColorOptions.SystemWindows && main.BackgroundWindowDark);

            if (darkBackground)
            {
                Foreground = Brushes.White;
                main.Foreground = Brushes.White;
                HoverBrush = new SolidColorBrush(Color.FromArgb(255, 62, 62, 64));
                main.HoverBrush = new SolidColorBrush(Color.FromArgb(255, 62, 62, 64));
            }
            else
            {
                Foreground = Brushes.Black;
                main.Foreground = Brushes.Black;
                HoverBrush = new SolidColorBrush(Color.FromArgb(255, 192, 192, 194));
                main.HoverBrush = new SolidColorBrush(Color.FromArgb(255, 192, 192, 194));
            }

            PopulateColors(IsAppDark(), darkBackground);
        }

        private void Accent_Click(object sender, RoutedEventArgs e)
        {
            main.AccentOptions = AccentColorOptions.AccentColor;
            var brush = (SolidColorBrush)(sender as Button).Background;
            SetBrush(brush);
        }

        private void Col_Click(object sender, RoutedEventArgs e)
        {
            main.AccentOptions = AccentColorOptions.Color;
            var brush = (SolidColorBrush)(sender as Button).Background;
            SetBrush(brush);
        }

        private void SetBrush(SolidColorBrush brush)
        {
            AccentBrush = brush;
            main.AccentBrush = brush;
            main.SaveAccentColor();
            main.AccentChanged();
            SetColor(brush.Color, true);
        }
    }
}
