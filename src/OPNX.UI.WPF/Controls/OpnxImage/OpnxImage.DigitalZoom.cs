using System.Windows;

namespace OPNX.UI.WPF.Controls
{
    public partial class OpnxImage
    {
        public static readonly DependencyProperty ViewRegionProperty =
            DependencyProperty.Register(nameof(ViewRegion), typeof(Rect), typeof(OpnxImage),
                new PropertyMetadata(new Rect(0, 0, 1, 1)), value =>
                {
                    var region = (Rect)value;
                    return !region.IsEmpty && double.IsFinite(region.X) && double.IsFinite(region.Y) &&
                        double.IsFinite(region.Width) && double.IsFinite(region.Height) &&
                        region.X >= 0 && region.Y >= 0 && region.Width > 0 && region.Height > 0 &&
                        region.Right <= 1 && region.Bottom <= 1;
                });

        /// <summary>Requested view in normalized original-video coordinates. Applied on the next display update.</summary>
        public Rect ViewRegion
        {
            get => (Rect)GetValue(ViewRegionProperty);
            set => SetValue(ViewRegionProperty, value);
        }

        // Capture and overlays use the region actually rendered, including pixel rounding.
        public Rect DisplayedViewRegion { get; private set; } = new(0, 0, 1, 1);

        public void ResetDigitalZoom() => ViewRegion = new Rect(0, 0, 1, 1);

        private Int32Rect GetPixelViewRegion(int width, int height)
        {
            var region = ViewRegion;
            int left = Math.Clamp((int)Math.Floor(region.Left * width), 0, width - 1);
            int top = Math.Clamp((int)Math.Floor(region.Top * height), 0, height - 1);
            int right = Math.Clamp((int)Math.Ceiling(region.Right * width), left + 1, width);
            int bottom = Math.Clamp((int)Math.Ceiling(region.Bottom * height), top + 1, height);
            return new Int32Rect(left, top, right - left, bottom - top);
        }

        private void SetDisplayedViewRegion(Int32Rect view, int width, int height)
        {
            DisplayedViewRegion = new Rect((double)view.X / width, (double)view.Y / height,
                (double)view.Width / width, (double)view.Height / height);
        }
    }
}
