using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using Teams.ThirdPartyAppApi.Adapters;

[assembly: InternalsVisibleTo("Teams.ThirdPartyAppApi.Tests")]
namespace Teams.ThirdPartyAppApi;
internal class WebSocketHandler
{
    private readonly IClientWebSocket _webSocket;
    private readonly BehaviorSubject<WebSocketState> _statusSubject;
    private readonly Subject<string> _messageSubject = new Subject<string>();
    internal readonly Uri _uri;
    private readonly bool _autoReconnect;
    internal CancellationToken _cancellationToken;
    private CancellationTokenSource? _receiveCancellationTokenSource;
    private readonly SemaphoreSlim _connectLock = new SemaphoreSlim(1, 1);

    // TODO: add send and recieve locks
    private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _recieveLock = new SemaphoreSlim(1, 1);

    private bool _manuallyDisconnected = false;

    public IObservable<WebSocketState> ConnectionStatus => _statusSubject.AsObservable();
    public IObservable<string> ReceivedMessages => _messageSubject.AsObservable();

    public WebSocketHandler(Uri uri, bool autoReconnect) : this(uri, autoReconnect, new ClientWebSocketAdapter(), new BehaviorSubject<WebSocketState>(WebSocketState.None))
    {
    }

    internal WebSocketHandler(Uri uri, bool autoReconnect, IClientWebSocket clientWebSocket) : this(uri, autoReconnect, clientWebSocket, new BehaviorSubject<WebSocketState>(WebSocketState.None))
    {
    }

    internal WebSocketHandler(Uri uri, bool autoReconnect, IClientWebSocket clientWebSocket, BehaviorSubject<WebSocketState> statusSubject)
    {
        _statusSubject = statusSubject;
        _webSocket = clientWebSocket;
        _uri = uri;
        _autoReconnect = autoReconnect;
        _receiveCancellationTokenSource = null;
        ConnectionStatus
            .Where(state => (state == WebSocketState.Aborted || state == WebSocketState.Closed) && _autoReconnect)
            .Subscribe(async _ => await ReconnectAsync());
    }


    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await _connectLock.WaitAsync(cancellationToken);
        try
        {
            await WaitForConnection(cancellationToken);

            if (_webSocket.State is WebSocketState.None or WebSocketState.Closed or WebSocketState.Aborted)
            {
                _cancellationToken = cancellationToken;
                _manuallyDisconnected = false;
                await _webSocket.ConnectAsync(_uri, cancellationToken);
                _statusSubject.OnNext(_webSocket.State);
                await StartReceivingMessagesAsync(cancellationToken);
            }
        }
        finally
        {
            _connectLock.Release();
        }
    }

    private async Task WaitForConnection(CancellationToken cancellationToken)
    {
        if (_webSocket.State is not WebSocketState.Connecting)
            return;

        while (!cancellationToken.IsCancellationRequested && _webSocket.State == WebSocketState.Connecting)
        {
            await Task.Delay(15, cancellationToken);
        }
    }

    private async Task StartReceivingMessagesAsync(CancellationToken cancellationToken)
    {
        _receiveCancellationTokenSource = new CancellationTokenSource();
        var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _receiveCancellationTokenSource.Token).Token;
        await Task.Run(() => ReceiveMessagesAsync(combinedToken), cancellationToken);
    }

    private void StopReceivingMessages()
    {
        _receiveCancellationTokenSource?.Cancel();
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        // TODO: add semaphore to prevent multiple recieve attempts
        var buffer = new byte[1024 * 4];
        while (!cancellationToken.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _messageSubject.OnNext(message);
        }
    }

    public async Task SendMessageAsync(string message)
    {

        // TODO: add semaphore to prevent multiple send attempts

        await WaitForConnection(_cancellationToken);

        if (_webSocket.State == WebSocketState.Open)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationToken);
        }
        else
        {
            throw new InvalidOperationException("WebSocket is not in a valid state to send a message.");
        }
    }

    public async Task ReconnectAsync()
    {
        if (!_cancellationToken.IsCancellationRequested && !_manuallyDisconnected)
        {
            await DisconnectAsync(_cancellationToken);
            await _webSocket.ConnectAsync(_uri, _cancellationToken);
            _statusSubject.OnNext(_webSocket.State);
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        _manuallyDisconnected = true;

        WebSocketState[] validStates = [WebSocketState.Connecting, WebSocketState.Open, WebSocketState.CloseSent, WebSocketState.CloseReceived];

        if (!validStates.Contains(_webSocket.State))
        {
            return;
        }

        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
        StopReceivingMessages();
        _statusSubject.OnNext(_webSocket.State);
    }
}
