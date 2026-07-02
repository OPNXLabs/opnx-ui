using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace OPNX.UI.WPF.Controls
{
    public enum OpnxDateRangeType
    {
        None,
        Today,
        OneWeek,
        OneMonth,
        ThreeMonths,
        Custom
    }

    public sealed class OpnxDateRangeItem : DependencyObject
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(OpnxDateRangeItem),
            new PropertyMetadata(null));

        public static readonly DependencyProperty RangeTypeProperty = DependencyProperty.Register(
            nameof(RangeType),
            typeof(OpnxDateRangeType),
            typeof(OpnxDateRangeItem),
            new PropertyMetadata(OpnxDateRangeType.None));

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(OpnxDateRangeItem),
            new PropertyMetadata(false));

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(OpnxDateRangeItem),
            new PropertyMetadata(new CornerRadius(3)));

        public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register(
            nameof(ItemMargin),
            typeof(Thickness),
            typeof(OpnxDateRangeItem),
            new PropertyMetadata(new Thickness(0)));

        public object? Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public OpnxDateRangeType RangeType
        {
            get => (OpnxDateRangeType)GetValue(RangeTypeProperty);
            set => SetValue(RangeTypeProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public Thickness ItemMargin
        {
            get => (Thickness)GetValue(ItemMarginProperty);
            set => SetValue(ItemMarginProperty, value);
        }
    }

    public partial class OpnxDateRangeSelector : UserControl
    {
        private static int _radioGroupSeed;
        private bool _isUpdatingRange;
        private bool _hasUserItems;

        private static readonly DependencyPropertyKey IsCustomRangePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsCustomRange),
            typeof(bool),
            typeof(OpnxDateRangeSelector),
            new PropertyMetadata(false));

        public static readonly DependencyProperty FromDateTimeProperty = DependencyProperty.Register(
            nameof(FromDateTime),
            typeof(DateTime),
            typeof(OpnxDateRangeSelector),
            new FrameworkPropertyMetadata(
                DateTime.Today,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ToDateTimeProperty = DependencyProperty.Register(
            nameof(ToDateTime),
            typeof(DateTime),
            typeof(OpnxDateRangeSelector),
            new FrameworkPropertyMetadata(
                EndOfToday(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty RangeTypeProperty = DependencyProperty.Register(
            nameof(RangeType),
            typeof(OpnxDateRangeType),
            typeof(OpnxDateRangeSelector),
            new FrameworkPropertyMetadata(
                OpnxDateRangeType.Today,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRangeTypeChanged));

        public static readonly DependencyProperty DateDisplayFormatProperty = DependencyProperty.Register(
            nameof(DateDisplayFormat),
            typeof(string),
            typeof(OpnxDateRangeSelector),
            new PropertyMetadata("yyyy-MM-dd"));

        public static readonly DependencyProperty IsCustomRangeProperty = IsCustomRangePropertyKey.DependencyProperty;

        public OpnxDateRangeSelector()
        {
            RadioGroupName = $"{nameof(OpnxDateRangeSelector)}_{Interlocked.Increment(ref _radioGroupSeed)}";
            Items = [];
            Items.CollectionChanged += Items_CollectionChanged;

            InitializeComponent();
            Loaded += OpnxDateRangeSelector_Loaded;
        }

        public ObservableCollection<OpnxDateRangeItem> Items { get; }

        public DateTime FromDateTime
        {
            get => (DateTime)GetValue(FromDateTimeProperty);
            set => SetValue(FromDateTimeProperty, value);
        }

        public DateTime ToDateTime
        {
            get => (DateTime)GetValue(ToDateTimeProperty);
            set => SetValue(ToDateTimeProperty, value);
        }

        public OpnxDateRangeType RangeType
        {
            get => (OpnxDateRangeType)GetValue(RangeTypeProperty);
            set => SetValue(RangeTypeProperty, value);
        }

        public string DateDisplayFormat
        {
            get => (string)GetValue(DateDisplayFormatProperty);
            set => SetValue(DateDisplayFormatProperty, value);
        }

        public bool IsCustomRange
        {
            get => (bool)GetValue(IsCustomRangeProperty);
            private set => SetValue(IsCustomRangePropertyKey, value);
        }

        public string RadioGroupName { get; }

        private static void OnRangeTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxDateRangeSelector control && e.NewValue is OpnxDateRangeType rangeType)
                control.ApplyRange(rangeType);
        }

        private void OpnxDateRangeSelector_Loaded(object sender, RoutedEventArgs e)
        {
            EnsureDefaultItems();
            ApplyRange(RangeType);
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_isUpdatingRange && !_hasUserItems)
                _hasUserItems = true;

            UpdateItemLayout();
            UpdateRangeItems(RangeType);
        }

        private void RangeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingRange)
                return;

            if (sender is RadioButton { Tag: OpnxDateRangeType rangeType })
                RangeType = rangeType;
        }

        private void ApplyRange(OpnxDateRangeType rangeType)
        {
            _isUpdatingRange = true;

            try
            {
                IsCustomRange = rangeType == OpnxDateRangeType.Custom;

                if (!IsCustomRange && rangeType != OpnxDateRangeType.None)
                {
                    var today = DateTime.Today;
                    ToDateTime = today.AddDays(1).AddTicks(-1);
                    FromDateTime = rangeType switch
                    {
                        OpnxDateRangeType.Today => today,
                        OpnxDateRangeType.OneWeek => today.AddDays(-7),
                        OpnxDateRangeType.OneMonth => today.AddMonths(-1),
                        OpnxDateRangeType.ThreeMonths => today.AddMonths(-3),
                        _ => FromDateTime
                    };
                }

                UpdateRangeItems(rangeType);
            }
            finally
            {
                _isUpdatingRange = false;
            }
        }

        private void EnsureDefaultItems()
        {
            if (_hasUserItems || Items.Count > 0)
                return;

            _isUpdatingRange = true;

            try
            {
                Items.Add(new OpnxDateRangeItem { Header = "오늘", RangeType = OpnxDateRangeType.Today });
                Items.Add(new OpnxDateRangeItem { Header = "1주", RangeType = OpnxDateRangeType.OneWeek });
                Items.Add(new OpnxDateRangeItem { Header = "1개월", RangeType = OpnxDateRangeType.OneMonth });
                Items.Add(new OpnxDateRangeItem { Header = "3개월", RangeType = OpnxDateRangeType.ThreeMonths });
                Items.Add(new OpnxDateRangeItem { Header = "직접입력", RangeType = OpnxDateRangeType.Custom });
            }
            finally
            {
                _isUpdatingRange = false;
            }

            UpdateItemLayout();
        }

        private void UpdateItemLayout()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].ItemMargin = i == 0 || i == Items.Count - 1
                    ? new Thickness(0)
                    : new Thickness(7, 0, 7, 0);
            }
        }

        private void UpdateRangeItems(OpnxDateRangeType rangeType)
        {
            foreach (var item in Items)
                item.IsSelected = item.RangeType == rangeType;
        }

        private static DateTime EndOfToday()
        {
            return DateTime.Today.AddDays(1).AddTicks(-1);
        }
    }
}
