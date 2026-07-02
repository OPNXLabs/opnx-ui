using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace OPNX.UI.WPF.Controls
{
    public partial class OpnxStepSelector : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(OpnxStepSelector),
            new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(OpnxStepSelector),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemChanged));

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(OpnxStepSelector),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsCircularProperty = DependencyProperty.Register(
            nameof(IsCircular),
            typeof(bool),
            typeof(OpnxStepSelector),
            new PropertyMetadata(false, OnNavigationPropertyChanged));

        public static readonly DependencyProperty ButtonWidthProperty = DependencyProperty.Register(
            nameof(ButtonWidth),
            typeof(double),
            typeof(OpnxStepSelector),
            new PropertyMetadata(24d));

        public static readonly DependencyProperty ButtonHeightProperty = DependencyProperty.Register(
            nameof(ButtonHeight),
            typeof(double),
            typeof(OpnxStepSelector),
            new PropertyMetadata(24d));

        public static readonly DependencyProperty PreviousButtonMarginProperty = DependencyProperty.Register(
            nameof(PreviousButtonMargin),
            typeof(Thickness),
            typeof(OpnxStepSelector),
            new PropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty NextButtonMarginProperty = DependencyProperty.Register(
            nameof(NextButtonMargin),
            typeof(Thickness),
            typeof(OpnxStepSelector),
            new PropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty PreviousButtonContentProperty = DependencyProperty.Register(
            nameof(PreviousButtonContent),
            typeof(object),
            typeof(OpnxStepSelector),
            new PropertyMetadata("-"));

        public static readonly DependencyProperty NextButtonContentProperty = DependencyProperty.Register(
            nameof(NextButtonContent),
            typeof(object),
            typeof(OpnxStepSelector),
            new PropertyMetadata("+"));

        public static readonly DependencyProperty PreviousButtonContentTemplateProperty = DependencyProperty.Register(
            nameof(PreviousButtonContentTemplate),
            typeof(DataTemplate),
            typeof(OpnxStepSelector),
            new PropertyMetadata(null));

        public static readonly DependencyProperty NextButtonContentTemplateProperty = DependencyProperty.Register(
            nameof(NextButtonContentTemplate),
            typeof(DataTemplate),
            typeof(OpnxStepSelector),
            new PropertyMetadata(null));

        public static readonly DependencyProperty PreviousButtonToolTipProperty = DependencyProperty.Register(
            nameof(PreviousButtonToolTip),
            typeof(object),
            typeof(OpnxStepSelector),
            new PropertyMetadata("Previous"));

        public static readonly DependencyProperty NextButtonToolTipProperty = DependencyProperty.Register(
            nameof(NextButtonToolTip),
            typeof(object),
            typeof(OpnxStepSelector),
            new PropertyMetadata("Next"));

        public static readonly DependencyProperty PreviousButtonStyleProperty = DependencyProperty.Register(
            nameof(PreviousButtonStyle),
            typeof(Style),
            typeof(OpnxStepSelector),
            new PropertyMetadata(null));

        public static readonly DependencyProperty NextButtonStyleProperty = DependencyProperty.Register(
            nameof(NextButtonStyle),
            typeof(Style),
            typeof(OpnxStepSelector),
            new PropertyMetadata(null));

        private INotifyCollectionChanged? _notifyCollectionChanged;

        public OpnxStepSelector()
        {
            InitializeComponent();
            UpdateNavigationState();
        }

        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public bool IsCircular
        {
            get => (bool)GetValue(IsCircularProperty);
            set => SetValue(IsCircularProperty, value);
        }

        public double ButtonWidth
        {
            get => (double)GetValue(ButtonWidthProperty);
            set => SetValue(ButtonWidthProperty, value);
        }

        public double ButtonHeight
        {
            get => (double)GetValue(ButtonHeightProperty);
            set => SetValue(ButtonHeightProperty, value);
        }

        public Thickness PreviousButtonMargin
        {
            get => (Thickness)GetValue(PreviousButtonMarginProperty);
            set => SetValue(PreviousButtonMarginProperty, value);
        }

        public Thickness NextButtonMargin
        {
            get => (Thickness)GetValue(NextButtonMarginProperty);
            set => SetValue(NextButtonMarginProperty, value);
        }

        public object? PreviousButtonContent
        {
            get => GetValue(PreviousButtonContentProperty);
            set => SetValue(PreviousButtonContentProperty, value);
        }

        public object? NextButtonContent
        {
            get => GetValue(NextButtonContentProperty);
            set => SetValue(NextButtonContentProperty, value);
        }

        public DataTemplate? PreviousButtonContentTemplate
        {
            get => (DataTemplate?)GetValue(PreviousButtonContentTemplateProperty);
            set => SetValue(PreviousButtonContentTemplateProperty, value);
        }

        public DataTemplate? NextButtonContentTemplate
        {
            get => (DataTemplate?)GetValue(NextButtonContentTemplateProperty);
            set => SetValue(NextButtonContentTemplateProperty, value);
        }

        public object? PreviousButtonToolTip
        {
            get => GetValue(PreviousButtonToolTipProperty);
            set => SetValue(PreviousButtonToolTipProperty, value);
        }

        public object? NextButtonToolTip
        {
            get => GetValue(NextButtonToolTipProperty);
            set => SetValue(NextButtonToolTipProperty, value);
        }

        public Style? PreviousButtonStyle
        {
            get => (Style?)GetValue(PreviousButtonStyleProperty);
            set => SetValue(PreviousButtonStyleProperty, value);
        }

        public Style? NextButtonStyle
        {
            get => (Style?)GetValue(NextButtonStyleProperty);
            set => SetValue(NextButtonStyleProperty, value);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxStepSelector control)
                control.OnItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxStepSelector control)
                control.UpdateNavigationState();
        }

        private static void OnNavigationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxStepSelector control)
                control.UpdateNavigationState();
        }

        private void OnItemsSourceChanged(IEnumerable? oldValue, IEnumerable? newValue)
        {
            if (_notifyCollectionChanged is not null)
                _notifyCollectionChanged.CollectionChanged -= ItemsSource_CollectionChanged;

            _notifyCollectionChanged = newValue as INotifyCollectionChanged;

            if (_notifyCollectionChanged is not null)
                _notifyCollectionChanged.CollectionChanged += ItemsSource_CollectionChanged;

            var items = GetItems();
            if (SelectedItem is null && items.Count > 0)
                SelectedItem = items[0];
            else if (SelectedItem is not null && !items.Contains(SelectedItem))
                SelectedItem = items.Count > 0 ? items[0] : null;

            UpdateNavigationState();
        }

        private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnItemsSourceChanged(ItemsSource, ItemsSource);
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            MoveSelection(-1);
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            MoveSelection(1);
        }

        private void MoveSelection(int offset)
        {
            var items = GetItems();
            if (items.Count == 0)
                return;

            int currentIndex = SelectedItem is null ? -1 : items.IndexOf(SelectedItem);
            int nextIndex = currentIndex + offset;

            if (IsCircular)
            {
                if (nextIndex < 0)
                    nextIndex = items.Count - 1;
                else if (nextIndex >= items.Count)
                    nextIndex = 0;
            }

            if (nextIndex < 0 || nextIndex >= items.Count)
                return;

            SelectedItem = items[nextIndex];
        }

        private void UpdateNavigationState()
        {
            var items = GetItems();
            int currentIndex = SelectedItem is null ? -1 : items.IndexOf(SelectedItem);
            bool canMove = items.Count > 0 && currentIndex >= 0;

            btnPrevious.IsEnabled = canMove && (IsCircular || currentIndex > 0);
            btnNext.IsEnabled = canMove && (IsCircular || currentIndex < items.Count - 1);
        }

        private List<object?> GetItems()
        {
            if (ItemsSource is null)
                return [];

            var items = new List<object?>();
            foreach (var item in ItemsSource)
                items.Add(item);

            return items;
        }
    }
}
