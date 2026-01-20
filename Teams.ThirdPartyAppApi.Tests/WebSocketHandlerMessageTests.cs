using Moq;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using System.Text;
using Teams.ThirdPartyAppApi.Adapters;

namespace Teams.ThirdPartyAppApi.Tests;

public class WebSocketHandlerMessageTests
{
    [Fact]
    public async Task SendMessageAsync_ShouldNotSend_WhenWebSocketIsNotOpen()
    {
        // Arrange
        var mockClientWebSocket = new Mock<IClientWebSocket>();
        mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.None);
        var statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        var webSocketHandler = new WebSocketHandler(new Uri("ws://localhost"), false, mockClientWebSocket.Object, statusSubject);

        // Act
        await webSocketHandler.SendMessageAsync("test message", CancellationToken.None);

        // Assert
        mockClientWebSocket.Verify(ws => ws.SendAsync(
            It.IsAny<ArraySegment<byte>>(),
            It.IsAny<WebSocketMessageType>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldSendMessage_WhenWebSocketIsOpen()
    {
        // Arrange
        var mockClientWebSocket = new Mock<IClientWebSocket>();
        mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.Open);
        var statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        var webSocketHandler = new WebSocketHandler(new Uri("ws://localhost"), false, mockClientWebSocket.Object, statusSubject);
        var testMessage = "test message";
        var expectedBytes = Encoding.UTF8.GetBytes(testMessage);

        // Act
        await webSocketHandler.SendMessageAsync(testMessage, CancellationToken.None);

        // Assert
        mockClientWebSocket.Verify(ws => ws.SendAsync(
            It.Is<ArraySegment<byte>>(seg => 
                seg.Count == expectedBytes.Length && 
                seg.Array != null),
            WebSocketMessageType.Text,
            true,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldHandleUnicodeCharacters()
    {
        // Arrange
        var mockClientWebSocket = new Mock<IClientWebSocket>();
        mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.Open);
        var statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        var webSocketHandler = new WebSocketHandler(new Uri("ws://localhost"), false, mockClientWebSocket.Object, statusSubject);
        var unicodeMessage = "Hello 世界 🌍";

        // Act
        await webSocketHandler.SendMessageAsync(unicodeMessage, CancellationToken.None);

        // Assert
        mockClientWebSocket.Verify(ws => ws.SendAsync(
            It.IsAny<ArraySegment<byte>>(),
            WebSocketMessageType.Text,
            true,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldHandleEmptyString()
    {
        // Arrange
        var mockClientWebSocket = new Mock<IClientWebSocket>();
        mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.Open);
        var statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        var webSocketHandler = new WebSocketHandler(new Uri("ws://localhost"), false, mockClientWebSocket.Object, statusSubject);

        // Act
        await webSocketHandler.SendMessageAsync(string.Empty, CancellationToken.None);

        // Assert
        mockClientWebSocket.Verify(ws => ws.SendAsync(
            It.Is<ArraySegment<byte>>(seg => seg.Count == 0),
            WebSocketMessageType.Text,
            true,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldHandleLargeMessages()
    {
        // Arrange
        var mockClientWebSocket = new Mock<IClientWebSocket>();
        mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.Open);
        var statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        var webSocketHandler = new WebSocketHandler(new Uri("ws://localhost"), false, mockClientWebSocket.Object, statusSubject);
        var largeMessage = new string('a', 10000);

        // Act
        await webSocketHandler.SendMessageAsync(largeMessage, CancellationToken.None);

        // Assert
        mockClientWebSocket.Verify(ws => ws.SendAsync(
            It.Is<ArraySegment<byte>>(seg => seg.Count == 10000),
            WebSocketMessageType.Text,
            true,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void ReceivedMessages_ShouldBeObservable()
    {
        // Arrange
        var mockClientWebSocket = new Mock<IClientWebSocket>();
        mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.None);
        var statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        var webSocketHandler = new WebSocketHandler(new Uri("ws://localhost"), false, mockClientWebSocket.Object, statusSubject);

        // Act
        var observable = webSocketHandler.ReceivedMessages;

        // Assert
        Assert.NotNull(observable);
    }

    [Fact]
    public void ConnectionStatus_ShouldBeObservable()
    {
        // Arrange
        var mockClientWebSocket = new Mock<IClientWebSocket>();
        mockClientWebSocket.SetupGet(ws => ws.State).Returns(WebSocketState.None);
        var statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        var webSocketHandler = new WebSocketHandler(new Uri("ws://localhost"), false, mockClientWebSocket.Object, statusSubject);

        // Act
        var observable = webSocketHandler.ConnectionStatus;

        // Assert
        Assert.NotNull(observable);
    }
}
