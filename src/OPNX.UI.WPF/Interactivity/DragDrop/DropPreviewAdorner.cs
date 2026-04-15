using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace OPNX.UI.WPF.Interactivity.DragDrop
{
    public class DropPreviewAdorner : Adorner
    {
        private double _left;
        private readonly ContentPresenter _presenter;
        private double _top;

        public DropPreviewAdorner(UIElement? feedbackUI, UIElement? adornedElt) : base(adornedElt)
        {
            _presenter = new ContentPresenter()
            {
                Content = feedbackUI,
                IsHitTestVisible = false
            };
        }

        public double Left
        {
            get { return _left; }
            set
            {
                _left = value;
                UpdatePosition();
            }
        }

        public double Top
        {
            get { return _top; }
            set
            {
                _top = value;
                UpdatePosition();
            }
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        private void UpdatePosition()
        {
            if (this.Parent is AdornerLayer layer)
            {
                layer.Update(AdornedElement);
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _presenter.Measure(constraint);
            return _presenter.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _presenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _presenter;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new();
            result.Children.Add(new TranslateTransform(Left, Top));
            if (Left > 0) this.Visibility = Visibility.Visible;
            result.Children.Add(base.GetDesiredTransform(transform));

            return result;
        }
    }
}


