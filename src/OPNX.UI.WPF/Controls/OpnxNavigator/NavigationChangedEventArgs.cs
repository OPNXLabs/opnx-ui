using System.Windows;

namespace OPNX.UI.WPF.Controls
{
    public class NavigationChangedEventArgs : RoutedEventArgs
    {
        public NavigationChangedEventArgs(
            RoutedEvent routedEvent,
            object source,
            object? oldItem,
            object? newItem)
            : base(routedEvent, source)
        {
            OldItem = oldItem;
            NewItem = newItem;
        }

        public object? OldItem { get; }

        public object? NewItem { get; }
    }
}
