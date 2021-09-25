using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ZembryoAnalyser
{
    /// <summary>
    /// Interaction logic for ColorPickerControl.xaml
    /// </summary>
    public partial class ColorPickerControl : UserControl
    {
        public event EventHandler ColorChanged;
        private bool mouseDownColors;
        private bool mouseDownColor;
        private bool slidersChangingAutomatically;
        private double fixSize;

        public ColorPickerControl()
        {
            InitializeComponent();
            mouseDownColors = false;
            mouseDownColor = false;
            slidersChangingAutomatically = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            rounder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136));
            chooseColorBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136));
            slider.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136));
            selectedColor.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136));
            fixSize = chooseColor.ActualWidth / (chooseColor.ActualWidth - (rounderCircle.ActualWidth / 2));
        }

        public void SetColor(Color color)
        {
            ChangeColorProperties(color);
            ChangePalletePosition(color);
        }

        public void SetColor(Color color, bool automatically)
        {
            bool sliders = slidersChangingAutomatically;
            slidersChangingAutomatically = automatically;
            SetColor(color);
            slidersChangingAutomatically = sliders;
        }

        public Brush GetSelectedBrush()
        {
            return selectedColor.Background;
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double pos = e.GetPosition(slider).Y;
            pos = pos < 0 ? 0 : pos;
            pos = pos > slider.ActualHeight ? slider.ActualHeight : pos;
            mouseDownColors = true;
            SetColors(pos);
            _ = Mouse.Capture(slider);
        }

        private void ChooseColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double posX = e.GetPosition(chooseColor).X;
            double posY = e.GetPosition(chooseColor).Y;
            posX = posX < 4 ? 4 : posX;
            posY = posY < 4 ? 4 : posY;
            posX = posX > chooseColor.ActualWidth - 4 ? chooseColor.ActualWidth - 4 : posX;
            posY = posY > chooseColor.ActualHeight - 4 ? chooseColor.ActualHeight - 4 : posY;
            mouseDownColor = true;
            SetColor(posX, posY);
            _ = Mouse.Capture(chooseColor);
        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDownColors)
            {
                double pos = e.GetPosition(slider).Y;
                pos = pos < 0 ? 0 : pos;
                pos = pos > slider.ActualHeight ? slider.ActualHeight : pos;
                mouseDownColors = true;
                SetColors(pos);
            }

            if (mouseDownColor)
            {
                double posX = e.GetPosition(chooseColor).X;
                double posY = e.GetPosition(chooseColor).Y;
                posX = posX < 4 ? 4 : posX;
                posY = posY < 4 ? 4 : posY;
                posX = posX > chooseColor.ActualWidth - 4 ? chooseColor.ActualWidth - 4 : posX;
                posY = posY > chooseColor.ActualHeight - 4 ? chooseColor.ActualHeight - 4 : posY;
                mouseDownColor = true;
                SetColor(posX, posY);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (alphaSlider != null && redSlider != null && greenSlider != null && blueSlider != null && selectedColor != null && selectedColorText != null && selectedColorTextRGB != null)
            {
                Color col = Color.FromArgb((byte)alphaSlider.Value, (byte)redSlider.Value, (byte)greenSlider.Value, (byte)blueSlider.Value);
                selectedColor.Background = new SolidColorBrush(col);
                selectedColorText.Text = col.ToString(CultureInfo.InvariantCulture);
                selectedColorText.Foreground = new SolidColorBrush(ContrastColor(col));
                selectedColorTextRGB.Text = $"Red: {col.R}, Green: {col.G}, Blue: {col.B}";
                alphaText.Text = $"Alpha: {col.A}";
                ChangePalletePositionInternal(col);
            }
        }

        private void ChangePalletePosition(Color col)
        {
            if (slider.ActualHeight > 0 && chooseColor.ActualHeight > 0 && chooseColor.ActualWidth > 0)
            {
                HSV hsv = RGBToHSV(col);

                double pos = hsv.H / 360 * 190;

                colorGradientStop.Color = GetRelativeColorSlider(gsc, pos / slider.ActualHeight);
                rounder.Margin = new Thickness(0, pos, 0, 0);
                rounderCircle.Margin = new Thickness(0, 4, 4, 0);
                rounder.Margin = new Thickness(0, pos, 0, 0);

                double sizeX = chooseColor.ActualWidth - 8;
                double sizeY = chooseColor.ActualHeight - 8;

                double posX = sizeX * hsv.S;
                double posY = sizeY * (1 - hsv.V);

                posX += 4;
                posY += 4;

                rounderCircle.Margin = new Thickness(0, posY, chooseColor.ActualWidth - posX, 0);
            }
        }

        private void ChangePalletePositionInternal(Color col)
        {
            if (!slidersChangingAutomatically)
            {
                ChangePalletePosition(col);

                if (slider.ActualHeight > 0 && chooseColor.ActualHeight > 0 && chooseColor.ActualWidth > 0)
                {
                    ColorChanged?.Invoke(this, null);
                }
            }
        }

        private static HSV RGBToHSV(Color rgb)
        {
            double delta, min;
            double h = 0, s, v;

            min = Math.Min(Math.Min(rgb.R, rgb.G), rgb.B);
            v = Math.Max(Math.Max(rgb.R, rgb.G), rgb.B);
            delta = v - min;

            s = v == 0.0 ? 0 : delta / v;

            if (s == 0)
            {
                h = 0.0;
            }
            else
            {
                if (rgb.R == v)
                {
                    h = (rgb.G - rgb.B) / delta;
                }
                else if (rgb.G == v)
                {
                    h = 2 + ((rgb.B - rgb.R) / delta);
                }
                else if (rgb.B == v)
                {
                    h = 4 + ((rgb.R - rgb.G) / delta);
                }

                h *= 60;

                if (h < 0.0)
                {
                    h += 360;
                }
            }

            return new HSV(h, s, v / 255);
        }

        private void ChangeColorProperties(Color color)
        {
            selectedColor.Background = new SolidColorBrush(color);
            selectedColorTextRGB.Text = $"Red: {color.R}, Green: {color.G}, Blue: {color.B}";
            alphaText.Text = $"Alpha: {color.A}";
            selectedColorText.Text = color.ToString(CultureInfo.InvariantCulture);
            selectedColorText.Foreground = new SolidColorBrush(ContrastColor(color));
            redSlider.Value = color.R;
            greenSlider.Value = color.G;
            blueSlider.Value = color.B;
            alphaSlider.Value = color.A;
        }

        private void ChangeColorPropertiesInternal(Color color)
        {
            slidersChangingAutomatically = true;
            ChangeColorProperties(color);
            ColorChanged?.Invoke(this, null);
            slidersChangingAutomatically = false;
        }

        private void SetColors(double pos)
        {
            colorGradientStop.Color = GetRelativeColorSlider(gsc, pos / slider.ActualHeight);
            ChangeColorPropertiesInternal(colorGradientStop.Color);
            rounder.Margin = new Thickness(0, pos, 0, 0);
            rounderCircle.Margin = new Thickness(0, 4, 4, 0);
        }

        private void SetColor(double posX, double posY)
        {
            double posXTemp = (posX - 4) * fixSize;
            double posYTemp = (posY - 4) * fixSize;
            double xPos = posXTemp / (chooseColor.ActualWidth - 4);
            double yPos = posYTemp / (chooseColor.ActualHeight - 4);
            Color col = GetRelativeColorLinear(colorGradientStop.Color, xPos, yPos);
            ChangeColorPropertiesInternal(col);
            rounderCircle.Margin = new Thickness(0, posY, chooseColor.ActualWidth - posX, 0);
        }

        private Color ContrastColor(Color iColor)
        {
            if (iColor.A < 128)
            {
                if (Foreground is SolidColorBrush)
                {
                    return (Foreground as SolidColorBrush).Color;
                }
                else if (Background is SolidColorBrush)
                {
                    return ContrastColor((Background as SolidColorBrush).Color);
                }
                return Colors.Black;
            }
            double luma = ((0.299 * iColor.R) + ((0.587 * iColor.G) + (0.114 * iColor.B))) / 255;
            return luma > 0.5 ? Colors.Black : Colors.White;
        }

        private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseDownColors = false;
            mouseDownColor = false;
            _ = Mouse.Capture(null);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseDownColors = false;
            mouseDownColor = false;
            _ = Mouse.Capture(null);
        }

        private static Color GetRelativeColorSlider(GradientStopCollection gsc, double offset)
        {
            GradientStop before = gsc.First(w => w.Offset == gsc.Min(m => m.Offset));
            GradientStop after = gsc.First(w => w.Offset == gsc.Max(m => m.Offset));

            foreach (GradientStop gs in gsc)
            {
                if (gs.Offset == offset)
                {
                    return gs.Color;
                }
                if (gs.Offset < offset && gs.Offset > before.Offset)
                {
                    before = gs;
                }
                if (gs.Offset > offset && gs.Offset < after.Offset)
                {
                    after = gs;
                }
            }

            return Color.FromArgb((byte)(((offset - before.Offset) * (after.Color.A - before.Color.A) / (after.Offset - before.Offset)) + before.Color.A),
                                  (byte)(((offset - before.Offset) * (after.Color.R - before.Color.R) / (after.Offset - before.Offset)) + before.Color.R),
                                  (byte)(((offset - before.Offset) * (after.Color.G - before.Color.G) / (after.Offset - before.Offset)) + before.Color.G),
                                  (byte)(((offset - before.Offset) * (after.Color.B - before.Color.B) / (after.Offset - before.Offset)) + before.Color.B));
        }

        private Color GetRelativeColorLinear(Color color, double xPos, double yPos)
        {
            byte r = color.R;
            byte g = color.G;
            byte b = color.B;

            double tempR = r + ((1 - xPos) * 255);
            double tempG = g + ((1 - xPos) * 255);
            double tempB = b + ((1 - xPos) * 255);

            r = tempR > 255 ? (byte)255 : (byte)tempR;
            g = tempG > 255 ? (byte)255 : (byte)tempG;
            b = tempB > 255 ? (byte)255 : (byte)tempB;

            r = (byte)(r * (1 - yPos));
            g = (byte)(g * (1 - yPos));
            b = (byte)(b * (1 - yPos));

            return Color.FromArgb((byte)alphaSlider.Value, r, g, b);
        }
    }
}
