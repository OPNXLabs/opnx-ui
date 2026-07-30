using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public partial class OpnxPagingControl : UserControl, INotifyPropertyChanged
    {
        private readonly ObservableCollection<OpnxPagingItem> _pagingItems = [];
        private OpnxPagingItem? _selectedItem;
        private bool _isUpdatingSelection;

        public OpnxPagingControl()
        {
            InitializeComponent();

            CreatePage();
            RefreshPageItems();
            SelectPageNumber(SelectedPageNumber);
            UpdateMoveButtonVisibility();
        }

        public static readonly DependencyProperty PageNumToolTipOpeningProperty =
            DependencyProperty.Register(
                nameof(PageNumToolTipOpening),
                typeof(ICommand),
                typeof(OpnxPagingControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty PageNumToolTipContentProperty =
            DependencyProperty.Register(
                nameof(PageNumToolTipContent),
                typeof(ImageSource),
                typeof(OpnxPagingControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty MaxPageNumberProperty =
            DependencyProperty.Register(
                nameof(MaxPageNumber),
                typeof(int),
                typeof(OpnxPagingControl),
                new FrameworkPropertyMetadata(1, OnPagingPropertyChanged));

        public static readonly DependencyProperty TotalPagingItemCountProperty =
            DependencyProperty.Register(
                nameof(TotalPagingItemCount),
                typeof(int),
                typeof(OpnxPagingControl),
                new FrameworkPropertyMetadata(10, OnPagingPropertyChanged));

        public static readonly DependencyProperty SelectedPageNumberProperty =
            DependencyProperty.Register(
                nameof(SelectedPageNumber),
                typeof(int),
                typeof(OpnxPagingControl),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPagingPropertyChanged));

        public static readonly DependencyProperty PrevButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(PrevButtonVisibility),
                typeof(Visibility),
                typeof(OpnxPagingControl),
                new PropertyMetadata(Visibility.Visible, OnDependencyPropertyChanged));

        public static readonly DependencyProperty NextButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(NextButtonVisibility),
                typeof(Visibility),
                typeof(OpnxPagingControl),
                new PropertyMetadata(Visibility.Visible, OnDependencyPropertyChanged));

        public static readonly DependencyProperty AutoUpdatePageNumberProperty =
            DependencyProperty.Register(
                nameof(AutoUpdatePageNumber),
                typeof(bool),
                typeof(OpnxPagingControl),
                new PropertyMetadata(true));

        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register(
                nameof(DisplayMode),
                typeof(OpnxPagingDisplayMode),
                typeof(OpnxPagingControl),
                new PropertyMetadata(OpnxPagingDisplayMode.Sliding, OnPagingPropertyChanged));

        public static readonly DependencyProperty AutoHideMoveButtonsProperty =
            DependencyProperty.Register(
                nameof(AutoHideMoveButtons),
                typeof(bool),
                typeof(OpnxPagingControl),
                new PropertyMetadata(false, OnPagingPropertyChanged));

        public static readonly DependencyProperty PageButtonWidthProperty =
            DependencyProperty.Register(
                nameof(PageButtonWidth),
                typeof(double),
                typeof(OpnxPagingControl),
                new PropertyMetadata(25d));

        public static readonly DependencyProperty PageButtonHeightProperty =
            DependencyProperty.Register(
                nameof(PageButtonHeight),
                typeof(double),
                typeof(OpnxPagingControl),
                new PropertyMetadata(25d));

        public static readonly DependencyProperty PageButtonMarginProperty =
            DependencyProperty.Register(
                nameof(PageButtonMargin),
                typeof(Thickness),
                typeof(OpnxPagingControl),
                new PropertyMetadata(new Thickness(5, 0, 5, 0), OnPageButtonLayoutPropertyChanged));

        public static readonly DependencyProperty PageButtonColumnsProperty =
            DependencyProperty.Register(
                nameof(PageButtonColumns),
                typeof(int),
                typeof(OpnxPagingControl),
                new PropertyMetadata(0));

        public static readonly DependencyProperty PageButtonRowSpacingProperty =
            DependencyProperty.Register(
                nameof(PageButtonRowSpacing),
                typeof(double),
                typeof(OpnxPagingControl),
                new PropertyMetadata(0d, OnPageButtonLayoutPropertyChanged));

        public static readonly DependencyProperty PageButtonCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(PageButtonCornerRadius),
                typeof(CornerRadius),
                typeof(OpnxPagingControl),
                new PropertyMetadata(new CornerRadius(16)));

        public static readonly DependencyProperty PageButtonBorderThicknessProperty =
            DependencyProperty.Register(
                nameof(PageButtonBorderThickness),
                typeof(Thickness),
                typeof(OpnxPagingControl),
                new PropertyMetadata(new Thickness(1.5)));

        public static readonly DependencyProperty PageButtonBackgroundProperty =
            DependencyProperty.Register(
                nameof(PageButtonBackground),
                typeof(Brush),
                typeof(OpnxPagingControl),
                new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty PageButtonForegroundProperty =
            DependencyProperty.Register(
                nameof(PageButtonForeground),
                typeof(Brush),
                typeof(OpnxPagingControl),
                new PropertyMetadata(Brushes.LightGray));

        public static readonly DependencyProperty PageButtonBorderBrushProperty =
            DependencyProperty.Register(
                nameof(PageButtonBorderBrush),
                typeof(Brush),
                typeof(OpnxPagingControl),
                new PropertyMetadata(Brushes.LightGray));

        public static readonly DependencyProperty PageButtonSelectedForegroundProperty =
            DependencyProperty.Register(
                nameof(PageButtonSelectedForeground),
                typeof(Brush),
                typeof(OpnxPagingControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x16, 0xAB, 0xBD))));

        public static readonly DependencyProperty PageButtonSelectedBorderBrushProperty =
            DependencyProperty.Register(
                nameof(PageButtonSelectedBorderBrush),
                typeof(Brush),
                typeof(OpnxPagingControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x16, 0xAB, 0xBD))));

        public static readonly DependencyProperty PageButtonSelectedBackgroundProperty =
            DependencyProperty.Register(
                nameof(PageButtonSelectedBackground),
                typeof(Brush),
                typeof(OpnxPagingControl),
                new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty PageButtonDisabledForegroundProperty =
            DependencyProperty.Register(
                nameof(PageButtonDisabledForeground),
                typeof(Brush),
                typeof(OpnxPagingControl),
                new PropertyMetadata(Brushes.DimGray));

        public static readonly DependencyProperty PageButtonDisabledBorderBrushProperty =
            DependencyProperty.Register(
                nameof(PageButtonDisabledBorderBrush),
                typeof(Brush),
                typeof(OpnxPagingControl),
                new PropertyMetadata(Brushes.DimGray));

        public static readonly DependencyProperty PageButtonDisabledBackgroundProperty =
            DependencyProperty.Register(
                nameof(PageButtonDisabledBackground),
                typeof(Brush),
                typeof(OpnxPagingControl),
                new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty PageButtonMouseOverOpacityProperty =
            DependencyProperty.Register(
                nameof(PageButtonMouseOverOpacity),
                typeof(double),
                typeof(OpnxPagingControl),
                new PropertyMetadata(0.8d));

        public static readonly DependencyProperty MoveButtonForegroundProperty =
            DependencyProperty.Register(
                nameof(MoveButtonForeground),
                typeof(Brush),
                typeof(OpnxPagingControl),
                new PropertyMetadata(Brushes.LightGray));

        public static readonly DependencyProperty MoveButtonPressedForegroundProperty =
            DependencyProperty.Register(
                nameof(MoveButtonPressedForeground),
                typeof(Brush),
                typeof(OpnxPagingControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x16, 0xAB, 0xBD))));

        public static readonly DependencyProperty MoveButtonMouseOverOpacityProperty =
            DependencyProperty.Register(
                nameof(MoveButtonMouseOverOpacity),
                typeof(double),
                typeof(OpnxPagingControl),
                new PropertyMetadata(0.8d));

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand? PageNumToolTipOpening
        {
            get => (ICommand?)GetValue(PageNumToolTipOpeningProperty);
            set => SetValue(PageNumToolTipOpeningProperty, value);
        }

        public ImageSource? PageNumToolTipContent
        {
            get => (ImageSource?)GetValue(PageNumToolTipContentProperty);
            set => SetValue(PageNumToolTipContentProperty, value);
        }

        public int MaxPageNumber
        {
            get => (int)GetValue(MaxPageNumberProperty);
            set => SetValue(MaxPageNumberProperty, value);
        }

        public int TotalPagingItemCount
        {
            get => (int)GetValue(TotalPagingItemCountProperty);
            set => SetValue(TotalPagingItemCountProperty, value);
        }

        public int SelectedPageNumber
        {
            get => (int)GetValue(SelectedPageNumberProperty);
            set => SetValue(SelectedPageNumberProperty, value);
        }

        public Visibility PrevButtonVisibility
        {
            get => (Visibility)GetValue(PrevButtonVisibilityProperty);
            set => SetValue(PrevButtonVisibilityProperty, value);
        }

        public Visibility NextButtonVisibility
        {
            get => (Visibility)GetValue(NextButtonVisibilityProperty);
            set => SetValue(NextButtonVisibilityProperty, value);
        }

        public bool AutoUpdatePageNumber
        {
            get => (bool)GetValue(AutoUpdatePageNumberProperty);
            set => SetValue(AutoUpdatePageNumberProperty, value);
        }

        public OpnxPagingDisplayMode DisplayMode
        {
            get => (OpnxPagingDisplayMode)GetValue(DisplayModeProperty);
            set => SetValue(DisplayModeProperty, value);
        }

        public bool AutoHideMoveButtons
        {
            get => (bool)GetValue(AutoHideMoveButtonsProperty);
            set => SetValue(AutoHideMoveButtonsProperty, value);
        }

        public double PageButtonWidth
        {
            get => (double)GetValue(PageButtonWidthProperty);
            set => SetValue(PageButtonWidthProperty, value);
        }

        public double PageButtonHeight
        {
            get => (double)GetValue(PageButtonHeightProperty);
            set => SetValue(PageButtonHeightProperty, value);
        }

        public Thickness PageButtonMargin
        {
            get => (Thickness)GetValue(PageButtonMarginProperty);
            set => SetValue(PageButtonMarginProperty, value);
        }

        public int PageButtonColumns
        {
            get => (int)GetValue(PageButtonColumnsProperty);
            set => SetValue(PageButtonColumnsProperty, value);
        }

        public double PageButtonRowSpacing
        {
            get => (double)GetValue(PageButtonRowSpacingProperty);
            set => SetValue(PageButtonRowSpacingProperty, value);
        }

        public Thickness PageButtonActualMargin =>
            new(
                PageButtonMargin.Left,
                PageButtonMargin.Top,
                PageButtonMargin.Right,
                PageButtonMargin.Bottom + Math.Max(0, PageButtonRowSpacing));

        public CornerRadius PageButtonCornerRadius
        {
            get => (CornerRadius)GetValue(PageButtonCornerRadiusProperty);
            set => SetValue(PageButtonCornerRadiusProperty, value);
        }

        public Thickness PageButtonBorderThickness
        {
            get => (Thickness)GetValue(PageButtonBorderThicknessProperty);
            set => SetValue(PageButtonBorderThicknessProperty, value);
        }

        public Brush PageButtonBackground
        {
            get => (Brush)GetValue(PageButtonBackgroundProperty);
            set => SetValue(PageButtonBackgroundProperty, value);
        }

        public Brush PageButtonForeground
        {
            get => (Brush)GetValue(PageButtonForegroundProperty);
            set => SetValue(PageButtonForegroundProperty, value);
        }

        public Brush PageButtonBorderBrush
        {
            get => (Brush)GetValue(PageButtonBorderBrushProperty);
            set => SetValue(PageButtonBorderBrushProperty, value);
        }

        public Brush PageButtonSelectedForeground
        {
            get => (Brush)GetValue(PageButtonSelectedForegroundProperty);
            set => SetValue(PageButtonSelectedForegroundProperty, value);
        }

        public Brush PageButtonSelectedBorderBrush
        {
            get => (Brush)GetValue(PageButtonSelectedBorderBrushProperty);
            set => SetValue(PageButtonSelectedBorderBrushProperty, value);
        }

        public Brush PageButtonSelectedBackground
        {
            get => (Brush)GetValue(PageButtonSelectedBackgroundProperty);
            set => SetValue(PageButtonSelectedBackgroundProperty, value);
        }

        public Brush PageButtonDisabledForeground
        {
            get => (Brush)GetValue(PageButtonDisabledForegroundProperty);
            set => SetValue(PageButtonDisabledForegroundProperty, value);
        }

        public Brush PageButtonDisabledBorderBrush
        {
            get => (Brush)GetValue(PageButtonDisabledBorderBrushProperty);
            set => SetValue(PageButtonDisabledBorderBrushProperty, value);
        }

        public Brush PageButtonDisabledBackground
        {
            get => (Brush)GetValue(PageButtonDisabledBackgroundProperty);
            set => SetValue(PageButtonDisabledBackgroundProperty, value);
        }

        public double PageButtonMouseOverOpacity
        {
            get => (double)GetValue(PageButtonMouseOverOpacityProperty);
            set => SetValue(PageButtonMouseOverOpacityProperty, value);
        }

        public Brush MoveButtonForeground
        {
            get => (Brush)GetValue(MoveButtonForegroundProperty);
            set => SetValue(MoveButtonForegroundProperty, value);
        }

        public Brush MoveButtonPressedForeground
        {
            get => (Brush)GetValue(MoveButtonPressedForegroundProperty);
            set => SetValue(MoveButtonPressedForegroundProperty, value);
        }

        public double MoveButtonMouseOverOpacity
        {
            get => (double)GetValue(MoveButtonMouseOverOpacityProperty);
            set => SetValue(MoveButtonMouseOverOpacityProperty, value);
        }

        public ObservableCollection<OpnxPagingItem> PagingItems => _pagingItems;

        public Visibility ActualPrevButtonVisibility =>
            PrevButtonVisibility != Visibility.Visible
                ? PrevButtonVisibility
                : AutoHideMoveButtons && SelectedPageNumber <= 1
                    ? Visibility.Collapsed
                    : Visibility.Visible;

        public Visibility ActualNextButtonVisibility =>
            NextButtonVisibility != Visibility.Visible
                ? NextButtonVisibility
                : AutoHideMoveButtons && SelectedPageNumber >= Math.Max(1, MaxPageNumber)
                    ? Visibility.Collapsed
                    : Visibility.Visible;

        public OpnxPagingItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (value == null || !value.IsEnabled)
                    return;

                if (_selectedItem == value)
                {
                    if (!_selectedItem.IsSelected)
                        _selectedItem.IsSelected = true;

                    return;
                }

                if (_selectedItem != null)
                    _selectedItem.IsSelected = false;

                _selectedItem = value;
                _selectedItem.IsSelected = true;

                OnPropertyChanged();

                if (!_isUpdatingSelection && int.TryParse(value.Content, out int pageNumber))
                    SelectedPageNumber = pageNumber;
            }
        }

        private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not OpnxPagingControl control)
                return;

            control.OnPropertyChanged(e.Property.Name);
            control.UpdateMoveButtonVisibility();
        }

        private static void OnPageButtonLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxPagingControl control)
                control.OnPropertyChanged(nameof(PageButtonActualMargin));
        }

        private static void OnPagingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not OpnxPagingControl control)
                return;

            control.OnPropertyChanged(e.Property.Name);

            switch (e.Property.Name)
            {
                case nameof(TotalPagingItemCount):
                    control.CreatePage();
                    control.RefreshPageItems();
                    control.SelectPageNumber(control.SelectedPageNumber);
                    break;

                case nameof(MaxPageNumber):
                    if (control.SelectedPageNumber < 1)
                        control.SelectedPageNumber = 1;
                    else if (control.SelectedPageNumber > control.MaxPageNumber)
                        control.SelectedPageNumber = Math.Max(1, control.MaxPageNumber);
                    else
                    {
                        control.RefreshPageItems();
                        control.SelectPageNumber(control.SelectedPageNumber);
                    }
                    break;

                case nameof(SelectedPageNumber):
                case nameof(DisplayMode):
                    control.RefreshPageItems();
                    control.SelectPageNumber(control.SelectedPageNumber);
                    break;

                case nameof(AutoHideMoveButtons):
                    control.UpdateMoveButtonVisibility();
                    break;
            }

            control.UpdateMoveButtonVisibility();
        }

        private void CreatePage()
        {
            _pagingItems.Clear();

            int count = Math.Max(1, TotalPagingItemCount);
            for (int i = 0; i < count; i++)
                _pagingItems.Add(new OpnxPagingItem());
        }

        private void RefreshPageItems()
        {
            int maxPageNumber = Math.Max(1, MaxPageNumber);
            int firstNumber = GetFirstPageNumber();

            for (int i = 0; i < _pagingItems.Count; i++)
            {
                int number = firstNumber + i;
                var item = _pagingItems[i];

                item.Content = number.ToString();
                item.IsEnabled = number <= maxPageNumber;
                item.IsSelected = false;
            }
        }

        private int GetFirstPageNumber()
        {
            int itemCount = Math.Max(1, TotalPagingItemCount);
            int selectedPageNumber = Math.Clamp(SelectedPageNumber, 1, Math.Max(1, MaxPageNumber));

            if (DisplayMode == OpnxPagingDisplayMode.Block)
                return ((selectedPageNumber - 1) / itemCount) * itemCount + 1;

            if (_pagingItems.Count == 0 || !AutoUpdatePageNumber)
                return 1;

            int currentFirstNumber = int.TryParse(_pagingItems[0].Content, out int first) ? first : 1;
            int currentLastNumber = int.TryParse(_pagingItems[^1].Content, out int last) ? last : itemCount;

            if (selectedPageNumber == currentLastNumber && selectedPageNumber < MaxPageNumber)
                return currentFirstNumber + 1;

            if (selectedPageNumber > 1 && selectedPageNumber == currentFirstNumber)
                return selectedPageNumber - 1;

            if (selectedPageNumber < currentFirstNumber || selectedPageNumber > currentLastNumber)
                return Math.Max(1, selectedPageNumber - itemCount + 1);

            return currentFirstNumber;
        }

        private void SelectPageNumber(int pageNumber)
        {
            if (pageNumber < 1 || pageNumber > Math.Max(1, MaxPageNumber))
                return;

            var findItem = _pagingItems.FirstOrDefault(x => x.IsEnabled && x.Content == pageNumber.ToString());
            if (findItem == null)
                return;

            _isUpdatingSelection = true;
            SelectedItem = findItem;
            _isUpdatingSelection = false;
        }

        private void UpdateMoveButtonVisibility()
        {
            OnPropertyChanged(nameof(ActualPrevButtonVisibility));
            OnPropertyChanged(nameof(ActualNextButtonVisibility));
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPageNumber < MaxPageNumber)
                SelectedPageNumber++;
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPageNumber > 1)
                SelectedPageNumber--;
        }

        private void ListBoxItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem { DataContext: OpnxPagingItem pagingItem })
            {
                if (!pagingItem.IsEnabled || pagingItem == _selectedItem)
                    e.Handled = true;
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton { DataContext: OpnxPagingItem pagingItem } && pagingItem.IsEnabled)
                SelectedItem = pagingItem;
        }

        private void ToolTip_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (sender is not ToggleButton { DataContext: OpnxPagingItem item })
                return;

            if (PageNumToolTipOpening == null)
            {
                e.Handled = true;
                return;
            }

            if (PageNumToolTipOpening.CanExecute(item))
                PageNumToolTipOpening.Execute(item);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

