using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OPNX.UI.WPF.Converters
{
    public class TitleHeaderHeightConverter : IMultiValueConverter
    {
        #region IMultiValueConverter

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="values">
        /// The values.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The GridLength.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //if (values == null || values.Length < 2 || CommonsConfig.Instance.ZoomedCellTitleHeight <= 0)
            if (values == null || values.Length < 2)
            {
                return new GridLength(0.0);
            }

            if (values[0] is bool && values[1] is Visual)
            {
                var isZoomed = (bool)values[0];
                if (isZoomed)
                {
                    var visual = (Visual)values[1];

                    Point pointTopLeft = visual.PointFromScreen(new Point(0, 0));
                    //Point pointBottomRight = visual.PointFromScreen(new Point(0, CommonsConfig.Instance.ZoomedCellTitleHeight));
                    Point pointBottomRight = visual.PointFromScreen(new Point(0, 0));
                    return new GridLength(pointBottomRight.Y - pointTopLeft.Y);
                }
            }

            return new GridLength(0.0);
        }

        /// <summary>
        /// The convert back.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetTypes">
        /// The target types.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// Not Used.
        /// </returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

