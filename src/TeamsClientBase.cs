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
    public string Token { get; protected set; } = string.Empty;
    protected string Url { get; }
    protected int Port { get; }
    protected readonly ClientInformation _clientInformation;

    protected readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected TeamsClientBase(string url, int port, string token, bool autoReconnect, ClientInformation clientInformation, CancellationToken cancellationToken)
    {
        Token = token;
        Url = url;
        Port = port == 0 ? 8124 : port;
        _autoReconnect = autoReconnect;
        _cancellationToken = cancellationToken;
        _clientInformation = clientInformation;
        Uri = BuildUri();

        _socket = new WebSocketHandler(Uri, autoReconnect);

        _ = _socket.ConnectionStatus
            .Subscribe(state =>
            _isConnectedChanged.OnNextIfValueChanged(state == WebSocketState.Open));

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
    
    internal async Task SendCommand(string clientMessage)
    {
        await _socket.SendMessageAsync(clientMessage);
    }

    internal bool TryDeserialize <T>(string json, out T? result)
    {
        try        
        {
            result = JsonSerializer.Deserialize<T>(json, _serializerOptions);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}