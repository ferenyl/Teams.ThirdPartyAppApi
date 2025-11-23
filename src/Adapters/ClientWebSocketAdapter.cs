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
    private bool _disposed;

    public ClientWebSocketAdapter()
    {
        _clientWebSocket = new ClientWebSocket();
    }

    public void CreateNewSocket()
    {
        if (_disposed) return;
        
        try { _clientWebSocket.Dispose(); }
        catch { }

        _clientWebSocket = new ClientWebSocket();
    }

    public WebSocketState State => _clientWebSocket.State;
    public WebSocketCloseStatus? CloseStatus => _clientWebSocket.CloseStatus;
    public string? CloseStatusDescription => _clientWebSocket.CloseStatusDescription;

    public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken) =>
        await _clientWebSocket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);

    public async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken) =>
        await _clientWebSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken).ConfigureAwait(false);

    public async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken) =>
        await _clientWebSocket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);

    public async Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) =>
        await _clientWebSocket.CloseAsync(closeStatus, statusDescription, cancellationToken).ConfigureAwait(false);

    public async Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) =>
        await _clientWebSocket.CloseOutputAsync(closeStatus, statusDescription, cancellationToken).ConfigureAwait(false);

    public void Abort() =>
        _clientWebSocket.Abort();

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        try { _clientWebSocket?.Dispose(); }
        catch { }
    }
}
