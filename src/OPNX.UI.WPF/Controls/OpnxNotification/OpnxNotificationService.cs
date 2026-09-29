using OPNX.UI.WPF.Controls.OpnxNotification.Abstractions;
using OPNX.UI.WPF.Controls.OpnxNotification.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace OPNX.UI.WPF.Controls.OpnxNotification;

public sealed class OpnxNotificationService : IOpnxNotificationService, IDisposable
{
    #region Fields
    private readonly OpnxNotificationServiceOptions _options;
    private readonly ObservableCollection<OpnxNotification> _notifications = [];
    private readonly Dispatcher _dispatcher;
    private OpnxNotificationWindow? _window;
    private bool _isDisposed;
    #endregion

    #region Constructors
    public OpnxNotificationService() : this(new OpnxNotificationServiceOptions())
    {
    }

    public OpnxNotificationService(OpnxNotificationServiceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.MaxVisibleCount < 1)
            throw new ArgumentOutOfRangeException(nameof(options), "MaxVisibleCount must be greater than zero.");
        if (options.Width <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Width must be greater than zero.");

        _options = options;
        _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
    }
    #endregion

    #region Public Methods
    public Task<OpnxNotificationResult> ShowAsync(object content, OpnxNotificationOptions? options = null, CancellationToken cancellationToken = default)
    {
        IOpnxNotification notification = Create(content, options);
        return notification.ShowAsync(cancellationToken);
    }

    public async Task<OpnxNotificationResult> ShowAsync(Func<IOpnxNotification, object> contentFactory, OpnxNotificationOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contentFactory);
        ThrowIfDisposed();

        cancellationToken.ThrowIfCancellationRequested();

        IOpnxNotification notification = await RunOnDispatcherAsync(
            () => CreateNotification(contentFactory, options),
            cancellationToken).ConfigureAwait(false);

        return await notification.ShowAsync(cancellationToken).ConfigureAwait(false);
    }

    public IOpnxNotification Create(object content, OpnxNotificationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(content);
        ThrowIfDisposed();
        ValidateContentDispatcher(content);

        return new OpnxNotification(this, options ?? new OpnxNotificationOptions(), _dispatcher)
        {
            Content = content
        };
    }

    public IOpnxNotification Create(Func<IOpnxNotification, object> contentFactory, OpnxNotificationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(contentFactory);
        ThrowIfDisposed();

        return RunOnDispatcher(() => CreateNotification(contentFactory, options));
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        RunOnDispatcher(() =>
        {
            foreach (OpnxNotification notification in _notifications.ToArray())
                Remove(notification, OpnxNotificationResult.Closed);

            _window?.Close();
            _window = null;
        });
    }
    #endregion

    #region Internal Methods
    internal void Show(OpnxNotification notification)
    {
        ThrowIfDisposed();
        RunOnDispatcher(() =>
        {
            if (_notifications.Contains(notification))
                return;

            while (_notifications.Count >= _options.MaxVisibleCount)
                Remove(_notifications[0], OpnxNotificationResult.Replaced);

            OpnxNotificationWindow window = GetWindow();
            _notifications.Add(notification);
            notification.Start(_options.DefaultDuration);

            if (!window.IsVisible)
                window.Show();

            window.UpdatePositionAfterLayout();
        });
    }

    internal void Close(OpnxNotification notification, OpnxNotificationResult result) =>
        RunOnDispatcher(() => Remove(notification, result));
    #endregion

    #region Private Methods
    private OpnxNotification CreateNotification(Func<IOpnxNotification, object> contentFactory, OpnxNotificationOptions? options)
    {
        OpnxNotification notification = new(this, options ?? new OpnxNotificationOptions(), _dispatcher);
        object content = contentFactory(notification) ??
            throw new InvalidOperationException("The notification content factory returned null.");

        ValidateContentDispatcher(content);
        notification.Content = content;
        return notification;
    }

    private OpnxNotificationWindow GetWindow()
    {
        _window ??= new OpnxNotificationWindow(_notifications, _options);
        return _window;
    }

    private void Remove(OpnxNotification notification, OpnxNotificationResult result)
    {
        if (!_notifications.Remove(notification))
        {
            notification.Complete(result);
            return;
        }

        notification.Complete(result);

        if (_notifications.Count == 0)
            _window?.Hide();
        else
            _window?.UpdatePositionAfterLayout();
    }

    private void ValidateContentDispatcher(object content)
    {
        if (content is DispatcherObject dispatcherObject && dispatcherObject.Dispatcher != _dispatcher)
        {
            throw new InvalidOperationException(
                "WPF notification content must be created on the application UI Dispatcher. " +
                "Use the contentFactory overload when showing a notification from a background thread.");
        }
    }

    private void RunOnDispatcher(Action action)
    {
        if (_dispatcher.CheckAccess())
            action();
        else
            _dispatcher.Invoke(action);
    }

    private T RunOnDispatcher<T>(Func<T> action)
    {
        if (_dispatcher.CheckAccess())
            return action();

        return _dispatcher.Invoke(action);
    }

    private async Task<T> RunOnDispatcherAsync<T>(Func<T> action, CancellationToken cancellationToken)
    {
        if (_dispatcher.CheckAccess())
            return action();

        DispatcherOperation<T> operation = _dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken);

        return await operation.Task.ConfigureAwait(false);
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_isDisposed, this);
    #endregion
}
