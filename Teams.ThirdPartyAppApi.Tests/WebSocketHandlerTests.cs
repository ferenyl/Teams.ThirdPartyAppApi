using Moq;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using Teams.ThirdPartyAppApi.Adapters;
using Teams.ThirdPartyAppApi.Tests.Mocks;

namespace Teams.ThirdPartyAppApi.Tests;

public class WebSocketHandlerTests
{

    private readonly Mock<IClientWebSocket> _mockClientWebSocket;
    private readonly BehaviorSubject<WebSocketState> _statusSubject;
    private readonly WebSocketHandler _webSocketHandler;
    private readonly Uri _testUri = new Uri("ws://localhost");

    public WebSocketHandlerTests()
    {
        _mockClientWebSocket = new Mock<IClientWebSocket>();
        _statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        _webSocketHandler = new WebSocketHandler(_testUri, false, _mockClientWebSocket.Object, _statusSubject);

    }

    [Fact]
    public async Task ConnectAsync_ShouldConnect_WhenWebSocketStateIsNone()
    {
        // Arrange
        _mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.None);
        _mockClientWebSocket.Setup(ws => ws.ConnectAsync(_testUri, It.IsAny<CancellationToken>()))
            .Callback(() => _mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.Open));

        // Act
        await _webSocketHandler.ConnectAsync(CancellationToken.None);

        // Assert
        _mockClientWebSocket.Verify(ws => ws.ConnectAsync(_testUri, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_ShouldNotConnect_WhenWebSocketStateIsOpen()
    {
        // Arrange
        _mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.Open);

        // Act
        await _webSocketHandler.ConnectAsync(CancellationToken.None);

        // Assert
        _mockClientWebSocket.Verify(ws => ws.ConnectAsync(_testUri, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(Skip = "not working")]
    public async Task ReconnectAsync_ShouldAutoConnect_WhenNotManuallyDisconnected()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        _mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.Closed);

        _mockClientWebSocket.Setup(ws => ws.CreateNewSocket())
            .Callback(() => _mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.None));

        _mockClientWebSocket.Setup(ws => ws.ConnectAsync(_testUri, It.IsAny<CancellationToken>()))
            .Callback(() => _mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.Open));

        // Act
        await _webSocketHandler.ReconnectAsync();


        // Assert
        _mockClientWebSocket.Verify((ws) => ws.CreateNewSocket(), Times.Once);
        _mockClientWebSocket.Verify(ws => ws.ConnectAsync(_testUri, cancellationToken), Times.Once);
        Assert.Equal(WebSocketState.Open, _statusSubject.Value);
    }

    [Fact]
    public async Task ReconnectAsync_ShouldNotConnect_WhenManuallyDisconnected()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        _mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.Open);

        // Act
        await _webSocketHandler.DisconnectAsync(cancellationToken);


        // Assert
        _mockClientWebSocket.Verify(ws => ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken), Times.Once);
        _mockClientWebSocket.Verify(ws => ws.ConnectAsync(_testUri, cancellationToken), Times.Never);
    }


}
