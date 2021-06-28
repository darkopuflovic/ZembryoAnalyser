using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ZembryoAnalyser
{
    public sealed class StatusBarContent : ObservableCollection<object>
    {
        private readonly TextBlock text;
        private readonly Slider slider;
        private readonly ProgressBar progress;

        public void SetSliderVisibility(bool visible) =>
            slider.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

        public void SetProgressVisibility(bool visible) =>
            progress.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

        public StatusBarContent()
        {
            text = new TextBlock
            {
                Text = "Ready"
            };

            slider = new Slider
            {
                Minimum = 8,
                Maximum = 96,
                Value = 16,
                Width = 100,
                Visibility = Visibility.Collapsed
            };

            progress = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Width = 100,
                Height = 10,
                Visibility = Visibility.Collapsed
            };

            Add(text);
            Add(slider);
            Add(progress);
        }

        public void SetSliderBackground(Brush scb) =>
            slider.Background = scb;

        public void SetProgressBackground(Brush scb) =>
            progress.Background = scb;

        public void SetText(string t) =>
            text.Text = t;

        public void MoveSlider(double width) =>
            slider.Margin = new Thickness(width - slider.Width - 30, 0, 0, 0);

        public void MoveProgress(double width) =>
            progress.Margin = new Thickness(width - progress.Width - 30, 0, 0, 0);

        public void SetProgress(double value) =>
            progress.Value = value;
    }
}
