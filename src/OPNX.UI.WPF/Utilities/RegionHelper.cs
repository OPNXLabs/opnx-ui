using System.Windows;

namespace OPNX.UI.WPF.Utilities
{
    /// <summary>
    /// The border position.
    /// </summary>
    public enum BorderPosition
    {
        /// <summary>
        /// The unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The left.
        /// </summary>
        Left,

        /// <summary>
        /// The right.
        /// </summary>
        Right,

        /// <summary>
        /// The top.
        /// </summary>
        Top,

        /// <summary>
        /// The bottom.
        /// </summary>
        Bottom
    }

    /// <summary>
    /// The inno region util.
    /// </summary>
    public static class RegionHelper
    {
        #region Public Methods

        /// <summary>
        /// The get border position.
        /// </summary>
        /// <param name="point">
        /// The point.
        /// </param>
        /// <param name="rect">
        /// The rect.
        /// </param>
        /// <returns>
        /// return BorderPosition.
        /// </returns>
        public static BorderPosition GetBorderPosition(Point point, Rect rect)
        {
            BorderPosition result = BorderPosition.Unknown;
            if (point.X <= rect.Left)
            {
                result = BorderPosition.Left;
            }
            else if (point.X >= rect.Right - 1)
            {
                result = BorderPosition.Right;
            }

            if (point.Y <= rect.Top)
            {
                result = BorderPosition.Top;
            }
            else if (point.Y >= rect.Bottom - 1)
            {
                result = BorderPosition.Bottom;
            }

            return result;
        }


        public static List<Rect>? GetParentRegion(FrameworkElement control)
        {
            // 부모와 연결되어 있지 않으면 처리하지 않음.
            if (PresentationSource.FromVisual(control) == null)
            {
                return null;
            }

            // 자신을 포함한 모든 부모의 요소를 가져옴.
            var ancestorList = ControlHelper.GetAllParents(control, false);
            if (ancestorList.Any(parent => parent.Visibility != Visibility.Visible))
            {
                ancestorList = null;
            }

            // 자신을 포함한 모든 부모의 화면 영역을 리스트로 저장.
            // 아직 자식이 부모와 연결되어 있지 않으면 그 요소는 무시.
            var ancestorScreenRegionList = new List<Rect>();
            if (ancestorList != null)
            {
                foreach (var element in ancestorList)
                {
                    if (PresentationSource.FromVisual(element) != null)
                    {
                        var elementScreenRegion = new Rect(
                            element.PointToScreen(new Point(0, 0)),
                            element.PointToScreen(new Point(element.ActualWidth, element.ActualHeight)));
                        if (!ancestorScreenRegionList.Any(rect => rect.Equals(elementScreenRegion)))
                        {
                            ancestorScreenRegionList.Add(elementScreenRegion);
                        }
                    }
                }
            }

            return ancestorScreenRegionList;
        }

        /// <summary>
        /// The get point gap.
        /// </summary>
        /// <param name="point1">
        /// The point 1.
        /// </param>
        /// <param name="point2">
        /// The point 2.
        /// </param>
        /// <param name="position">
        /// The position.
        /// </param>
        /// <returns>
        /// return int.
        /// </returns>
        public static int GetPointGap(Point p1, Point p2, BorderPosition pos)
            => pos switch
            {
                BorderPosition.Left or BorderPosition.Right => (int)Math.Abs(p1.X - p2.X),
                BorderPosition.Top or BorderPosition.Bottom => (int)Math.Abs(p1.Y - p2.Y),
                _ => 0
            };

        /// <summary>
        /// The get round rect.
        /// </summary>
        /// <param name="rect">
        /// The rect.
        /// </param>
        /// <param name="digits">
        /// The digits.
        /// </param>
        /// <returns>
        /// return rect.
        /// </returns>
        public static Rect GetRoundRect(Rect rect, int digits)
        {
            if (!IsCorrectRectArea(rect))
            {
                return Rect.Empty;
            }

            var result = new Rect()
            {
                X = Math.Round(rect.Left, digits, MidpointRounding.AwayFromZero),
                Y = Math.Round(rect.Top, digits, MidpointRounding.AwayFromZero)
            };



            // result.Width = Math.Round(rect.Width, digits, MidpointRounding.AwayFromZero);
            // result.Height = Math.Round(rect.Height, digits, MidpointRounding.AwayFromZero);
            double right = Math.Round(rect.Right, digits, MidpointRounding.AwayFromZero);
            double bottom = Math.Round(rect.Bottom, digits, MidpointRounding.AwayFromZero);
            result.Width = Math.Round(right - result.X, 1, MidpointRounding.AwayFromZero);
            result.Height = Math.Round(bottom - result.Y, 1, MidpointRounding.AwayFromZero);

            return result;
        }

        /// <summary>
        /// The is correct rect area.
        /// </summary>
        /// <param name="rect">
        /// The rect.
        /// </param>
        /// <returns>
        /// return bool.
        /// </returns>
        public static bool IsCorrectRectArea(Rect rect)
        {
            if (rect.IsEmpty)
            {
                return false;
            }

            if (Double.IsNaN(rect.Left) || Double.IsInfinity(rect.Left))
            {
                return false;
            }

            if (Double.IsNaN(rect.Top) || Double.IsInfinity(rect.Top))
            {
                return false;
            }

            if (Double.IsNaN(rect.Width) || Double.IsInfinity(rect.Width))
            {
                return false;
            }

            if (Double.IsNaN(rect.Height) || Double.IsInfinity(rect.Height))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The is rect contains.
        /// </summary>
        /// <param name="baseRect">
        /// The base rect.
        /// </param>
        /// <param name="rect">
        /// The rect.
        /// </param>
        /// <param name="digits">
        /// The digits.
        /// </param>
        /// <returns>
        /// return bool.
        /// </returns>
        public static bool IsRectContains(Rect baseRect, Rect rect, int digits, bool areaContain = false)
        {
            Rect rect1 = GetRoundRect(baseRect, digits);
            Rect rect2 = GetRoundRect(rect, digits);

            if (areaContain == false)
            {
                return rect1.Contains(rect2);
            }

            else if (areaContain || rect1.Contains(rect2) == false)
            {
                return rect2.Contains(rect1);
            }

            return rect1.Contains(rect2);
        }

        /// <summary>
        /// The is rect equals.
        /// </summary>
        /// <param name="rect1">
        /// The rect 1.
        /// </param>
        /// <param name="rect2">
        /// The rect 2.
        /// </param>
        /// <param name="digits">
        /// The digits.
        /// </param>
        /// <returns>
        /// return bool.
        /// </returns>
        public static bool IsRectEquals(Rect rect1, Rect rect2, int digits)
        {
            Rect newRect1 = GetRoundRect(rect1, digits);
            Rect newRect2 = GetRoundRect(rect2, digits);

            return newRect1.Equals(newRect2);
        }

        /// <summary>
        /// The is rect intersect with.
        /// </summary>
        /// <param name="rect1">
        /// The rect 1.
        /// </param>
        /// <param name="rect2">
        /// The rect 2.
        /// </param>
        /// <param name="digits">
        /// The digits.
        /// </param>
        /// <returns>
        /// return bool.
        /// </returns>
        public static bool IsRectIntersectWith(Rect rect1, Rect rect2, int digits)
        {
            Rect newRect1 = GetRoundRect(rect1, digits);
            Rect newRect2 = GetRoundRect(rect2, digits);
            Rect intersectedRect = Rect.Intersect(newRect1, newRect2);
            return !intersectedRect.IsEmpty && intersectedRect.Width > 0 && intersectedRect.Height > 0;
        }

        /// <summary>
        /// point좌표를 rect안으로 제한한다.
        /// </summary>
        /// <param name="point">
        /// The point.
        /// </param>
        /// <param name="rect">
        /// The rect.
        /// </param>
        /// <returns>
        /// return point.
        /// </returns>
        public static Point LimitPointByRect(Point point, Rect rect)
        {
            if (point.X < rect.Left)
            {
                point.X = rect.Left;
            }

            if (point.X > rect.Right - 1)
            {
                point.X = rect.Right - 1;
            }

            if (point.Y < rect.Top)
            {
                point.Y = rect.Top;
            }

            if (point.Y > rect.Bottom - 1)
            {
                point.Y = rect.Bottom - 1;
            }

            return point;
        }

        /// <summary>
        /// The set point gap.
        /// </summary>
        /// <param name="point">
        /// The point.
        /// </param>
        /// <param name="gap">
        /// The gap.
        /// </param>
        /// <param name="position">
        /// The position.
        /// </param>
        /// <returns>
        /// rerurn point.
        /// </returns>
        public static Point SetPointGap(Point point, int gap, BorderPosition position)
        {
            switch (position)
            {
                case BorderPosition.Left:
                    point.Offset(-gap, 0);
                    break;

                case BorderPosition.Top:
                    point.Offset(0, -gap);
                    break;

                case BorderPosition.Right:
                    point.Offset(gap, 0);
                    break;

                case BorderPosition.Bottom:
                    point.Offset(0, gap);
                    break;
            }

            return point;
        }

        #endregion
    }
}


