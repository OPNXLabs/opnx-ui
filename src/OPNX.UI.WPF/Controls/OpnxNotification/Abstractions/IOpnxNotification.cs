using OPNX.UI.WPF.Controls.OpnxNotification.Models;

namespace OPNX.UI.WPF.Controls.OpnxNotification.Abstractions;

public interface IOpnxNotification
{
    Guid ID { get; }

    object Content { get; }

    bool IsVisible { get; }

    Task<OpnxNotificationResult> ShowAsync(CancellationToken cancellationToken = default);

    void Close(OpnxNotificationResult result = OpnxNotificationResult.Closed);
}
