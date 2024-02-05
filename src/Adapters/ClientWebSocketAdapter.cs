using System.Net.WebSockets;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Teams.ThirdPartyAppApi.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Teams.ThirdPartyAppApi.Adapters;
internal interface IClientWebSocket : IDisposable
{
    WebSocketState State { get; }
    Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
    Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
    Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
    Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);
    Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);
    void Abort();
    void CreateNewSocket();
    WebSocketCloseStatus? CloseStatus { get; }
    string? CloseStatusDescription { get; }
}

internal class ClientWebSocketAdapter : IClientWebSocket, IDisposable
{
    private ClientWebSocket _clientWebSocket;

    public ClientWebSocketAdapter()
    {
        _clientWebSocket = new ClientWebSocket();
    }

    public void CreateNewSocket()
    {
        try { _clientWebSocket.Dispose(); }
        catch { }

        _clientWebSocket = new ClientWebSocket();
    }

    public WebSocketState State => _clientWebSocket.State;
    public WebSocketCloseStatus? CloseStatus => _clientWebSocket.CloseStatus;
    public string? CloseStatusDescription => _clientWebSocket.CloseStatusDescription;

    public Task ConnectAsync(Uri uri, CancellationToken cancellationToken) =>
        _clientWebSocket.ConnectAsync(uri, cancellationToken);

    public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken) =>
        _clientWebSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);

    public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken) =>
        _clientWebSocket.ReceiveAsync(buffer, cancellationToken);

    public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) =>
        _clientWebSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);

    public Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) =>
        _clientWebSocket.CloseOutputAsync(closeStatus, statusDescription, cancellationToken);

    public void Abort() =>
        _clientWebSocket.Abort();

    public void Dispose()
    {
        try { _clientWebSocket.Dispose(); }
        catch { }
    }
}

