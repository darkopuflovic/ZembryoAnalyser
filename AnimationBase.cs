using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ZembryoAnalyser
{
    public static class AnimationBase
    {
        private static FrameworkElement visibleElement;
        private static FrameworkElement lastElement;

        static AnimationBase()
        {
            visibleElement = (MainWindow)Application.Current.MainWindow;
            lastElement = visibleElement;
        }

        public static void Animate(FrameworkElement element, string propertyPath, TimeSpan duration, double from, double to)
        {
            var s = new Storyboard();
            var da = new DoubleAnimation();
            s.Children.Add(da);
            Storyboard.SetTarget(da, element);
            Storyboard.SetTargetProperty(da, new PropertyPath(propertyPath));
            da.Duration = s.Duration = new Duration(duration);
            da.From = from;
            da.To = to;
            s.Begin();
        }

        public static void EnterAnimation(FrameworkElement element)
        {
            HideElement(visibleElement);
            ShowElement(element);
            lastElement = visibleElement;
            visibleElement = element;
        }

        public static void ExitAnimation()
        {
            HideElement(visibleElement);
            ShowElement(lastElement);
            FrameworkElement temp = lastElement;
            lastElement = visibleElement;
            visibleElement = temp;
        }

        private static void ShowElement(FrameworkElement element)
        {
            element.Opacity = 0.0;
            element.Visibility = Visibility.Visible;
            var s = new Storyboard();
            var da = new DoubleAnimation();
            s.Children.Add(da);
            Storyboard.SetTarget(da, element);
            Storyboard.SetTargetProperty(da, new PropertyPath("(FrameworkElement.Opacity)"));
            da.Duration = s.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            da.From = 0.0;
            da.To = 1.0;
            s.Begin();
        }

        private static void HideElement(FrameworkElement element)
        {
            element.Opacity = 1.0;
            var s = new Storyboard();
            s.Completed += (p, q) =>
            {
                element.Visibility = Visibility.Collapsed;
            };
            var da = new DoubleAnimation();
            s.Children.Add(da);
            Storyboard.SetTarget(da, element);
            Storyboard.SetTargetProperty(da, new PropertyPath("(FrameworkElement.Opacity)"));
            da.Duration = s.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            da.From = 1.0;
            da.To = 0.0;
            s.Begin();
        }
    }
}
