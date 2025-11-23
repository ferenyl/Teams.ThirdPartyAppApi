using System.Net.WebSockets;
using Teams.ThirdPartyAppApi.Adapters;

namespace Teams.ThirdPartyAppApi.Tests;

public class ClientWebSocketAdapterTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithNoneState()
    {
        // Arrange & Act
        using var adapter = new ClientWebSocketAdapter();

        // Assert
        Assert.Equal(WebSocketState.None, adapter.State);
    }

    [Fact]
    public void CreateNewSocket_ShouldResetToNoneState()
    {
        // Arrange
        using var adapter = new ClientWebSocketAdapter();

        // Act
        adapter.CreateNewSocket();

        // Assert
        Assert.Equal(WebSocketState.None, adapter.State);
    }

    [Fact]
    public void CreateNewSocket_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        // Arrange
        using var adapter = new ClientWebSocketAdapter();

        // Act & Assert
        var exception = Record.Exception(() =>
        {
            adapter.CreateNewSocket();
            adapter.CreateNewSocket();
            adapter.CreateNewSocket();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        // Arrange
        var adapter = new ClientWebSocketAdapter();

        // Act & Assert
        var exception = Record.Exception(() =>
        {
            adapter.Dispose();
            adapter.Dispose();
            adapter.Dispose();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void CreateNewSocket_ShouldNotThrow_AfterDispose()
    {
        // Arrange
        var adapter = new ClientWebSocketAdapter();
        adapter.Dispose();

        // Act & Assert
        var exception = Record.Exception(() => adapter.CreateNewSocket());

        Assert.Null(exception);
    }

    [Fact]
    public void CloseStatus_ShouldBeNull_BeforeConnection()
    {
        // Arrange & Act
        using var adapter = new ClientWebSocketAdapter();

        // Assert
        Assert.Null(adapter.CloseStatus);
    }

    [Fact]
    public void CloseStatusDescription_ShouldBeNull_BeforeConnection()
    {
        // Arrange & Act
        using var adapter = new ClientWebSocketAdapter();

        // Assert
        Assert.Null(adapter.CloseStatusDescription);
    }

    [Fact]
    public void Abort_ShouldNotThrow()
    {
        // Arrange
        using var adapter = new ClientWebSocketAdapter();

        // Act & Assert
        var exception = Record.Exception(() => adapter.Abort());

        Assert.Null(exception);
    }
}
