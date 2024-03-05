using Avalonia.Controls;
using System.Diagnostics;

namespace LanistaBrowserV1.UserControls
{
    public partial class AboutPage : UserControl
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void Hyperlink_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (textBlock != null)
            {
                var url = textBlock.Text;
                OpenUrl(url);
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                // Handle exceptions here, if any
            }
        }
    }
}