using System.Windows;
using System.Windows.Controls;

namespace OPNX.UI.WPF.Controls
{
    public partial class OpnxNotifyIconMenu : UserControl
    {
        public OpnxNotifyIconMenu(OpnxNotifyIconOptions options)
        {
            InitializeComponent();
            DataContext = options;

            OpenButton.Visibility = options.ShowOpenMenuItem
                ? Visibility.Visible
                : Visibility.Collapsed;
            ExitButton.Visibility = options.ShowExitMenuItem
                ? Visibility.Visible
                : Visibility.Collapsed;
            Separator.Visibility = options.ShowOpenMenuItem && options.ShowExitMenuItem
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public event EventHandler? OpenRequested;

        public event EventHandler? ExitRequested;

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
