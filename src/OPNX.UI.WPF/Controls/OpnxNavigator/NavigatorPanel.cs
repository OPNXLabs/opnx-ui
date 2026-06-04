using System.Windows;
using System.Windows.Controls;

namespace OPNX.UI.WPF.Controls
{
    public class NavigatorPanel : Panel
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(NavigatorPanel),
            new FrameworkPropertyMetadata(
                Orientation.Vertical,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var desiredSize = new Size();

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);

                if (Orientation == Orientation.Vertical)
                {
                    desiredSize.Width = Math.Max(desiredSize.Width, child.DesiredSize.Width);
                    desiredSize.Height += child.DesiredSize.Height;
                }
                else
                {
                    desiredSize.Width += child.DesiredSize.Width;
                    desiredSize.Height = Math.Max(desiredSize.Height, child.DesiredSize.Height);
                }
            }

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Orientation == Orientation.Vertical)
            {
                ArrangeVertically(finalSize);
            }
            else
            {
                ArrangeHorizontally(finalSize);
            }

            return finalSize;
        }

        private void ArrangeVertically(Size finalSize)
        {
            var start = 0.0;
            var end = finalSize.Height;

            foreach (UIElement child in InternalChildren)
            {
                if (OpnxNavigator.GetPlacement(child) != NavigatorItemPlacement.Start)
                {
                    continue;
                }

                child.Arrange(new Rect(0, start, finalSize.Width, child.DesiredSize.Height));
                start += child.DesiredSize.Height;
            }

            for (var index = InternalChildren.Count - 1; index >= 0; index--)
            {
                var child = InternalChildren[index];
                if (OpnxNavigator.GetPlacement(child) != NavigatorItemPlacement.End)
                {
                    continue;
                }

                end -= child.DesiredSize.Height;
                child.Arrange(new Rect(0, end, finalSize.Width, child.DesiredSize.Height));
            }
        }

        private void ArrangeHorizontally(Size finalSize)
        {
            var start = 0.0;
            var end = finalSize.Width;

            foreach (UIElement child in InternalChildren)
            {
                if (OpnxNavigator.GetPlacement(child) != NavigatorItemPlacement.Start)
                {
                    continue;
                }

                child.Arrange(new Rect(start, 0, child.DesiredSize.Width, finalSize.Height));
                start += child.DesiredSize.Width;
            }

            for (var index = InternalChildren.Count - 1; index >= 0; index--)
            {
                var child = InternalChildren[index];
                if (OpnxNavigator.GetPlacement(child) != NavigatorItemPlacement.End)
                {
                    continue;
                }

                end -= child.DesiredSize.Width;
                child.Arrange(new Rect(end, 0, child.DesiredSize.Width, finalSize.Height));
            }
        }
    }
}
