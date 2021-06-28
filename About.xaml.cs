using Crystalbyte.UI;
using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ZembryoAnalyser
{
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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var info = new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            };
            using var _ = Process.Start(info);
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
}
