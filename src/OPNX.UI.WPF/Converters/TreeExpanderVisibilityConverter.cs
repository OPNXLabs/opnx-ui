using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OPNX.UI.WPF.Converters
{
    public class TreeExpanderVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length > 1 && values[0] is bool d1 && values[1] is bool d2)
            {
                return (d1 && d2) ? Visibility.Visible : Visibility.Hidden;
            }
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
