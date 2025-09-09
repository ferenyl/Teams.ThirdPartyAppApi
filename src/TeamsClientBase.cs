using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Teams.ThirdPartyAppApi;

public abstract class TeamsClientBase : IDisposable
{
    protected readonly BehaviorSubject<bool> _isConnectedChanged = new(false);
    protected readonly Subject<string> _receivedMessages = new();
    protected readonly bool _autoReconnect;
    protected readonly CancellationToken _cancellationToken;
    
    protected Uri Uri { get; private set; }
    private readonly WebSocketHandler _socket;
    public string Token { get; protected set; } = string.Empty;
    protected readonly string Url;
    protected readonly int Port;
    protected readonly ClientInformation _clientInformation;

    protected readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IDisposable _connectionStatusSubscription;
    private readonly IDisposable _receivedMessagesSubscription;
    private readonly ILogger? _logger;

    protected TeamsClientBase(string url, int port, string token, bool autoReconnect, ClientInformation clientInformation, CancellationToken cancellationToken, ILogger? logger = null)
    {
        Token = token;
        Url = url;
        Port = port == 0 ? 8124 : port;
        _autoReconnect = autoReconnect;
        _cancellationToken = cancellationToken;
        _clientInformation = clientInformation;
        Uri = BuildUri();
        _logger = logger;

        _socket = new WebSocketHandler(Uri, autoReconnect);

        _connectionStatusSubscription = _socket.ConnectionStatus
            .Subscribe(state =>
            _isConnectedChanged.OnNextIfValueChanged(state == WebSocketState.Open));

        _receivedMessagesSubscription = _socket.ReceivedMessages.Subscribe(message => _receivedMessages.OnNext(message));
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
        await _socket.SendMessageAsync(clientMessage, _cancellationToken);
    }

    internal bool TryDeserialize<T>(string json, out T? result)
    {
        try        
        {
            result = JsonSerializer.Deserialize<T>(json, _serializerOptions);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to deserialize JSON: {Json}", json);
            result = default;
            return false;
        }
    }

    public virtual void Dispose()
    {
        _connectionStatusSubscription.Dispose();
        _receivedMessagesSubscription.Dispose();
        _isConnectedChanged.Dispose();
        _receivedMessages.Dispose();
        _socket.Dispose();
    }
}