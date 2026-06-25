using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    [TemplatePart(Name = HeaderScrollViewerPartName, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = ScrollButtonPanelPartName, Type = typeof(UIElement))]
    [TemplatePart(Name = LeftButtonPartName, Type = typeof(ButtonBase))]
    [TemplatePart(Name = RightButtonPartName, Type = typeof(ButtonBase))]
    public class OpnxTabControl : TabControl
    {
        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register(
                nameof(HeaderBackground),
                typeof(Brush),
                typeof(OpnxTabControl),
                new FrameworkPropertyMetadata(Brushes.Transparent));

        private const string HeaderScrollViewerPartName = "PART_HeaderScrollViewer";
        private const string ScrollButtonPanelPartName = "PART_ScrollButtonPanel";
        private const string LeftButtonPartName = "PART_LeftButton";
        private const string RightButtonPartName = "PART_RightButton";
        private const double ScrollStep = 100d;

        private ScrollViewer? _headerScrollViewer;
        private UIElement? _scrollButtonPanel;
        private ButtonBase? _leftButton;
        private ButtonBase? _rightButton;

        static OpnxTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxTabControl),
                new FrameworkPropertyMetadata(typeof(OpnxTabControl)));
        }

        public Brush HeaderBackground
        {
            get => (Brush)GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }

        public override void OnApplyTemplate()
        {
            DetachTemplateEvents();

            base.OnApplyTemplate();

            _headerScrollViewer = GetTemplateChild(HeaderScrollViewerPartName) as ScrollViewer;
            _scrollButtonPanel = GetTemplateChild(ScrollButtonPanelPartName) as UIElement;
            _leftButton = GetTemplateChild(LeftButtonPartName) as ButtonBase;
            _rightButton = GetTemplateChild(RightButtonPartName) as ButtonBase;

            AttachTemplateEvents();
            UpdateScrollButtons();
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            UpdateScrollButtons();
        }

        private void AttachTemplateEvents()
        {
            if (_headerScrollViewer is not null)
            {
                _headerScrollViewer.SizeChanged += HeaderScrollViewer_SizeChanged;
                _headerScrollViewer.ScrollChanged += HeaderScrollViewer_ScrollChanged;
            }

            if (_leftButton is not null)
            {
                _leftButton.Click += LeftButton_Click;
            }

            if (_rightButton is not null)
            {
                _rightButton.Click += RightButton_Click;
            }
        }

        private void DetachTemplateEvents()
        {
            if (_headerScrollViewer is not null)
            {
                _headerScrollViewer.SizeChanged -= HeaderScrollViewer_SizeChanged;
                _headerScrollViewer.ScrollChanged -= HeaderScrollViewer_ScrollChanged;
            }

            if (_leftButton is not null)
            {
                _leftButton.Click -= LeftButton_Click;
            }

            if (_rightButton is not null)
            {
                _rightButton.Click -= RightButton_Click;
            }
        }

        private void HeaderScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScrollButtons();
        }

        private void HeaderScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateScrollButtons();
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            ScrollHeader(-ScrollStep);
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            ScrollHeader(ScrollStep);
        }

        private void ScrollHeader(double offset)
        {
            if (_headerScrollViewer is null)
                return;

            _headerScrollViewer.ScrollToHorizontalOffset(_headerScrollViewer.HorizontalOffset + offset);
        }

        private void UpdateScrollButtons()
        {
            if (_headerScrollViewer is null)
                return;

            bool isOverflowing = _headerScrollViewer.ExtentWidth > _headerScrollViewer.ViewportWidth;

            if (_scrollButtonPanel is not null)
            {
                _scrollButtonPanel.Visibility = isOverflowing ? Visibility.Visible : Visibility.Collapsed;
            }

            if (_leftButton is not null)
            {
                _leftButton.IsEnabled = isOverflowing && _headerScrollViewer.HorizontalOffset > 0;
            }

            if (_rightButton is not null)
            {
                _rightButton.IsEnabled =
                    isOverflowing &&
                    _headerScrollViewer.HorizontalOffset < _headerScrollViewer.ScrollableWidth;
            }
        }
    }
}
