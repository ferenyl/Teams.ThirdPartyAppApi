using Moq;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using Teams.ThirdPartyAppApi.Adapters;

namespace Teams.ThirdPartyAppApi.Tests;

public class WebSocketHandlerDisposeTests
{
    [Fact]
    public void Dispose_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        // Arrange
        var mockClientWebSocket = new Mock<IClientWebSocket>();
        mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.None);
        var statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        var webSocketHandler = new WebSocketHandler(new Uri("ws://localhost"), false, mockClientWebSocket.Object, statusSubject);

        // Act & Assert
        var exception = Record.Exception(() =>
        {
            webSocketHandler.Dispose();
            webSocketHandler.Dispose();
            webSocketHandler.Dispose();
        });

        Assert.Null(exception);
    }

    [Fact]
    public async Task ConnectAsync_ShouldNotThrow_WhenCalledAfterDispose()
    {
        // Arrange
        var mockClientWebSocket = new Mock<IClientWebSocket>();
        mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.None);
        var statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        var webSocketHandler = new WebSocketHandler(new Uri("ws://localhost"), false, mockClientWebSocket.Object, statusSubject);
        webSocketHandler.Dispose();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await webSocketHandler.ConnectAsync(CancellationToken.None));

        Assert.Null(exception);
        mockClientWebSocket.Verify(ws => ws.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldNotThrow_WhenCalledAfterDispose()
    {
        // Arrange
        var mockClientWebSocket = new Mock<IClientWebSocket>();
        mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.Open);
        var statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        var webSocketHandler = new WebSocketHandler(new Uri("ws://localhost"), false, mockClientWebSocket.Object, statusSubject);
        webSocketHandler.Dispose();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await webSocketHandler.SendMessageAsync("test", CancellationToken.None));

        Assert.Null(exception);
        mockClientWebSocket.Verify(ws => ws.SendAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<WebSocketMessageType>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DisconnectAsync_ShouldNotThrow_WhenCalledAfterDispose()
    {
        // Arrange
        var mockClientWebSocket = new Mock<IClientWebSocket>();
        mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.Open);
        var statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        var webSocketHandler = new WebSocketHandler(new Uri("ws://localhost"), false, mockClientWebSocket.Object, statusSubject);
        webSocketHandler.Dispose();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await webSocketHandler.DisconnectAsync(CancellationToken.None));

        Assert.Null(exception);
        mockClientWebSocket.Verify(ws => ws.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReconnectAsync_ShouldNotThrow_WhenCalledAfterDispose()
    {
        // Arrange
        var mockClientWebSocket = new Mock<IClientWebSocket>();
        mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.Closed);
        var statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        var webSocketHandler = new WebSocketHandler(new Uri("ws://localhost"), false, mockClientWebSocket.Object, statusSubject);
        webSocketHandler.Dispose();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await webSocketHandler.ReconnectAsync());

        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_ShouldDisposeUnderlyingWebSocket()
    {
        // Arrange
        var mockClientWebSocket = new Mock<IClientWebSocket>();
        mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.None);
        var statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        var webSocketHandler = new WebSocketHandler(new Uri("ws://localhost"), false, mockClientWebSocket.Object, statusSubject);

        // Act
        webSocketHandler.Dispose();

        // Assert
        mockClientWebSocket.Verify(ws => ws.Dispose(), Times.Once);
    }
}
