using System.Windows;
using System.Windows.Media;
using DrawingIcon = System.Drawing.Icon;

namespace OPNX.UI.WPF.Controls
{
    public sealed class OpnxNotifyIconOptions
    {
        public DrawingIcon? Icon { get; set; }

        public string ToolTipText { get; set; } = string.Empty;

        public string OpenText { get; set; } = "Open";

        public string ExitText { get; set; } = "Exit";

        public bool HideWindowOnClose { get; set; } = true;

        public bool RestoreWindowOnDoubleClick { get; set; } = true;

        public bool ShowOpenMenuItem { get; set; } = true;

        public bool ShowExitMenuItem { get; set; } = true;

        public double MenuWidth { get; set; } = 160;

        public double MenuItemHeight { get; set; } = 36;

        public Thickness MenuPadding { get; set; } = new(6);

        public CornerRadius MenuCornerRadius { get; set; } = new(8);

        public Brush MenuBackground { get; set; } = new SolidColorBrush(Color.FromRgb(45, 45, 48));

        public Brush MenuBorderBrush { get; set; } = new SolidColorBrush(Color.FromRgb(70, 70, 74));

        public Brush MenuForeground { get; set; } = Brushes.White;

        public Brush MenuItemMouseOverBackground { get; set; } = new SolidColorBrush(Color.FromRgb(74, 111, 227));

        public Brush MenuItemMouseOverForeground { get; set; } = Brushes.White;
    }
}
