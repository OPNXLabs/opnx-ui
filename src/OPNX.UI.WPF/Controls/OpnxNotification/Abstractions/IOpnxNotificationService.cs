using OPNX.UI.WPF.Controls.OpnxNotification.Models;

namespace OPNX.UI.WPF.Controls.OpnxNotification.Abstractions;

public interface IOpnxNotificationService
{
    Task<OpnxNotificationResult> ShowAsync(
        object content,
        OpnxNotificationOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<OpnxNotificationResult> ShowAsync(
        Func<IOpnxNotification, object> contentFactory,
        OpnxNotificationOptions? options = null,
        CancellationToken cancellationToken = default);

    IOpnxNotification Create(object content, OpnxNotificationOptions? options = null);

    IOpnxNotification Create(Func<IOpnxNotification, object> contentFactory, OpnxNotificationOptions? options = null);
}
