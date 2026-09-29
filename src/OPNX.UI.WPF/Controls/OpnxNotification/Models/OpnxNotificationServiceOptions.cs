namespace OPNX.UI.WPF.Controls.OpnxNotification.Models;

public sealed class OpnxNotificationServiceOptions
{
    public int MaxVisibleCount { get; set; } = 3;

    public TimeSpan DefaultDuration { get; set; } = TimeSpan.FromSeconds(5);

    public double Width { get; set; } = 360;

    public double ItemSpacing { get; set; } = 10;

    public double ScreenEdgeMargin { get; set; } = 16;

    public OpnxNotificationPosition Position { get; set; } = OpnxNotificationPosition.BottomRight;

    public OpnxNotificationScreen Screen { get; set; } = OpnxNotificationScreen.Primary;

    public bool PauseOnMouseOver { get; set; } = true;
}
