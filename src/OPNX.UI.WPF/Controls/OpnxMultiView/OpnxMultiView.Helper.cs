using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OPNX.UI.WPF.Controls
{
    public sealed class OpnxMultiViewThumbnailOptions
    {
        public double Width { get; init; } = 50;

        public double Height { get; init; } = 50;

        public Color FillColor { get; init; } = Colors.Transparent;

        public Color StrokeColor { get; init; } = Color.FromRgb(0xb8, 0xb8, 0xb8);

        public double StrokeThickness { get; init; } = 1;

        public Brush CreateFillBrush() => new SolidColorBrush(FillColor);

        public Brush CreateStrokeBrush() => new SolidColorBrush(StrokeColor);
    }

    public static class OpnxMultiViewThumbnailHelper
    {
        private const double ThumbnailCoordinateSize = 100000;

        public static Canvas? GetThumbnail(IMultiViewLayout multiViewLayout,
                                           OpnxMultiViewThumbnailOptions options)
        {
            ArgumentNullException.ThrowIfNull(multiViewLayout);
            ArgumentNullException.ThrowIfNull(options);

            return GetThumbnail(multiViewLayout.CellLayouts, options);
        }

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
                                           OpnxMultiViewThumbnailOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return GetThumbnail(
                cellLayouts,
                options.Width,
                options.Height,
                options.CreateFillBrush(),
                options.CreateStrokeBrush(),
                options.StrokeThickness);
        }

        public static Canvas? GetThumbnail(IEnumerable<IMultiViewCellLayout>? cellLayouts,
                                   double width,
                                   double height,
                                   Brush fill,
                                   Brush stroke,
                                   double strokeThickness)
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

        public static Canvas CreateAddThumbnail(OpnxMultiViewThumbnailOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            var stroke = options.CreateStrokeBrush();
            var canvas = new Canvas
            {
                Width = options.Width,
                Height = options.Height,
                Background = Brushes.Transparent
            };

            canvas.Children.Add(new Rectangle
            {
                Width = options.Width,
                Height = options.Height,
                Stroke = stroke,
                StrokeThickness = options.StrokeThickness,
                Fill = null,
                StrokeDashArray = [6, 3]
            });

            double crossSize = Math.Min(options.Width, options.Height) * 0.4;
            double thickness = options.StrokeThickness * 3;
            double centerX = options.Width / 2.0;
            double centerY = options.Height / 2.0;

            canvas.Children.Add(new Rectangle
            {
                Width = thickness,
                Height = crossSize,
                Fill = stroke,
                Margin = new Thickness(centerX - thickness / 2, centerY - crossSize / 2, 0, 0)
            });

            canvas.Children.Add(new Rectangle
            {
                Width = crossSize,
                Height = thickness,
                Fill = stroke,
                Margin = new Thickness(centerX - crossSize / 2, centerY - thickness / 2, 0, 0)
            });

            return canvas;
        }
    }

}


