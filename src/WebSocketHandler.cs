using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using Teams.ThirdPartyAppApi.Adapters;

[assembly: InternalsVisibleTo("Teams.ThirdPartyAppApi.Tests")]
namespace Teams.ThirdPartyAppApi;
internal class WebSocketHandler : IDisposable
{
    private readonly IClientWebSocket _webSocket;
    private readonly BehaviorSubject<WebSocketState> _whenStateChanged;
    private readonly Subject<WebSocketState> _whenStateChecked = new();
    private readonly Subject<string> _messageSubject = new();
    internal readonly Uri _uri;
    private readonly bool _autoReconnect;
    private CancellationTokenSource? _receiveCancellationTokenSource;
    private CancellationTokenSource? _connectionCancellationTokenSource;
    private readonly SemaphoreSlim _connectLock = new(1, 1);

    private readonly SemaphoreSlim _closeLock = new(1, 1);

    private bool _manuallyDisconnected = false;
    private readonly Timer _reconnectTimer;
    private bool _disposed = false;

    public IObservable<WebSocketState> ConnectionStatus => _whenStateChanged.AsObservable();
    public IObservable<string> ReceivedMessages => _messageSubject.AsObservable();



    public WebSocketHandler(Uri uri, bool autoReconnect) : this(uri, autoReconnect, new ClientWebSocketAdapter(), new BehaviorSubject<WebSocketState>(WebSocketState.None))
    {
    }

    internal WebSocketHandler(Uri uri, bool autoReconnect, IClientWebSocket clientWebSocket) : this(uri, autoReconnect, clientWebSocket, new BehaviorSubject<WebSocketState>(WebSocketState.None))
    {
    }

    internal WebSocketHandler(Uri uri, bool autoReconnect, IClientWebSocket clientWebSocket, BehaviorSubject<WebSocketState> stateSubject)
    {
        _whenStateChanged = stateSubject;
        _webSocket = clientWebSocket;
        _uri = uri;
        _autoReconnect = autoReconnect;
        _receiveCancellationTokenSource = null;

        _whenStateChanged
            .CombineLatest(_whenStateChecked, (state, _) => state)
            .Where(state =>
                _webSocket.State is WebSocketState.Aborted or WebSocketState.Closed
                && autoReconnect
                && !_manuallyDisconnected)
            .Subscribe(state => _ = ReconnectAsyncSafe());

        _reconnectTimer = new Timer(ReconnectTimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }

    private void ReconnectTimerCallback(object? state)
    {
        if (_disposed) return;
        _ = AutoReconnectCallbackSafe();
    }

    private async Task AutoReconnectCallbackSafe()
    {
        try { await AutoReconnectCallback(); }
        catch (Exception ex) { Console.Error.WriteLine($"AutoReconnectCallback error: {ex}"); }
    }

    private async Task ReconnectAsyncSafe()
    {
        try { await ReconnectAsync(); }
        catch (Exception ex) { Console.Error.WriteLine($"ReconnectAsync error: {ex}"); }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _reconnectTimer?.Dispose();
        _receiveCancellationTokenSource?.Cancel();
        _connectionCancellationTokenSource?.Cancel();
        _connectLock?.Dispose();
        _closeLock?.Dispose();
        _whenStateChanged?.Dispose();
        _whenStateChecked?.Dispose();
        _messageSubject?.Dispose();
        _webSocket?.Dispose();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await _connectLock.WaitAsync(cancellationToken);
        try
        {
            await WaitForConnection(cancellationToken);
            if (_webSocket.State is WebSocketState.Closed or WebSocketState.Aborted)
            {
                _webSocket.CreateNewSocket();
            }

            if (_webSocket.State is WebSocketState.None)
            {
                _manuallyDisconnected = false;

                _connectionCancellationTokenSource = new CancellationTokenSource(5000);
                var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _connectionCancellationTokenSource.Token).Token;
                await _webSocket.ConnectAsync(_uri, combinedToken);
                StartReceivingMessagesAsync(combinedToken);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ConnectAsync error: {ex}");
        }
        finally
        {
            _whenStateChanged.OnNextIfValueChanged(_webSocket.State);
            _connectLock.Release();
        }

    }

    private void StartReceivingMessagesAsync(CancellationToken cancellationToken)
    {
        _receiveCancellationTokenSource = new CancellationTokenSource();
        var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _receiveCancellationTokenSource.Token).Token;
        Task.Factory.StartNew(() => ReceiveMessagesAsync(combinedToken), combinedToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private void StopReceivingMessages()
    {
        _receiveCancellationTokenSource?.Cancel();
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        await WaitForConnection(cancellationToken);
        var buffer = new byte[1024 * 4];
        while (!cancellationToken.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult? result;
            try
            {
                result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ReceiveMessagesAsync error: {ex}");
                break;
            }
            if (result is null)
                continue;

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _messageSubject.OnNext(message);
        }
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        await WaitForConnection(cancellationToken);

        if (_webSocket.State != WebSocketState.Open)
            return;

        var buffer = Encoding.UTF8.GetBytes(message);
        try
        {
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"SendMessageAsync error: {ex}");
        }
    }

    public async Task ReconnectAsync()
    {
        // Token måste skickas in, använd en ny om ingen finns
        var token = _connectionCancellationTokenSource?.Token ?? CancellationToken.None;
        await WaitForConnection(token);

        await Close(token);

        if (_webSocket.State is WebSocketState.Open or WebSocketState.Connecting)
            return;

        if (!token.IsCancellationRequested)
        {
            await ConnectAsync(token);
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        _manuallyDisconnected = true;

        try { await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken); }
        catch (Exception ex) { Console.Error.WriteLine($"DisconnectAsync error: {ex}"); }

        StopReceivingMessages();
        _whenStateChanged.OnNextIfValueChanged(_webSocket.State);
        _reconnectTimer?.Dispose();
    }

    private async Task WaitForConnection(CancellationToken cancellationToken)
    {
        if (_webSocket.State is not WebSocketState.Connecting)
            return;

        while (!cancellationToken.IsCancellationRequested && _webSocket.State is WebSocketState.Connecting)
        {
            await Task.Delay(15, cancellationToken);
        }
    }

    private async Task Close(CancellationToken cancellationToken)
    {
        await _closeLock.WaitAsync(cancellationToken);
        try
        {
            if (_webSocket.State is WebSocketState.CloseReceived)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Got CloseReceived", cancellationToken);
                _whenStateChanged.OnNextIfValueChanged(_webSocket.State);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Close error: {ex}");
        }
        finally
        {
            _closeLock.Release();
        }
    }

    private async Task AutoReconnectCallback()
    {
        if (_webSocket.State is WebSocketState.Connecting or WebSocketState.Open or WebSocketState.None)
        {
            return;
        }
        
        if (_webSocket.State is WebSocketState.CloseReceived)
        {
            var token = _connectionCancellationTokenSource?.Token ?? CancellationToken.None;
            await Close(token);
        }
        else
        {
            _whenStateChecked.OnNext(_webSocket.State);
        }
    }
}