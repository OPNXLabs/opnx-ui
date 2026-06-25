using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace OPNX.UI.WPF.Controls
{
    public class TreeListViewRow : DataGridRow
    {
        private double _indentWidth;

        #region Properties

        public TreeListViewNode? Node { get; private set; }

        public bool Expanded
        {
            get => (bool)GetValue(ExpandedProperty);
            set => SetValue(ExpandedProperty, value);
        }

        public static readonly DependencyProperty ExpandedProperty =
            DependencyProperty.Register(
                nameof(Expanded),
                typeof(bool),
                typeof(TreeListViewRow),
                new PropertyMetadata(false, OnExpandedChanged));

        public Visibility ExpanderVisibility
        {
            get => (Visibility)GetValue(ExpanderVisibilityProperty);
            set => SetValue(ExpanderVisibilityProperty, value);
        }

        public static readonly DependencyProperty ExpanderVisibilityProperty =
            DependencyProperty.Register(
                nameof(ExpanderVisibility),
                typeof(Visibility),
                typeof(TreeListViewRow),
                new PropertyMetadata(Visibility.Visible));

        public bool HasChild
        {
            get => (bool)GetValue(HasChildProperty);
            set => SetValue(HasChildProperty, value);
        }

        public static readonly DependencyProperty HasChildProperty =
            DependencyProperty.Register(
                nameof(HasChild),
                typeof(bool),
                typeof(TreeListViewRow),
                new PropertyMetadata(false));

        public int ChildrenCount
        {
            get => (int)GetValue(ChildrenCountProperty);
            private set => SetValue(ChildrenCountProperty, value);
        }

        private static readonly DependencyProperty ChildrenCountProperty =
            DependencyProperty.Register(
                nameof(ChildrenCount),
                typeof(int),
                typeof(TreeListViewRow),
                new PropertyMetadata(0));

        public IEnumerable Children
        {
            get => (IEnumerable)GetValue(ChildrenProperty);
            private set => SetValue(ChildrenProperty, value);
        }

        private static readonly DependencyProperty ChildrenProperty =
            DependencyProperty.Register(
                nameof(Children),
                typeof(IEnumerable),
                typeof(TreeListViewRow),
                new PropertyMetadata(null));

        public IEnumerable AllChildren
        {
            get => (IEnumerable)GetValue(AllChildrenProperty);
            private set => SetValue(AllChildrenProperty, value);
        }

        private static readonly DependencyProperty AllChildrenProperty =
            DependencyProperty.Register(
                nameof(AllChildren),
                typeof(IEnumerable),
                typeof(TreeListViewRow),
                new PropertyMetadata(null));

        public int TreeLevel
        {
            get => (int)GetValue(TreeLevelProperty);
            private set => SetValue(TreeLevelProperty, value);
        }

        private static readonly DependencyProperty TreeLevelProperty =
            DependencyProperty.Register(
                nameof(TreeLevel),
                typeof(int),
                typeof(TreeListViewRow),
                new PropertyMetadata(0, OnTreeLevelChanged));

        public double Indent
        {
            get => (double)GetValue(IndentProperty);
            private set => SetValue(IndentPropertyKey, value);
        }

        private static readonly DependencyPropertyKey IndentPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(Indent),
                typeof(double),
                typeof(TreeListViewRow),
                new FrameworkPropertyMetadata(
                    0d,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty IndentProperty =
            IndentPropertyKey.DependencyProperty;

        #endregion

        #region Private / Protected Methods

        internal void UpdateIndent(double indentWidth)
        {
            _indentWidth = Math.Max(0d, indentWidth);
            Indent = _indentWidth * TreeLevel;
        }

        internal void SetNode(TreeListViewNode node)
        {
            Node = node;

            Expanded = node?.Expanded ?? false;

            if (ShouldRebind(ChildrenProperty))
            {
                BindingOperations.SetBinding(this, ChildrenProperty, new Binding(nameof(TreeListViewNode.ChildrenDatas))
                {
                    Source = node,
                    Mode = BindingMode.OneWay
                });
            }

            if (ShouldRebind(HasChildProperty))
            {
                BindingOperations.SetBinding(this, HasChildProperty, new Binding(nameof(TreeListViewNode.HasChild))
                {
                    Source = node,
                    Mode = BindingMode.OneWay
                });
            }

            if (ShouldRebind(ChildrenCountProperty))
            {
                BindingOperations.SetBinding(this, ChildrenCountProperty, new Binding(nameof(TreeListViewNode.ChildrenCount))
                {
                    Source = node,
                    Mode = BindingMode.OneWay
                });
            }

            if (ShouldRebind(TreeLevelProperty))
            {
                BindingOperations.SetBinding(this, TreeLevelProperty, new Binding(nameof(TreeListViewNode.Level))
                {
                    Source = node,
                    Mode = BindingMode.OneWay
                });
            }

            BindingOperations.SetBinding(this, AllChildrenProperty, new Binding(nameof(TreeListViewNode.AllChildrenDatas))
            {
                Source = node,
                Mode = BindingMode.OneWay
            });
        }

        private bool ShouldRebind(DependencyProperty property)
        {
            var existingBinding = BindingOperations.GetBinding(this, property);
            if (existingBinding != null)
            {
                if (existingBinding.Source is TreeListViewNode)
                {
                    BindingOperations.ClearBinding(this, property);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private static void OnExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TreeListViewRow row ||
                row.ExpanderVisibility != Visibility.Visible ||
                e.NewValue is not bool expanded ||
                e.OldValue is not bool oldExpanded ||
                expanded == oldExpanded)
                return;

            row.Node?.Expanded = expanded;
            if (row.DetailsTemplate != null)
                row?.DetailsVisibility = expanded ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnTreeLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeListViewRow row)
            {
                row.Indent = row._indentWidth * (int)e.NewValue;
            }
        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                base.OnPreviewMouseDoubleClick(e);
                return;
            }

            if (HasChild)
            {
                e.Handled = true;
                Expanded = !Expanded;
                return;
            }

            if (DetailsTemplate != null)
            {
                e.Handled = true;
                DetailsVisibility = DetailsVisibility == Visibility.Collapsed
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                return;
            }

            base.OnPreviewMouseDoubleClick(e);
        }

        #endregion
    }
}

