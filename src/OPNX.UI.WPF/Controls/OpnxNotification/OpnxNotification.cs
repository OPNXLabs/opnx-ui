using OPNX.UI.WPF.Controls.OpnxNotification.Abstractions;
using OPNX.UI.WPF.Controls.OpnxNotification.Models;
using System.Windows.Threading;

namespace OPNX.UI.WPF.Controls.OpnxNotification;

internal sealed class OpnxNotification : IOpnxNotification
{
    #region Fields
    private readonly OpnxNotificationService _service;
    private readonly OpnxNotificationOptions _options;
    private readonly TaskCompletionSource<OpnxNotificationResult> _completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly DispatcherTimer _timer;
    private CancellationTokenRegistration _cancellationRegistration;
    private TimeSpan _remaining;
    private DateTime _timerStartedAt;
    private bool _isShown;
    #endregion

    #region Constructors
    internal OpnxNotification(OpnxNotificationService service, OpnxNotificationOptions options, Dispatcher dispatcher)
    {
        _service = service;
        _options = options;
        _timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher);
        _timer.Tick += OnTimerTick;
    }
    #endregion

    #region Properties
    public Guid ID { get; } = Guid.NewGuid();

    public object Content { get; internal set; } = null!;

    public bool IsVisible { get; private set; }
    #endregion

    #region Public Methods
    public Task<OpnxNotificationResult> ShowAsync(CancellationToken cancellationToken = default)
    {
        if (!_isShown)
        {
            _isShown = true;
            _service.Show(this);

            if (cancellationToken.CanBeCanceled)
            {
                CancellationTokenRegistration registration = cancellationToken.Register(() => Close());
                _cancellationRegistration = registration;

                if (_completionSource.Task.IsCompleted)
                    _cancellationRegistration.Dispose();
            }
        }

        return _completionSource.Task;
    }

    public void Close(OpnxNotificationResult result = OpnxNotificationResult.Closed) => _service.Close(this, result);
    #endregion

    #region Internal Methods
    internal void Start(TimeSpan duration)
    {
        IsVisible = true;
        _remaining = _options.Duration ?? duration;

        if (_remaining > TimeSpan.Zero)
            StartTimer();
    }

    internal void Pause()
    {
        if (!_timer.IsEnabled)
            return;

        _remaining -= DateTime.UtcNow - _timerStartedAt;
        _timer.Stop();
    }

    internal void Resume()
    {
        if (IsVisible && !_timer.IsEnabled && _remaining > TimeSpan.Zero)
            StartTimer();
    }

    internal void Complete(OpnxNotificationResult result)
    {
        if (!IsVisible && _completionSource.Task.IsCompleted)
            return;

        IsVisible = false;
        _timer.Stop();
        _cancellationRegistration.Dispose();
        _completionSource.TrySetResult(result);
    }
    #endregion

    #region Private Methods
    private void StartTimer()
    {
        _timerStartedAt = DateTime.UtcNow;
        _timer.Interval = _remaining;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _timer.Stop();
        Close(OpnxNotificationResult.TimedOut);
    }
    #endregion
}
