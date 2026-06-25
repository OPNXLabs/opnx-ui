using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public class OpnxArcButton : Button
    {
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(OpnxArcButton),
                new PropertyMetadata(0.0, OnGeometryChanged));

        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register(nameof(EndAngle), typeof(double), typeof(OpnxArcButton),
                new PropertyMetadata(90.0, OnGeometryChanged));

        public static readonly DependencyProperty OuterRadiusProperty =
            DependencyProperty.Register(nameof(OuterRadius), typeof(double), typeof(OpnxArcButton),
                new PropertyMetadata(80.0, OnGeometryChanged));

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register(nameof(InnerRadius), typeof(double), typeof(OpnxArcButton),
                new PropertyMetadata(40.0, OnGeometryChanged));

        public static readonly DependencyProperty GeometryProperty =
            DependencyProperty.Register(nameof(Geometry), typeof(Geometry), typeof(OpnxArcButton));

        static OpnxArcButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxArcButton),
                new FrameworkPropertyMetadata(typeof(OpnxArcButton)));
        }

        public double StartAngle
        {
            get => (double)GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        public double EndAngle
        {
            get => (double)GetValue(EndAngleProperty);
            set => SetValue(EndAngleProperty, value);
        }

        public double OuterRadius
        {
            get => (double)GetValue(OuterRadiusProperty);
            set => SetValue(OuterRadiusProperty, value);
        }

        public double InnerRadius
        {
            get => (double)GetValue(InnerRadiusProperty);
            set => SetValue(InnerRadiusProperty, value);
        }

        public Geometry Geometry
        {
            get => (Geometry)GetValue(GeometryProperty);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateGeometry();

            if (GetTemplateChild("PART_ContentTransform") is TranslateTransform transform)
            {
                double angleDeg = (StartAngle + EndAngle) / 2.0;
                double angleRad = angleDeg * Math.PI / 180.0;
                double midRadius = (OuterRadius + InnerRadius) / 2.0;

                transform.X = Math.Cos(angleRad) * midRadius;
                transform.Y = Math.Sin(angleRad) * midRadius;
            }
        }

        private static void OnGeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxArcButton button)
                button.UpdateGeometry();
        }

        private void UpdateGeometry()
        {
            SetValue(GeometryProperty, CreateArcSegmentGeometry());

            Width = OuterRadius * 2;
            Height = OuterRadius * 2;
        }

        private PathGeometry CreateArcSegmentGeometry()
        {
            double centerX = OuterRadius;
            double centerY = OuterRadius;

            double startAngleRad = StartAngle * Math.PI / 180;
            double endAngleRad = EndAngle * Math.PI / 180;

            double outerStartX = centerX + OuterRadius * Math.Cos(startAngleRad);
            double outerStartY = centerY + OuterRadius * Math.Sin(startAngleRad);
            double outerEndX = centerX + OuterRadius * Math.Cos(endAngleRad);
            double outerEndY = centerY + OuterRadius * Math.Sin(endAngleRad);

            double innerStartX = centerX + InnerRadius * Math.Cos(startAngleRad);
            double innerStartY = centerY + InnerRadius * Math.Sin(startAngleRad);
            double innerEndX = centerX + InnerRadius * Math.Cos(endAngleRad);
            double innerEndY = centerY + InnerRadius * Math.Sin(endAngleRad);

            bool isLargeArc = Math.Abs(EndAngle - StartAngle) > 180;

            var pathFigure = new PathFigure
            {
                StartPoint = new Point(outerStartX, outerStartY),
                IsClosed = true
            };

            pathFigure.Segments.Add(new ArcSegment(
                new Point(outerEndX, outerEndY),
                new Size(OuterRadius, OuterRadius),
                0,
                isLargeArc,
                SweepDirection.Clockwise,
                true));

            pathFigure.Segments.Add(new LineSegment(new Point(innerEndX, innerEndY), true));

            pathFigure.Segments.Add(new ArcSegment(
                new Point(innerStartX, innerStartY),
                new Size(InnerRadius, InnerRadius),
                0,
                isLargeArc,
                SweepDirection.Counterclockwise,
                true));

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);
            return pathGeometry;
        }
    }
}
