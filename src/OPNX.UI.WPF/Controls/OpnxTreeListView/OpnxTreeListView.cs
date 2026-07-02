using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace OPNX.UI.WPF.Controls
{
    public class OpnxTreeListView : DataGrid
    {
        private static readonly Brush DefaultColumnHeaderBackground = CreateDefaultColumnHeaderBackground();
        private static readonly Brush DefaultRowDropLineBrush = CreateDefaultRowDropLineBrush();
        private static readonly string RowDropDataFormat = typeof(OpnxTreeListView).FullName + ".RowDrop";

        private readonly Dictionary<object, TreeListViewNode> _nodesByItem;
        private readonly List<TreeListViewNode> _rootNodes = [];
        private readonly List<object> _visibleItems;
        private readonly ListCollectionView _visibleItemsView;
        private readonly TreeListViewTreeBuilder _treeBuilder;
        private readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _filterPropertiesByType = [];

        private DataGridColumn? _treeColumn = null;

        private bool _isChangingItemsSource;
        private bool _wasPreviousLeftButtonDownHandled;
        private Point? _rowDropDragStartPoint;
        private object? _rowDropDraggedItem;
        private TreeListViewRow? _rowDropTargetRow;
        private RowDropLineAdorner? _rowDropLineAdorner;
        private OpnxTreeListViewRowDropPosition _rowDropPosition;

        public event EventHandler<OpnxTreeListViewRowDropEventArgs>? RowDrop;

        static OpnxTreeListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxTreeListView),
                new FrameworkPropertyMetadata(typeof(OpnxTreeListView)));
        }

        public OpnxTreeListView()
        {
            AutoGenerateColumns = false;
            CanUserAddRows = false;
            _nodesByItem = [];

            _visibleItems = [];

            _visibleItemsView = new ListCollectionView(_visibleItems);
            _treeBuilder = new TreeListViewTreeBuilder(this);
            base.ItemsSource = _visibleItemsView;

            base.Sorting += DataGridTree_Sorting;
        }

        public List<object> VisibleItems => _visibleItems;
        public ListCollectionView VisibleItemsView => _visibleItemsView;
        public Dictionary<object, TreeListViewNode> NodesByItem => _nodesByItem;

        public List<TreeListViewNode> RootNodes => _rootNodes;

        public Brush ColumnHeaderBackground
        {
            get => (Brush)GetValue(ColumnHeaderBackgroundProperty);
            set => SetValue(ColumnHeaderBackgroundProperty, value);
        }

        public static readonly DependencyProperty ColumnHeaderBackgroundProperty =
            DependencyProperty.Register(
                nameof(ColumnHeaderBackground),
                typeof(Brush),
                typeof(OpnxTreeListView),
                new FrameworkPropertyMetadata(DefaultColumnHeaderBackground));

        private static Brush CreateDefaultColumnHeaderBackground()
        {
            var brush = new SolidColorBrush(Color.FromRgb(0x0D, 0x0D, 0x10));
            brush.Freeze();
            return brush;
        }

        private static Brush CreateDefaultRowDropLineBrush()
        {
            var brush = new SolidColorBrush(Color.FromRgb(0x4A, 0x6F, 0xE3));
            brush.Freeze();
            return brush;
        }

        public bool CanDropRows
        {
            get => (bool)GetValue(CanDropRowsProperty);
            set => SetValue(CanDropRowsProperty, value);
        }

        public static readonly DependencyProperty CanDropRowsProperty =
            DependencyProperty.Register(
                nameof(CanDropRows),
                typeof(bool),
                typeof(OpnxTreeListView),
                new FrameworkPropertyMetadata(false, OnCanDropRowsChanged));

        private static void OnCanDropRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxTreeListView treeListView)
            {
                treeListView.AllowDrop = (bool)e.NewValue;
                if ((bool)e.NewValue == false)
                {
                    treeListView.ClearRowDropState();
                }
            }
        }

        public Brush RowDropLineBrush
        {
            get => (Brush)GetValue(RowDropLineBrushProperty);
            set => SetValue(RowDropLineBrushProperty, value);
        }

        public static readonly DependencyProperty RowDropLineBrushProperty =
            DependencyProperty.Register(
                nameof(RowDropLineBrush),
                typeof(Brush),
                typeof(OpnxTreeListView),
                new FrameworkPropertyMetadata(DefaultRowDropLineBrush));

        public double RowDropLineThickness
        {
            get => (double)GetValue(RowDropLineThicknessProperty);
            set => SetValue(RowDropLineThicknessProperty, value);
        }

        public static readonly DependencyProperty RowDropLineThicknessProperty =
            DependencyProperty.Register(
                nameof(RowDropLineThickness),
                typeof(double),
                typeof(OpnxTreeListView),
                new FrameworkPropertyMetadata(2d));

        public ICommand? RowDropCommand
        {
            get => (ICommand?)GetValue(RowDropCommandProperty);
            set => SetValue(RowDropCommandProperty, value);
        }

        public static readonly DependencyProperty RowDropCommandProperty =
            DependencyProperty.Register(
                nameof(RowDropCommand),
                typeof(ICommand),
                typeof(OpnxTreeListView),
                new FrameworkPropertyMetadata(null));

        public double IndentWidth
        {
            get => (double)GetValue(IndentWidthProperty);
            set => SetValue(IndentWidthProperty, value);
        }

        public static readonly DependencyProperty IndentWidthProperty =
            DependencyProperty.Register(
                nameof(IndentWidth),
                typeof(double),
                typeof(OpnxTreeListView),
                new FrameworkPropertyMetadata(
                    8d,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnIndentWidthChanged));

        private static void OnIndentWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeListView = (OpnxTreeListView)d;
            var indentWidth = (double)e.NewValue;

            foreach (object item in treeListView.Items)
            {
                if (treeListView.ItemContainerGenerator.ContainerFromItem(item) is TreeListViewRow row)
                {
                    row.UpdateIndent(indentWidth);
                }
            }
        }


        public object TreeCellHeader
        {
            get { return (object)GetValue(TreeCellHeaderProperty); }
            set { SetValue(TreeCellHeaderProperty, value); }
        }
        public static readonly DependencyProperty TreeCellHeaderProperty =
            DependencyProperty.Register(nameof(TreeCellHeader), typeof(object), typeof(OpnxTreeListView), new PropertyMetadata(null));

        public DataGridColumn? TreeCellTemplate
        {
            get { return (DataGridColumn)GetValue(TreeCellTemplateProperty); }
            set { SetValue(TreeCellTemplateProperty, value); }
        }
        public static readonly DependencyProperty TreeCellTemplateProperty =
            DependencyProperty.Register(nameof(TreeCellTemplate), typeof(DataGridColumn), typeof(OpnxTreeListView), new PropertyMetadata(null));

        public object TreeCell
        {
            get { return (object)GetValue(TreeCellProperty); }
            set { SetValue(TreeCellProperty, value); }
        }
        public static readonly DependencyProperty TreeCellProperty =
            DependencyProperty.Register(nameof(TreeCell), typeof(object), typeof(OpnxTreeListView), new PropertyMetadata(null));

        public PropertyPath IdPath
        {
            get { return (PropertyPath)GetValue(IdPathProperty); }
            set { SetValue(IdPathProperty, value); }
        }
        public static readonly DependencyProperty IdPathProperty =
            DependencyProperty.Register(nameof(IdPath), typeof(PropertyPath), typeof(OpnxTreeListView), new PropertyMetadata(default));

        public PropertyPath ParentIdPath
        {
            get { return (PropertyPath)GetValue(ParentIdPathProperty); }
            set { SetValue(ParentIdPathProperty, value); }
        }
        public static readonly DependencyProperty ParentIdPathProperty =
            DependencyProperty.Register(nameof(ParentIdPath), typeof(PropertyPath), typeof(OpnxTreeListView), new PropertyMetadata(default));

        public bool ExpandAll
        {
            get { return (bool)GetValue(ExpandAllProperty); }
            set { SetValue(ExpandAllProperty, value); }
        }
        public static readonly DependencyProperty ExpandAllProperty =
            DependencyProperty.Register(nameof(ExpandAll), typeof(bool), typeof(OpnxTreeListView), new PropertyMetadata(false, ExpandAllPropertyChangedCallback));

        public string FilterText
        {
            get { return (string)GetValue(FilterTextProperty); }
            set { SetValue(FilterTextProperty, value); }
        }

        public static readonly DependencyProperty FilterTextProperty =
            DependencyProperty.Register(nameof(FilterText), typeof(string), typeof(OpnxTreeListView), new PropertyMetadata(string.Empty, OnFilterTextChanged));


        public Brush? SelectedBackground
        {
            get => (Brush?)GetValue(SelectedBackgroundProperty);
            set => SetValue(SelectedBackgroundProperty, value);
        }

        public static readonly DependencyProperty SelectedBackgroundProperty =
            DependencyProperty.Register(
                nameof(SelectedBackground),
                typeof(Brush),
                typeof(OpnxTreeListView),
                new FrameworkPropertyMetadata(null));

        public Brush? MouseOverBackground
        {
            get => (Brush?)GetValue(MouseOverBackgroundProperty);
            set => SetValue(MouseOverBackgroundProperty, value);
        }

        public static readonly DependencyProperty MouseOverBackgroundProperty =
            DependencyProperty.Register(
                nameof(MouseOverBackground),
                typeof(Brush),
                typeof(OpnxTreeListView),
                new FrameworkPropertyMetadata(null));

        public double RowSpacing
        {
            get => (double)GetValue(RowSpacingProperty);
            set => SetValue(RowSpacingProperty, value);
        }

        public static readonly DependencyProperty RowSpacingProperty =
            DependencyProperty.Register(
                nameof(RowSpacing),
                typeof(double),
                typeof(OpnxTreeListView),
                new FrameworkPropertyMetadata(
                    0d,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnRowSpacingChanged,
                    CoerceRowSpacing));

        private static object CoerceRowSpacing(DependencyObject d, object baseValue)
        {
            return Math.Max(0d, (double)baseValue);
        }

        private static void OnRowSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeListView = (OpnxTreeListView)d;

            foreach (object item in treeListView.Items)
            {
                if (treeListView.ItemContainerGenerator.ContainerFromItem(item) is TreeListViewRow row)
                {
                    treeListView.ApplyRowSpacing(row);
                }
            }
        }

        private void ApplyRowSpacing(TreeListViewRow row)
        {
            row.Margin = new Thickness(0, 0, 0, RowSpacing);
        }


        private static void OnFilterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tree = (OpnxTreeListView)d;
            tree.SetFilterText(e.NewValue as string ?? string.Empty);
        }

        private static void ExpandAllPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tree = (OpnxTreeListView)d;
            if (e.NewValue is bool expand)
            {
                tree.SetIsOpenAll(expand);
            }
        }

        private void SetIsOpenAll(bool isOpen)
        {
            _visibleItemsView.CancelEdit();
            foreach (var rootNode in _rootNodes)
            {
                rootNode.SetIsOpenAll(isOpen);
            }
            _visibleItemsView.Refresh();
        }

        private void SetFilterText(string filterText)
        {
            List<string> filterNames = [];
            foreach (var column in Columns)
            {
                if (column is OpnxTreeListViewTemplateColumn treeListViewTemplateColumn)
                {
                    if (!treeListViewTemplateColumn.IsColumnFiltered)
                        continue;

                    if (!string.IsNullOrEmpty(treeListViewTemplateColumn.FieldName))
                        filterNames.Add(treeListViewTemplateColumn.FieldName);
                }
                else if (column is OpnxTreeListViewTextColumn treeListViewTextColumn)
                {
                    if (!treeListViewTextColumn.IsColumnFiltered)
                        continue;

                    if (!string.IsNullOrEmpty(treeListViewTextColumn.FieldName))
                        filterNames.Add(treeListViewTextColumn.FieldName);
                }
            }

            if (string.IsNullOrWhiteSpace(filterText) || filterNames.Count == 0)
            {
                VisibleItemsView.Filter = null;
                VisibleItemsView.Refresh();
                return;
            }

            VisibleItemsView.Filter = e =>
            {
                Type objectType = e.GetType();
                var propertiesByName = GetFilterProperties(objectType);

                bool result = false;
                foreach (string filterName in filterNames)
                {
                    if (propertiesByName.TryGetValue(filterName, out var propertyInfo))
                    {
                        string? value = Convert.ToString(propertyInfo.GetValue(e));
                        if (!string.IsNullOrEmpty(value))
                        {
                            result = value.Contains(filterText, StringComparison.OrdinalIgnoreCase);
                            if (result)
                                break;
                        }
                    }
                }

                return result;
            };

            VisibleItemsView.Refresh();
        }

        private Dictionary<string, PropertyInfo> GetFilterProperties(Type objectType)
        {
            if (_filterPropertiesByType.TryGetValue(objectType, out var propertiesByName))
                return propertiesByName;

            propertiesByName = objectType.GetProperties()
                .GroupBy(property => property.Name)
                .ToDictionary(group => group.Key, group => group.First());

            _filterPropertiesByType[objectType] = propertiesByName;
            return propertiesByName;
        }

        [Bindable(true),]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set
            {
                if (value == null)
                {
                    ClearValue(ItemsSourceProperty);
                }
                else
                {
                    SetValue(ItemsSourceProperty, value);
                }
            }
        }
        public new static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(OpnxTreeListView),
                                                                                new FrameworkPropertyMetadata(null,
                                                                                new PropertyChangedCallback(OnItemsSourceChanged)));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OpnxTreeListView tree = (OpnxTreeListView)d;
            IEnumerable oldValue = (IEnumerable)e.OldValue;
            IEnumerable newValue = (IEnumerable)e.NewValue;

            ((IContainItemStorage)tree).Clear();

            tree._filterPropertiesByType.Clear();
            tree.RefreshTree();
            tree._isChangingItemsSource = true;
            try
            {
                tree.OnItemsSourceChanged(oldValue, newValue);
            }
            finally
            {
                tree._isChangingItemsSource = false;
            }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (_isChangingItemsSource == false) return;

            if (oldValue is INotifyCollectionChanged oldVal)
            {
                oldVal.CollectionChanged -= Notify_CollectionChanged;
            }
            if (newValue is INotifyCollectionChanged newVal)
            {
                newVal.CollectionChanged += Notify_CollectionChanged;
            }

            base.OnItemsSourceChanged(oldValue, newValue);
        }

        private void Notify_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender != this.ItemsSource) return;

            RefreshTree();
        }

        private void ApplyTreeColumn()
        {
            if (_treeColumn != null || TreeCellTemplate == null)
                return;

            _treeColumn = TreeCellTemplate;
            base.Columns.Insert(0, _treeColumn);
        }

        public override void OnApplyTemplate()
        {
            ApplyTreeColumn();
            base.OnApplyTemplate();
            //XamlObjectReader lx = new XamlObjectReader(null);
            //lx.Read();

        }

        private void RefreshTree()
        {
            _visibleItemsView.CancelEdit();
            _treeBuilder.BuildTree();
            _visibleItemsView.Refresh();
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var row = new TreeListViewRow();
            return row;
        }
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var row = (TreeListViewRow)element;
            //row.Resources["Part_TreeGrid_TreeCell_Default"] = this.TreeCellBinding ?? this.TreeCell;

            base.PrepareContainerForItemOverride(element, item);
            ApplyRowSpacing(row);
            //BindingBase binding;
            //if (this.TreeCellBinding == null) { binding = new Binding(nameof(TreeCell)) { Source = this }; }
            //else
            //{
            //    binding = processBinding(this.TreeCellBinding, item);
            //}
            if (_nodesByItem.TryGetValue(item, out var node))
            {
                row.SetNode(node);
                row.UpdateIndent(IndentWidth);
            }
            //ctlData.TreeGridRow = row;
            //BindingOperations.SetBinding(row.TreeRowCtlData, TreeRowCtlData.TreeCellProperty, binding);
        }

        private void DataGridTree_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = false;
        }

        //private static DataTemplate GetTreeCellTemplate(object treeCell)
        //{
        //    using var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("MagicDataGridTree.DataGridTreeStyles.xaml")!);
        //    var cellTempStr = reader.ReadToEnd();
        //    object resource = null;
        //    if (treeCell != null)
        //    {
        //        var cellStr = "";
        //        switch (treeCell)
        //        {
        //            case ControlTemplate dataTemplate:
        //                var ttt = dataTemplate.LoadContent();
        //                //dataTemplate.Template
        //                var cellStr1 = System.Windows.Markup.XamlWriter.Save(ttt);
        //                cellStr = " Template=\"{DynamicResource __DataGridTree__TreeCell_Template__}\" />";
        //                resource = dataTemplate;
        //                break;
        //            case DataTemplate dataTemplate:
        //                var ttt2 = dataTemplate.LoadContent();

        //                var cellStr12 = System.Windows.Markup.XamlWriter.Save(dataTemplate.Template);
        //                cellStr = " Template=\"{DynamicResource __DataGridTree__TreeCell_Template__}\" />";
        //                resource = dataTemplate;
        //                break;
        //            default:
        //                cellStr = ">" + System.Windows.Markup.XamlWriter.Save(treeCell) + "</ContentControl>";
        //                break;
        //        }
        //        cellTempStr = cellTempStr.Replace("></ContentControl>", cellStr);
        //    }
        //    StringReader stringReader = new(cellTempStr);
        //    XmlReader xmlReader = XmlReader.Create(stringReader);
        //    var dataTemplateRlt = (DataTemplate)System.Windows.Markup.XamlReader.Load(xmlReader);
        //    if (resource != null)
        //    {
        //        dataTemplateRlt.Resources["__DataGridTree__TreeCell_Template__"] = resource;
        //    }

        //    return dataTemplateRlt;
        //}

        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            base.OnSorting(eventArgs);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (ItemsControl.ContainerFromElement((OpnxTreeListView)this, e.OriginalSource as DependencyObject) is not TreeListViewRow treeListControlRow)
                return;

            if (CanDropRows && IsRowDropIgnoredSource(e.OriginalSource as DependencyObject) == false)
            {
                _rowDropDragStartPoint = e.GetPosition(this);
                _rowDropDraggedItem = treeListControlRow.DataContext;
            }

            var selectedItems = SelectedItems.Cast<dynamic>();
            if (selectedItems?.Skip(1).Any() == true && selectedItems.Any(x => x == treeListControlRow.DataContext))
            {
                e.Handled = true;
            }
            //if (selectedItems != null && selectedItems.Count() > 1)
            //{
            //    if (selectedItems.Any(x => x == treeListControlRow.DataContext))
            //    {
            //        e.Handled = true;
            //    }
            //}

            _wasPreviousLeftButtonDownHandled = e.Handled;
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);
            _rowDropDragStartPoint = null;
            _rowDropDraggedItem = null;

            if (_wasPreviousLeftButtonDownHandled)
            {
                if (ItemsControl.ContainerFromElement((OpnxTreeListView)this, e.OriginalSource as DependencyObject) is TreeListViewRow treeListControlRow)
                {
                    SelectedItems.Clear();
                    SelectedItem = treeListControlRow.DataContext;
                }
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (!CanDropRows ||
                _rowDropDragStartPoint is not Point dragStartPoint ||
                _rowDropDraggedItem is null ||
                e.LeftButton != MouseButtonState.Pressed)
                return;

            Point currentPoint = e.GetPosition(this);
            if (Math.Abs(currentPoint.X - dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(currentPoint.Y - dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            var dataObject = new DataObject(RowDropDataFormat, new RowDropPayload(this, _rowDropDraggedItem));
            DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Move);
            ClearRowDropState();
            e.Handled = true;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);

            if (!CanDropRows || !TryGetRowDropPayload(e.Data, out var payload) || payload.Source != this)
            {
                ClearRowDropLine();
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            if (GetRowFromElement(e.OriginalSource as DependencyObject) is not TreeListViewRow targetRow ||
                ReferenceEquals(targetRow.DataContext, payload.Item))
            {
                ClearRowDropLine();
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            Point rowPoint = e.GetPosition(targetRow);
            var position = rowPoint.Y < targetRow.ActualHeight / 2
                ? OpnxTreeListViewRowDropPosition.Before
                : OpnxTreeListViewRowDropPosition.After;

            ShowRowDropLine(targetRow, position);
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);

            if (CanDropRows &&
                TryGetRowDropPayload(e.Data, out var payload) &&
                payload.Source == this &&
                _rowDropTargetRow is not null)
            {
                object targetItem = _rowDropTargetRow.DataContext;
                int draggedIndex = VisibleItems.IndexOf(payload.Item);
                int targetIndex = VisibleItems.IndexOf(targetItem);
                int insertIndex = _rowDropPosition == OpnxTreeListViewRowDropPosition.After
                    ? targetIndex + 1
                    : targetIndex;

                var dropEventArgs = new OpnxTreeListViewRowDropEventArgs(
                    payload.Item,
                    targetItem,
                    draggedIndex,
                    targetIndex,
                    insertIndex,
                    _rowDropPosition);

                RowDrop?.Invoke(this, dropEventArgs);
                if (RowDropCommand?.CanExecute(dropEventArgs) == true)
                {
                    RowDropCommand.Execute(dropEventArgs);
                }

                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }

            ClearRowDropState();
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);

            if (!IsMouseOver)
            {
                ClearRowDropLine();
            }
        }

        private static bool TryGetRowDropPayload(IDataObject dataObject, out RowDropPayload payload)
        {
            if (dataObject.GetDataPresent(RowDropDataFormat) &&
                dataObject.GetData(RowDropDataFormat) is RowDropPayload rowDropPayload)
            {
                payload = rowDropPayload;
                return true;
            }

            payload = default;
            return false;
        }

        private static bool IsRowDropIgnoredSource(DependencyObject? source)
        {
            while (source is not null)
            {
                if (source is TreeListViewRow)
                    return false;

                if (source is TextBoxBase ||
                    source is ButtonBase ||
                    source is Selector ||
                    source is PasswordBox ||
                    source is Slider)
                    return true;

                source = VisualTreeHelper.GetParent(source);
            }

            return false;
        }

        private TreeListViewRow? GetRowFromElement(DependencyObject? source)
        {
            while (source is not null)
            {
                if (source is TreeListViewRow row)
                    return row;

                source = VisualTreeHelper.GetParent(source);
            }

            return null;
        }

        private void ShowRowDropLine(TreeListViewRow targetRow, OpnxTreeListViewRowDropPosition position)
        {
            if (_rowDropTargetRow == targetRow && _rowDropPosition == position && _rowDropLineAdorner is not null)
                return;

            ClearRowDropLine();

            var layer = AdornerLayer.GetAdornerLayer(targetRow);
            if (layer is null)
                return;

            _rowDropTargetRow = targetRow;
            _rowDropPosition = position;
            _rowDropLineAdorner = new RowDropLineAdorner(targetRow, position, RowDropLineBrush, RowDropLineThickness);
            layer.Add(_rowDropLineAdorner);
        }

        private void ClearRowDropState()
        {
            _rowDropDragStartPoint = null;
            _rowDropDraggedItem = null;
            ClearRowDropLine();
        }

        private void ClearRowDropLine()
        {
            if (_rowDropLineAdorner is not null && _rowDropTargetRow is not null)
            {
                var layer = AdornerLayer.GetAdornerLayer(_rowDropTargetRow);
                layer?.Remove(_rowDropLineAdorner);
            }

            _rowDropTargetRow = null;
            _rowDropLineAdorner = null;
        }

        //private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        //{
        //    while (current != null)
        //    {
        //        if (current is T ancestor)
        //        {
        //            return ancestor;
        //        }
        //        current = VisualTreeHelper.GetParent(current);
        //    }
        //    return null;
        //}

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.WidthChanged)
            {
                // 약간의 지연을 두고 실행 (렌더링 완료 후)
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ResetColumnWidths();
                }), DispatcherPriority.Loaded);
            }
        }


        private void ResetColumnWidths()
        {
            if (this.ActualWidth <= 0) return;

            var starColumns = this.Columns.Where(c => c.Width.IsStar).ToList();

            if (starColumns.Count > 0)
            {
                double scrollBarWidth = 17;
                double availableWidth = this.ActualWidth - scrollBarWidth;

                double fixedWidth = this.Columns
                    .Where(c => !c.Width.IsStar && !c.Width.IsAuto)
                    .Sum(c => c.Width.Value);

                double autoWidth = this.Columns
                    .Where(c => c.Width.IsAuto)
                    .Sum(c => c.ActualWidth > 0 ? c.ActualWidth : (c.MinWidth > 0 ? c.MinWidth : 100));

                double remainingWidth = Math.Max(0, availableWidth - fixedWidth - autoWidth);

                double totalStarValue = starColumns.Sum(c => c.Width.Value);

                if (totalStarValue > 0 && remainingWidth > 0)
                {
                    foreach (var col in starColumns)
                    {
                        double ratio = col.Width.Value / totalStarValue;
                        double newWidth = remainingWidth * ratio;


                        if (double.IsNaN(col.MinWidth) || col.MinWidth <= 0)
                            col.MinWidth = 30;

                        newWidth = Math.Max(newWidth, col.MinWidth);


                        if (!double.IsNaN(col.MaxWidth) && col.MaxWidth > 0)
                            newWidth = Math.Min(newWidth, col.MaxWidth);


                        col.Width = new DataGridLength(newWidth, DataGridLengthUnitType.Pixel);

                        col.Width = new DataGridLength(col.Width.Value, DataGridLengthUnitType.Star);
                    }
                }
            }

            this.UpdateLayout();
        }
    }

    public enum OpnxTreeListViewRowDropPosition
    {
        Before,
        After
    }

    public sealed class OpnxTreeListViewRowDropEventArgs(
        object draggedItem,
        object targetItem,
        int draggedIndex,
        int targetIndex,
        int insertIndex,
        OpnxTreeListViewRowDropPosition dropPosition) : EventArgs
    {
        public object DraggedItem { get; } = draggedItem;
        public object TargetItem { get; } = targetItem;
        public int DraggedIndex { get; } = draggedIndex;
        public int TargetIndex { get; } = targetIndex;
        public int InsertIndex { get; } = insertIndex;
        public OpnxTreeListViewRowDropPosition DropPosition { get; } = dropPosition;
    }

    internal readonly record struct RowDropPayload(OpnxTreeListView Source, object Item);

    internal sealed class RowDropLineAdorner(
        UIElement adornedElement,
        OpnxTreeListViewRowDropPosition position,
        Brush brush,
        double thickness) : Adorner(adornedElement)
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var pen = new Pen(brush, Math.Max(1d, thickness));
            double y = position == OpnxTreeListViewRowDropPosition.Before
                ? pen.Thickness / 2
                : RenderSize.Height - pen.Thickness / 2;

            drawingContext.DrawLine(pen, new Point(0, y), new Point(RenderSize.Width, y));
        }
    }
}
