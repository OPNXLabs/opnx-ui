using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace OPNX.UI.WPF.Controls
{
    public class OpnxNavigator : Selector
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(OpnxNavigator),
            new FrameworkPropertyMetadata(
                Orientation.Vertical,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty PlacementProperty = DependencyProperty.RegisterAttached(
            "Placement",
            typeof(NavigatorItemPlacement),
            typeof(OpnxNavigator),
            new FrameworkPropertyMetadata(
                NavigatorItemPlacement.Start,
                FrameworkPropertyMetadataOptions.AffectsParentMeasure |
                FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static readonly DependencyProperty IsNavigationTargetProperty = DependencyProperty.RegisterAttached(
            "IsNavigationTarget",
            typeof(bool),
            typeof(OpnxNavigator),
            new FrameworkPropertyMetadata(true));

        public static readonly RoutedEvent NavigationChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(NavigationChanged),
            RoutingStrategy.Bubble,
            typeof(EventHandler<NavigationChangedEventArgs>),
            typeof(OpnxNavigator));

        static OpnxNavigator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxNavigator),
                new FrameworkPropertyMetadata(typeof(OpnxNavigator)));
        }

        public OpnxNavigator()
        {
            AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnButtonClick));
        }

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static NavigatorItemPlacement GetPlacement(DependencyObject element)
        {
            return (NavigatorItemPlacement)element.GetValue(PlacementProperty);
        }

        public static void SetPlacement(DependencyObject element, NavigatorItemPlacement value)
        {
            element.SetValue(PlacementProperty, value);
        }

        public static bool GetIsNavigationTarget(DependencyObject element)
        {
            return (bool)element.GetValue(IsNavigationTargetProperty);
        }

        public static void SetIsNavigationTarget(DependencyObject element, bool value)
        {
            element.SetValue(IsNavigationTargetProperty, value);
        }

        public event EventHandler<NavigationChangedEventArgs> NavigationChanged
        {
            add => AddHandler(NavigationChangedEvent, value);
            remove => RemoveHandler(NavigationChangedEvent, value);
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            foreach (var item in e.RemovedItems)
            {
                if (GetItemElement(item) is { } element)
                {
                    Selector.SetIsSelected(element, false);

                    if (element is ToggleButton toggleButton)
                    {
                        toggleButton.IsChecked = false;
                    }
                }
            }

            foreach (var item in e.AddedItems)
            {
                if (GetItemElement(item) is { } element)
                {
                    Selector.SetIsSelected(element, true);

                    if (element is ToggleButton toggleButton)
                    {
                        toggleButton.IsChecked = true;
                    }
                }
            }

            RaiseEvent(new NavigationChangedEventArgs(
                NavigationChangedEvent,
                this,
                e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null,
                e.AddedItems.Count > 0 ? e.AddedItems[0] : null));
        }

        private DependencyObject? GetItemElement(object item)
        {
            return item as DependencyObject ?? ItemContainerGenerator.ContainerFromItem(item);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject source)
            {
                return;
            }

            var container = ContainerFromElement(this, source);
            if (container is null)
            {
                return;
            }

            if (!GetIsNavigationTarget(container))
            {
                return;
            }

            if (container is ToggleButton toggleButton)
            {
                toggleButton.IsChecked = true;
            }

            var item = ItemContainerGenerator.ItemFromContainer(container);
            if (item != DependencyProperty.UnsetValue)
            {
                SelectedItem = item;
            }
        }
    }
}
