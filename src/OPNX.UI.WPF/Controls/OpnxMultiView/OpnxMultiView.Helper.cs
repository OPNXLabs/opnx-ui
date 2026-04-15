using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OPNX.UI.WPF.Controls.OpnxMultiView
{
    public static class OpnxMultiViewThumbnailHelper
    {
        private const double ThumbnailCoordinateSize = 100000;

        public static Canvas? GetThumbnail(IMultiViewLayout multiViewLayout,
                                           int width,
                                           int height,
                                           Brush fill,
                                           Brush stroke,
                                           int strokeThickness)
        {
            ArgumentNullException.ThrowIfNull(multiViewLayout);
            return GetThumbnail(multiViewLayout.CellLayouts, width, height, fill, stroke, strokeThickness);
        }

        public static Canvas? GetThumbnail(IEnumerable<IMultiViewCellLayout>? cellLayouts,
                                   int width,
                                   int height,
                                   Brush fill,
                                   Brush stroke,
                                   int strokeThickness)
        {
            if (cellLayouts == null)
                return null;

            var canvas = new Canvas { Width = width, Height = height, Margin = new Thickness(0) };

            foreach (var cellLayout in cellLayouts)
            {
                var rectForCanvas = cellLayout.RectForCanvas;

                var rect = new Rectangle
                {
                    Width = width * rectForCanvas.Width / ThumbnailCoordinateSize,
                    Height = height * rectForCanvas.Height / ThumbnailCoordinateSize,
                    Stroke = stroke,
                    StrokeThickness = strokeThickness,
                    Fill = fill
                };

                rect.SetValue(Canvas.LeftProperty, width * rectForCanvas.Left / ThumbnailCoordinateSize);
                rect.SetValue(Canvas.TopProperty, height * rectForCanvas.Top / ThumbnailCoordinateSize);

                canvas.Children.Add(rect);
            }

            var outerBorder = new Rectangle
            {
                Width = width,
                Height = height,
                Stroke = stroke,
                StrokeThickness = strokeThickness * 1.75,
                Fill = null
            };
            outerBorder.SetValue(Canvas.LeftProperty, 0.0);
            outerBorder.SetValue(Canvas.TopProperty, 0.0);
            canvas.Children.Add(outerBorder);

            return canvas;
        }
    }

}

