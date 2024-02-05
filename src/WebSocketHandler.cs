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
    private IClientWebSocket _webSocket;
    private readonly BehaviorSubject<WebSocketState> _whenStateChanged;
    private readonly Subject<WebSocketState> _whenStateChecked = new();
    private readonly Subject<string> _messageSubject = new Subject<string>();
    internal readonly Uri _uri;
    private readonly bool _autoReconnect;
    internal CancellationToken _cancellationToken;
    private CancellationTokenSource? _receiveCancellationTokenSource;
    private readonly SemaphoreSlim _connectLock = new(1, 1);

    private bool _manuallyDisconnected = false;

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
            && !_manuallyDisconnected )
            .Subscribe(async _ => await ReconnectAsync());

        var timer = new Timer((state) => AutoReconnectCallback(), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }

    private void AutoReconnectCallback()
    {
        if (_webSocket.State is WebSocketState.Connecting or WebSocketState.Open or WebSocketState.None)
        {
            return;
        }

        _whenStateChecked.OnNext(_webSocket.State);
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

                _cancellationToken = cancellationToken;
                _manuallyDisconnected = false;
                await _webSocket.ConnectAsync(_uri, cancellationToken);
                _whenStateChanged.OnNextIfValueChanged(_webSocket.State);
                StartReceivingMessagesAsync(cancellationToken);
            }
        }
        catch (Exception)
        {
            //silent
        }
        finally
        {
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
        await WaitForConnection(_cancellationToken);
        var buffer = new byte[1024 * 4];
        while (!cancellationToken.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            if(result is null)
                continue;

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _messageSubject.OnNext(message);
        }
    }

    public async Task SendMessageAsync(string message)
    {
        await WaitForConnection(_cancellationToken);

        if (_webSocket.State != WebSocketState.Open)
            return;

        var buffer = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationToken);
    }

    public async Task ReconnectAsync()
    {
        await WaitForConnection(_cancellationToken);

        if (_webSocket.State is WebSocketState.Open or WebSocketState.Connecting)
            return;

        if (!_cancellationToken.IsCancellationRequested)
        {
            await ConnectAsync(_cancellationToken);
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        _manuallyDisconnected = true;

        try { await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken); }
        catch { }

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
}