using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    [TemplatePart(Name = EditableTextBoxPartName, Type = typeof(TextBox))]
    public class OpnxComboBox : System.Windows.Controls.ComboBox
    {
        private const string EditableTextBoxPartName = "PART_EditableTextBox";
        private static readonly Regex ConditionRegex = new(@"\[(?<path>.*?)\]\s*(?<operator>==|!=|<=|>=|<|>)\s*(?<value>.*)", RegexOptions.Compiled);

        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(OpnxComboBox),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PlaceholderEnabledProperty = DependencyProperty.Register(
            nameof(PlaceholderEnabled),
            typeof(bool),
            typeof(OpnxComboBox),
            new PropertyMetadata(true));

        public static readonly DependencyProperty PlaceholderForegroundProperty = DependencyProperty.Register(
            nameof(PlaceholderForeground),
            typeof(Brush),
            typeof(OpnxComboBox),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(135, 255, 255, 255))));

        public static readonly DependencyProperty ButtonVisibilityProperty = DependencyProperty.Register(
            nameof(ButtonVisibility),
            typeof(Visibility),
            typeof(OpnxComboBox),
            new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty DropDownButtonContentProperty = DependencyProperty.Register(
            nameof(DropDownButtonContent),
            typeof(object),
            typeof(OpnxComboBox),
            new PropertyMetadata(null));

        public static readonly DependencyProperty DropDownButtonContentTemplateProperty = DependencyProperty.Register(
            nameof(DropDownButtonContentTemplate),
            typeof(DataTemplate),
            typeof(OpnxComboBox),
            new PropertyMetadata(null));

        public static readonly DependencyProperty DropDownButtonStyleProperty = DependencyProperty.Register(
            nameof(DropDownButtonStyle),
            typeof(Style),
            typeof(OpnxComboBox),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ClearSelectionOnDropDownClosedProperty = DependencyProperty.Register(
            nameof(ClearSelectionOnDropDownClosed),
            typeof(bool),
            typeof(OpnxComboBox),
            new PropertyMetadata(false));

        public static readonly DependencyProperty FilterStringProperty = DependencyProperty.Register(
            nameof(FilterString),
            typeof(string),
            typeof(OpnxComboBox),
            new PropertyMetadata(string.Empty, OnFilterStringChanged));

        public static readonly DependencyProperty CustomSortProperty = DependencyProperty.Register(
            nameof(CustomSort),
            typeof(IComparer),
            typeof(OpnxComboBox),
            new PropertyMetadata(null, OnCustomSortChanged));

        public static readonly DependencyProperty SelectedItemChangedProperty = DependencyProperty.Register(
            nameof(SelectedItemChanged),
            typeof(ICommand),
            typeof(OpnxComboBox),
            new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemCommandProperty = DependencyProperty.Register(
            nameof(SelectedItemCommand),
            typeof(ICommand),
            typeof(OpnxComboBox),
            new PropertyMetadata(null));

        private bool _isUpdatingItemsSource;
        private CollectionViewSource? _collectionViewSource;
        private TextBox? _editableTextBox;

        static OpnxComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxComboBox),
                new FrameworkPropertyMetadata(typeof(OpnxComboBox)));
        }

        public OpnxComboBox()
        {
            AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(OnEditableTextChanged));
        }

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public bool PlaceholderEnabled
        {
            get => (bool)GetValue(PlaceholderEnabledProperty);
            set => SetValue(PlaceholderEnabledProperty, value);
        }

        public Brush PlaceholderForeground
        {
            get => (Brush)GetValue(PlaceholderForegroundProperty);
            set => SetValue(PlaceholderForegroundProperty, value);
        }

        public Visibility ButtonVisibility
        {
            get => (Visibility)GetValue(ButtonVisibilityProperty);
            set => SetValue(ButtonVisibilityProperty, value);
        }

        public object? DropDownButtonContent
        {
            get => GetValue(DropDownButtonContentProperty);
            set => SetValue(DropDownButtonContentProperty, value);
        }

        public DataTemplate? DropDownButtonContentTemplate
        {
            get => (DataTemplate?)GetValue(DropDownButtonContentTemplateProperty);
            set => SetValue(DropDownButtonContentTemplateProperty, value);
        }

        public Style? DropDownButtonStyle
        {
            get => (Style?)GetValue(DropDownButtonStyleProperty);
            set => SetValue(DropDownButtonStyleProperty, value);
        }

        public bool ClearSelectionOnDropDownClosed
        {
            get => (bool)GetValue(ClearSelectionOnDropDownClosedProperty);
            set => SetValue(ClearSelectionOnDropDownClosedProperty, value);
        }

        public string FilterString
        {
            get => (string)GetValue(FilterStringProperty);
            set => SetValue(FilterStringProperty, value);
        }

        public IComparer? CustomSort
        {
            get => (IComparer?)GetValue(CustomSortProperty);
            set => SetValue(CustomSortProperty, value);
        }

        public ICommand? SelectedItemChanged
        {
            get => (ICommand?)GetValue(SelectedItemChangedProperty);
            set => SetValue(SelectedItemChangedProperty, value);
        }

        public ICommand? SelectedItemCommand
        {
            get => (ICommand?)GetValue(SelectedItemCommandProperty);
            set => SetValue(SelectedItemCommandProperty, value);
        }

        public TextBox? EditableTextBox => _editableTextBox ??= GetTemplateChild(EditableTextBoxPartName) as TextBox;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _editableTextBox = GetTemplateChild(EditableTextBoxPartName) as TextBox;
            UpdatePlaceholderState();
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            if (_isUpdatingItemsSource)
            {
                return;
            }

            if (newValue is null)
            {
                _collectionViewSource = null;
                UpdatePlaceholderState();
                return;
            }

            if (ReferenceEquals(newValue, _collectionViewSource?.View))
            {
                return;
            }

            _collectionViewSource = new CollectionViewSource { Source = newValue };
            _collectionViewSource.Filter += CollectionViewSource_Filter;
            ApplyCustomSort();
            ClearCurrentItem();

            try
            {
                _isUpdatingItemsSource = true;
                SetCurrentValue(ItemsSourceProperty, _collectionViewSource.View);
            }
            finally
            {
                _isUpdatingItemsSource = false;
            }

            UpdatePlaceholderState();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            UpdatePlaceholderState();

            object? selectedItem = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            ICommand? selectedItemCommand = SelectedItemCommand;
            if (selectedItem is not null && selectedItemCommand?.CanExecute(selectedItem) == true)
            {
                selectedItemCommand.Execute(selectedItem);
            }

            ICommand? command = SelectedItemChanged;
            if (command?.CanExecute(e) == true)
            {
                command.Execute(e);
            }

            base.OnSelectionChanged(e);
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);

            if (ClearSelectionOnDropDownClosed && SelectedIndex >= 0)
            {
                SetCurrentValue(SelectedIndexProperty, -1);
                SetCurrentValue(SelectedItemProperty, null);
                SetCurrentValue(SelectedValueProperty, null);
                SetCurrentValue(TextProperty, string.Empty);
                ClearCurrentItem();
            }

            UpdatePlaceholderState();
        }

        private static void OnFilterStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxComboBox comboBox)
            {
                comboBox.RefreshFilter();
            }
        }

        private static void OnCustomSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxComboBox comboBox)
            {
                comboBox.ApplyCustomSort();
            }
        }

        private void OnEditableTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsEditable)
            {
                UpdatePlaceholderState();
                return;
            }

            if (IsKeyboardFocusWithin && Text.Length > 0)
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
            }

            RefreshFilter();
            UpdatePlaceholderState();
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = MatchesFilter(e.Item);
        }

        private void RefreshFilter()
        {
            _collectionViewSource?.View?.Refresh();
        }

        private void ApplyCustomSort()
        {
            if (_collectionViewSource?.View is ListCollectionView listCollectionView)
            {
                listCollectionView.CustomSort = CustomSort;
            }
        }

        private void ClearCurrentItem()
        {
            _collectionViewSource?.View?.MoveCurrentToPosition(-1);
        }

        private void UpdatePlaceholderState()
        {
            bool shouldShowPlaceholder = SelectedItem is null && SelectedValue is null && string.IsNullOrEmpty(Text);
            if (PlaceholderEnabled != shouldShowPlaceholder)
            {
                SetCurrentValue(PlaceholderEnabledProperty, shouldShowPlaceholder);
            }
        }

        private bool MatchesFilter(object? item)
        {
            string filterString = FilterString;
            if (string.IsNullOrWhiteSpace(filterString))
            {
                return true;
            }

            if (item is null)
            {
                return false;
            }

            string[] orGroups = Regex.Split(filterString, @"\s+OR\s+", RegexOptions.IgnoreCase);
            foreach (string orGroup in orGroups)
            {
                string[] andConditions = Regex.Split(orGroup, @"\s+AND\s+", RegexOptions.IgnoreCase);
                bool allMatched = true;

                foreach (string condition in andConditions)
                {
                    if (!EvaluateCondition(condition.Trim(), item))
                    {
                        allMatched = false;
                        break;
                    }
                }

                if (allMatched)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool EvaluateCondition(string condition, object item)
        {
            Match match = ConditionRegex.Match(condition);
            if (!match.Success)
            {
                return false;
            }

            string propertyPath = match.Groups["path"].Value.Trim();
            string operatorText = match.Groups["operator"].Value;
            string filterValueText = TrimFilterValue(match.Groups["value"].Value.Trim());

            if (!TryGetPropertyValue(item, propertyPath, out object? propertyValue, out Type? propertyType))
            {
                return false;
            }

            return EvaluateComparison(propertyValue, propertyType, filterValueText, operatorText);
        }

        private static bool TryGetPropertyValue(object item, string propertyPath, out object? value, out Type? valueType)
        {
            object? current = item;
            Type? currentType = current.GetType();

            foreach (string pathPart in propertyPath.Split('.'))
            {
                if (string.IsNullOrWhiteSpace(pathPart) || current is null || currentType is null)
                {
                    value = null;
                    valueType = null;
                    return false;
                }

                PropertyInfo? property = currentType.GetProperty(pathPart.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

                if (property is null && current is ComboBoxItem comboBoxItem && comboBoxItem.Content is not null)
                {
                    current = comboBoxItem.Content;
                    currentType = current.GetType();
                    property = currentType.GetProperty(pathPart.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                }

                if (property is null)
                {
                    value = null;
                    valueType = null;
                    return false;
                }

                current = property.GetValue(current);
                currentType = property.PropertyType;
            }

            value = current;
            valueType = currentType;
            return true;
        }

        private static bool EvaluateComparison(object? propertyValue, Type? propertyType, string filterValueText, string operatorText)
        {
            if (IsNullLiteral(filterValueText))
            {
                return operatorText switch
                {
                    "==" => propertyValue is null,
                    "!=" => propertyValue is not null,
                    _ => false
                };
            }

            if (propertyValue is null || propertyType is null)
            {
                return operatorText == "!=";
            }

            Type comparisonType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            object? filterValue = ConvertFilterValue(filterValueText, comparisonType);

            if (filterValue is null)
            {
                return CompareAsString(Convert.ToString(propertyValue, CultureInfo.CurrentCulture), filterValueText, operatorText);
            }

            if (operatorText is "==" or "!=")
            {
                bool equals = AreEqual(propertyValue, filterValue, comparisonType);
                return operatorText == "==" ? equals : !equals;
            }

            if (propertyValue is IComparable left && filterValue is IComparable right)
            {
                int result = left.CompareTo(right);
                return operatorText switch
                {
                    "<" => result < 0,
                    ">" => result > 0,
                    "<=" => result <= 0,
                    ">=" => result >= 0,
                    _ => false
                };
            }

            return CompareAsString(Convert.ToString(propertyValue, CultureInfo.CurrentCulture), Convert.ToString(filterValue, CultureInfo.CurrentCulture), operatorText);
        }

        private static object? ConvertFilterValue(string valueText, Type targetType)
        {
            try
            {
                if (targetType == typeof(string))
                {
                    return valueText;
                }

                if (targetType.IsEnum)
                {
                    return Enum.Parse(targetType, valueText, ignoreCase: true);
                }

                if (targetType == typeof(Guid))
                {
                    return Guid.Parse(valueText);
                }

                if (targetType == typeof(bool))
                {
                    return bool.Parse(valueText);
                }

                return Convert.ChangeType(valueText, targetType, CultureInfo.InvariantCulture);
            }
            catch
            {
                try
                {
                    return Convert.ChangeType(valueText, targetType, CultureInfo.CurrentCulture);
                }
                catch
                {
                    return null;
                }
            }
        }

        private static bool AreEqual(object propertyValue, object filterValue, Type comparisonType)
        {
            if (comparisonType == typeof(string))
            {
                return string.Equals(
                    Convert.ToString(propertyValue, CultureInfo.CurrentCulture),
                    Convert.ToString(filterValue, CultureInfo.CurrentCulture),
                    StringComparison.CurrentCulture);
            }

            return Equals(propertyValue, filterValue);
        }

        private static bool CompareAsString(string? left, string? right, string operatorText)
        {
            int result = string.Compare(left ?? string.Empty, right ?? string.Empty, StringComparison.CurrentCulture);
            return operatorText switch
            {
                "<" => result < 0,
                ">" => result > 0,
                "==" => result == 0,
                "!=" => result != 0,
                "<=" => result <= 0,
                ">=" => result >= 0,
                _ => false
            };
        }

        private static string TrimFilterValue(string value)
        {
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
            {
                return value[1..^1];
            }

            return value;
        }

        private static bool IsNullLiteral(string value)
        {
            return string.Equals(value, "null", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "{x:Null}", StringComparison.OrdinalIgnoreCase);
        }
    }
}
