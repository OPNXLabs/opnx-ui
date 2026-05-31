using OPNX.UI.WPF.Infrastructure;
using OPNX.UI.WPF.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public class SelectionFrame : UserControl, IInternalVisualElement
    {
        #region Constants and Fields

        private const double DefaultThickness = 6;
        private readonly Pen _verticalPen = new();
        private readonly Pen _horizontalPen = new();

        private Brush? _stroke;

        private PenLineJoin _strokeLineJoin;

        private double _verticalThickness;
        private double _horizontalThickness;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionFrame"/> class.
        /// </summary>
        public SelectionFrame()
        {
            this.Stroke = new SolidColorBrush(Colors.PaleVioletRed);
            this._verticalThickness = DefaultThickness;
            this._horizontalThickness = DefaultThickness;
            this.SnapsToDevicePixels = true;
            this.StrokeLineJoin = PenLineJoin.Bevel;
            this.Opacity = 0.5;
            this.Name = "SelectionFrame";

            //if (Public.IsEditingType())
            //    this.Visibility = Visibility.Visible;
            //else
            //    this.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Stroke.
        /// </summary>
        public Brush? Stroke
        {
            get
            {
                return this._stroke;
            }

            set
            {
                this._stroke = value;
                this.SetPenAttribute();
            }
        }

        /// <summary>
        /// Gets or sets StrokeLineJoin.
        /// </summary>
        public PenLineJoin StrokeLineJoin
        {
            get
            {
                return this._strokeLineJoin;
            }

            set
            {
                this._strokeLineJoin = value;
                this.SetPenAttribute();
            }
        }

        /// <summary>
        /// Gets or sets StrokeThickness.
        /// </summary>
        public double VerticalThickness
        {
            get
            {
                return this._verticalThickness;
            }

            set
            {
                this._verticalThickness = value;
                this.SetPenAttribute();
            }
        }

        /// <summary>
        /// Gets or sets StrokeThickness.
        /// </summary>
        public double HorizontalThickness
        {
            get
            {
                return this._horizontalThickness;
            }

            set
            {
                this._horizontalThickness = value;
                this.SetPenAttribute();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The Redraw.
        /// </summary>
        /// <param name="element">
        /// The element.
        /// </param>
        public void Redraw(FrameworkElement element)
        {
            this.Width = element.Width;
            this.Height = element.Height;

            Canvas.SetLeft(this, 0);
            Canvas.SetTop(this, 0);

            double ratioH = UIHelper.GetRatioHighWithScreen(this);
            double ratioW = UIHelper.GetRatioWidthWithScreen(this);

            if (!double.IsNaN(ratioH))
            {
                this.HorizontalThickness = DefaultThickness * ratioH;
            }

            if (!double.IsNaN(ratioW))
            {
                this.VerticalThickness = DefaultThickness * ratioW;
            }
        }

        /// <summary>
        /// The on render.
        /// </summary>
        /// <param name="drawingContext">
        /// The drawing context.
        /// </param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            var rect = new Rect
            {
                X = 0,
                Y = 0,
                Width = this.Width,
                Height = this.Height
            };

            drawingContext.DrawLine(this._verticalPen, rect.TopLeft, rect.TopRight);
            drawingContext.DrawLine(this._horizontalPen, rect.TopRight, rect.BottomRight);
            drawingContext.DrawLine(this._verticalPen, rect.BottomRight, rect.BottomLeft);
            drawingContext.DrawLine(this._horizontalPen, rect.BottomLeft, rect.TopLeft);

            base.OnRender(drawingContext);
        }

        private void SetPenAttribute()
        {
            this._verticalPen.Brush = this._stroke;
            this._horizontalPen.Brush = this._stroke;

            this._verticalPen.Thickness = this._verticalThickness;
            this._horizontalPen.Thickness = this._horizontalThickness;

            this._verticalPen.LineJoin = this._strokeLineJoin;
            this._horizontalPen.LineJoin = this._strokeLineJoin;
        }

        #endregion
    }
}









