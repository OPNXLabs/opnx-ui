using OPNX.UI.WPF.Interactivity.DragDrop;
using OPNX.UI.WPF.Utilities;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public class MultiViewCell : ContentControl, IDisposable
    {
        #region Constants and Fields
        private Border? _border = new();

        private Grid? _grid = new();

        private readonly Brush? _cellBorderBrush;

        private readonly Thickness _cellBorderThickness;

        private readonly Thickness _cellBorderThicknessForHidden;

        private readonly Brush _cellBorderBackground;
        public bool IsSelectedCell { get; set; }

        private readonly DropTargetAdvisor _targetDropAdvisor = new();
        #endregion

        #region Constructors and Destructors

        static MultiViewCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiViewCell), new FrameworkPropertyMetadata(typeof(MultiViewCell)));
        }

        internal MultiViewCell()
        {
            this._cellBorderBrush = new BrushConverter().ConvertFromString("#202228") as SolidColorBrush;
            this._cellBorderThickness = new Thickness(1);
            this._cellBorderThicknessForHidden = new Thickness(0);
            this._cellBorderBackground = Brushes.Black;

            this.ClipToBounds = true;

            this._border.Child = this._grid;

            this._border.BorderBrush = this._cellBorderBrush;
            this._border.BorderThickness = this._cellBorderThickness;
            this._border.Background = this._cellBorderBackground;

            this.Content = this._border;

            this.SyncId = Guid.NewGuid();

            _targetDropAdvisor.DropCompleted += TargetDropAdvisor_DropCompleted;
            DragDropManager.SetDropTargetAdvisor(this, _targetDropAdvisor);
        }

        #endregion

        #region Events

        internal event EventHandler<DropTargetAdvisorDropCompletedEventArgs>? DropCompleted;
        public void OnDropCompleted(DropTargetAdvisorDropCompletedEventArgs e)
        {
            DropCompleted?.Invoke(this, e);
        }

        internal event EventHandler<CellElementChangedEventArgs>? ItemAdded;

        public void OnItemAdded(CellElementChangedEventArgs e)
        {
            ItemAdded?.Invoke(this, e);
        }

        internal event EventHandler<CellElementChangedEventArgs>? ItemRemoved;

        public void OnItemRemoved(CellElementChangedEventArgs e)
        {
            ItemRemoved?.Invoke(this, e);
        }

        internal event EventHandler<EventArgs>? ViewAreaUpdated;

        public void OnViewAreaUpdated(EventArgs e)
        {
            ViewAreaUpdated?.Invoke(this, e);
        }

        internal event EventHandler<EventArgs>? SelectedCell;
        public void OnSelectedCell(EventArgs e)
        {
            SelectedCell?.Invoke(this, e);
        }

        public event EventHandler? PreparedRendering;

        public void OnPreparedRendering(EventArgs e)
        {
            PreparedRendering?.Invoke(this, e);
        }

        #endregion

        #region Properties
        public Grid? InnerGrid => _grid;
        public bool UseGridGuidLines
        {
            get { return (bool)this.GetValue(UseGridGuidLinesProperty); }
            set { this.SetValue(UseGridGuidLinesProperty, value); }
        }

        public string UseGridGuidLineColor
        {
            get { return (string)this.GetValue(UseGridGuidLineColorProperty); }
            set { this.SetValue(UseGridGuidLineColorProperty, value); }
        }


        public int UseGridGuidLineThickness
        {
            get { return (int)this.GetValue(UseGridGuidLineThicknessProperty); }
            set { this.SetValue(UseGridGuidLineThicknessProperty, value); }
        }

        public string CellBackground
        {
            get { return (string)this.GetValue(CellBackgroundProperty); }
            set { this.SetValue(CellBackgroundProperty, value); }
        }

        public bool HasSameTargetForDragAndDrop
        {
            get;
            set;
        }

        public bool IsZoomed
        {
            get { return (bool)this.GetValue(IsZoomedProperty); }
            set { this.SetValue(IsZoomedProperty, value); }
        }

        public Guid SyncId { get; set; }

        public Rect RectForCanvas => MultiViewPanel.GetLocation(this);
        #endregion

        #region Denpendency Properties
        internal static readonly DependencyProperty IsZoomedProperty = DependencyProperty.Register(
           nameof(IsZoomed), typeof(bool), typeof(MultiViewCell), new FrameworkPropertyMetadata(false));

        internal static readonly DependencyProperty UseGridGuidLinesProperty = DependencyProperty.Register(
           nameof(UseGridGuidLines), typeof(bool), typeof(MultiViewCell), new FrameworkPropertyMetadata(false));

        internal static readonly DependencyProperty UseGridGuidLineColorProperty = DependencyProperty.Register(
            nameof(UseGridGuidLineColor), typeof(string), typeof(MultiViewCell), new FrameworkPropertyMetadata("#A9A9A9"));

        internal static readonly DependencyProperty UseGridGuidLineThicknessProperty = DependencyProperty.Register(
            nameof(UseGridGuidLineThickness), typeof(int), typeof(MultiViewCell), new FrameworkPropertyMetadata(10));

        internal static readonly DependencyProperty CellBackgroundProperty = DependencyProperty.Register(
            nameof(CellBackground), typeof(string), typeof(MultiViewCell), new FrameworkPropertyMetadata(string.Empty));

        #endregion

        #region Methods

        private void TargetDropAdvisor_DropCompleted(object? sender, DropTargetAdvisorDropCompletedEventArgs e)
        {
            OnDropCompleted(e);
        }

        public void HideBorder()
        {
            if (this._border == null)
            {
                return;
            }

            this._border.BorderThickness = this._cellBorderThicknessForHidden;
        }

        public void ShowBorder()
        {
            if (this._border == null)
            {
                return;
            }

            this._border.BorderThickness = this._cellBorderThickness;
        }

        internal void UpdateElementSizeBinding()
        {
            this.OnViewAreaUpdated(new EventArgs());
        }

        internal static void UnbindElementSize(UIElement element)
        {
            if (element is FrameworkElement)
            {
                BindingOperations.ClearBinding(element, WidthProperty);
                BindingOperations.ClearBinding(element, HeightProperty);
            }
        }

        internal void AddElementWithoutBinding(UIElement element, int index = -1)
        {
            var children = _grid?.Children;
            if (children == null)
                return;

            if (index >= 0 && index <= children.Count)
            {
                children.Insert(index, element);
            }
            else
            {
                children.Add(element);
            }

            OnItemAdded(new CellElementChangedEventArgs(element));
            OnViewAreaUpdated(EventArgs.Empty);
        }

        internal void Add(UIElement element)
        {
            AddElement(element);

            this.UpdateElementSizeBinding();

            this.OnItemAdded(new CellElementChangedEventArgs(element));
        }

        internal void AddElement(UIElement element)
        {
            this.UpdateElementSizeBinding();

            this._grid?.Children.Add(element);
        }

        internal void Clear()
        {
            if (_grid == null) return;

            int count = _grid.Children.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                Remove(_grid.Children[i]);
            }
        }

        internal void ClearElements()
        {
            if (_grid == null) return;

            _grid.Children.Clear();
        }

        internal List<UIElement> GetAllElements()
        {
            return this._grid?.Children.Cast<UIElement>().ToList() ?? [];
        }

        internal UIElement? GetElement(int index)
        {
            return this._grid?.Children[index];
        }

        internal int? GetElementCount()
        {
            return this._grid?.Children.Count;
        }

        internal UIElement? GetVisibleElement()
        {
            if (this._grid == null) return null;

            return this._grid.Children.Cast<UIElement>().FirstOrDefault(x => x.Visibility == Visibility.Visible);
        }
        internal void Remove(UIElement element, bool aEventOccurrence = true)
        {
            if (element == null) return;

            if (element is IDisposable disposable)
            {
                disposable.Dispose();
            }

            this._grid?.Children.Remove(element);

            if (!aEventOccurrence) return;

            this.OnItemRemoved(new CellElementChangedEventArgs(element));
            this.OnViewAreaUpdated(new EventArgs());
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            var name = e.Property.Name;

            bool shouldUpdate = name == "Left" || name == "Top" ||
                                name == nameof(ActualWidth) || name == nameof(ActualHeight) ||
                                (name == nameof(IsVisible) && (bool)e.NewValue);

            // Hidden transitions are ignored because they do not produce a usable video display area.
            if (shouldUpdate)
            {
                OnViewAreaUpdated(EventArgs.Empty);
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
        }

        public void Dispose()
        {
            try
            {

                _targetDropAdvisor.DropCompleted -= TargetDropAdvisor_DropCompleted;
                DragDropManager.SetDropTargetAdvisor(this, null);

                DragDropManager.SetDragSourceAdvisor(this, null);

                this.Clear();

                UIHelper.RemoveRoutedEventHandlers(_grid);

                UIHelper.RemoveEventHandlerByReflection(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("[Cell Dispose Error] {0}", ex));
            }
            finally
            {
                _border = null;
                _grid = null;
            }

            GC.SuppressFinalize(this);
        }
        #endregion
    }

    [Serializable]
    public class CellSplitException : ApplicationException
    {
        public CellSplitException(string message)
            : base(message)
        {
        }

        public CellSplitException(string message, string cellStatus)
            : base(message)
        {
            CellStatus = cellStatus;
        }

        public string? CellStatus { get; set; }
    }
}






