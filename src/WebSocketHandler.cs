using System.Buffers;
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
    private Timer? _reconnectTimer;
    private bool _disposed = false;
    private CancellationTokenSource? _linkedConnectTokenSource;
    private CancellationTokenSource? _linkedReceiveTokenSource;
    private int _receiveLoopRunning = 0;
    private const int DefaultBufferSize = 1024 * 4;

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
        if (_disposed || _manuallyDisconnected) return;
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
        _reconnectTimer = null;
        
        _receiveCancellationTokenSource?.Cancel();
        _receiveCancellationTokenSource?.Dispose();
        _connectionCancellationTokenSource?.Cancel();
        _connectionCancellationTokenSource?.Dispose();
        _linkedConnectTokenSource?.Dispose();
        _linkedReceiveTokenSource?.Dispose();
        
        _connectLock?.Dispose();
        _closeLock?.Dispose();
        _whenStateChanged?.Dispose();
        _whenStateChecked?.Dispose();
        _messageSubject?.Dispose();
        _webSocket?.Dispose();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (_disposed) return;
        
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
                
                if (_autoReconnect)
                {
                    if (_reconnectTimer == null)
                    {
                        _reconnectTimer = new System.Threading.Timer(state => AutoReconnectCallbackSafe(), null, Timeout.Infinite, Timeout.Infinite);
                    }
                    _reconnectTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));
                }

                _connectionCancellationTokenSource?.Dispose();
                _connectionCancellationTokenSource = new CancellationTokenSource();
                
                _linkedConnectTokenSource?.Dispose();
                _linkedConnectTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _connectionCancellationTokenSource.Token);
                
                await _webSocket.ConnectAsync(_uri, _linkedConnectTokenSource.Token);
                StartReceivingMessagesAsync(_linkedConnectTokenSource.Token);
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
        if (Interlocked.CompareExchange(ref _receiveLoopRunning, 1, 0) == 1)
        {
            return; // Already running
        }

        _receiveCancellationTokenSource?.Dispose();
        _receiveCancellationTokenSource = new CancellationTokenSource();
        
        _linkedReceiveTokenSource?.Dispose();
        _linkedReceiveTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _receiveCancellationTokenSource.Token);
        
        Task.Factory.StartNew(() => ReceiveMessagesAsync(_linkedReceiveTokenSource.Token), _linkedReceiveTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private void StopReceivingMessages()
    {
        _receiveCancellationTokenSource?.Cancel();
        Interlocked.Exchange(ref _receiveLoopRunning, 0);
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        await WaitForConnection(cancellationToken);
        var buffer = ArrayPool<byte>.Shared.Rent(DefaultBufferSize);
        try
        {
            while (!cancellationToken.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult? result;
                try
                {
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("ReceiveMessagesAsync cancelled.");
                    break;
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
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: false);
            Interlocked.Exchange(ref _receiveLoopRunning, 0);
        }
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        if (_disposed) return;
        
        await WaitForConnection(cancellationToken);

        if (_webSocket.State != WebSocketState.Open)
            return;

        var byteCount = Encoding.UTF8.GetByteCount(message);
        var buffer = ArrayPool<byte>.Shared.Rent(byteCount);
        try
        {
            Encoding.UTF8.GetBytes(message, 0, message.Length, buffer, 0);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, byteCount), WebSocketMessageType.Text, true, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"SendMessageAsync error: {ex}");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: false);
        }
    }

    public async Task ReconnectAsync()
    {
        if (_disposed) return;
        
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
        if (_disposed) return;
        
        _manuallyDisconnected = true;
        
        if (_autoReconnect)
        {
            _reconnectTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        try
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DisconnectAsync error: {ex}");
        }

        StopReceivingMessages();
        _whenStateChanged.OnNextIfValueChanged(_webSocket.State);
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
        if (_disposed) return;
        
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
        if (_disposed || _webSocket.State is WebSocketState.Connecting or WebSocketState.Open or WebSocketState.None)
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
            if (!_whenStateChecked.IsDisposed)
                _whenStateChecked.OnNext(_webSocket.State);
        }
    }
}