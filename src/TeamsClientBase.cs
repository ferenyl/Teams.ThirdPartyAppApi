using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;

namespace Teams.ThirdPartyAppApi;

public abstract class TeamsClientBase
{
    protected readonly BehaviorSubject<bool> _isConnectedChanged = new(false);
    protected readonly Subject<string> _receivedMessages = new();
    protected readonly bool _autoReconnect;
    protected readonly CancellationToken _cancellationToken;
    private Uri Uri { get; set; }
    private readonly WebSocketHandler _socket;
    protected string Token { get; set; } = string.Empty;
    protected string Url { get; }

    protected readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected TeamsClientBase(string url, string token, bool autoReconnect, CancellationToken cancellationToken)
    {
        Token = token;
        Url = url;
        _autoReconnect = autoReconnect;
        _cancellationToken = cancellationToken;
        Uri = BuildUri();

        _socket = new WebSocketHandler(Uri, autoReconnect);

        _socket.ConnectionStatus
            .Subscribe(state => _isConnectedChanged.OnNextIfValueChanged(state == WebSocketState.Open));

        _socket.ReceivedMessages.Subscribe(message => _receivedMessages.OnNext(message));
    }

    public IObservable<bool> IsConnectedChanged => _isConnectedChanged.AsObservable();
    public IObservable<string> ReceivedMessages => _receivedMessages.AsObservable();
    public bool IsConnected => _isConnectedChanged.Value;

    protected abstract Uri BuildUri();

    public async Task Connect(CancellationToken cancellationToken = default)
    {
        await _socket.ConnectAsync(cancellationToken);
    }

    public async Task Disconnect(CancellationToken cancellationToken = default)
    {
        await _socket.DisconnectAsync(cancellationToken);
    }

    public async Task Reconnect()
    {
        await _socket.ReconnectAsync();
    }
    protected async Task SendCommand(string clientMessage)
    {
        await _socket.SendMessageAsync(clientMessage);
    }
}