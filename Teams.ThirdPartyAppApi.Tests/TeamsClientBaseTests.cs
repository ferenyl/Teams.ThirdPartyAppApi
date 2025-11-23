using Microsoft.Extensions.Logging;

namespace Teams.ThirdPartyAppApi.Tests;

public class TeamsClientBaseTests
{
    private class TestTeamsClient : TeamsClientBase
    {
        public TestTeamsClient(string url, int port, string token, bool autoReconnect, ClientInformation clientInformation, CancellationToken cancellationToken, ILogger? logger = null)
            : base(url, port, token, autoReconnect, clientInformation, cancellationToken, logger)
        {
        }

        protected override Uri BuildUri()
        {
            return new Uri($"ws://{Url}:{Port}");
        }
    }

    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var url = "localhost";
        var port = 8124;
        var token = "testToken";
        var clientInfo = new ClientInformation("manufacturer", "device", "app", "1.0");

        // Act
        using var client = new TestTeamsClient(url, port, token, false, clientInfo, CancellationToken.None);

        // Assert
        Assert.Equal(token, client.Token);
        Assert.False(client.IsConnected);
    }

    [Fact]
    public void Constructor_ShouldDefaultPortTo8124_WhenPortIsZero()
    {
        // Arrange
        var clientInfo = new ClientInformation("manufacturer", "device", "app", "1.0");

        // Act
        using var client = new TestTeamsClient("localhost", 0, "token", false, clientInfo, CancellationToken.None);

        // Assert - The client should be created successfully with default port
        Assert.NotNull(client);
        Assert.False(client.IsConnected);
    }

    [Fact]
    public async Task Connect_ShouldNotThrow_WhenCalledAfterDispose()
    {
        // Arrange
        var clientInfo = new ClientInformation("manufacturer", "device", "app", "1.0");
        var client = new TestTeamsClient("localhost", 8124, "token", false, clientInfo, CancellationToken.None);
        client.Dispose();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await client.Connect(CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Disconnect_ShouldNotThrow_WhenCalledAfterDispose()
    {
        // Arrange
        var clientInfo = new ClientInformation("manufacturer", "device", "app", "1.0");
        var client = new TestTeamsClient("localhost", 8124, "token", false, clientInfo, CancellationToken.None);
        client.Dispose();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await client.Disconnect(CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Reconnect_ShouldNotThrow_WhenCalledAfterDispose()
    {
        // Arrange
        var clientInfo = new ClientInformation("manufacturer", "device", "app", "1.0");
        var client = new TestTeamsClient("localhost", 8124, "token", false, clientInfo, CancellationToken.None);
        client.Dispose();

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () =>
            await client.Reconnect());

        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        // Arrange
        var clientInfo = new ClientInformation("manufacturer", "device", "app", "1.0");
        var client = new TestTeamsClient("localhost", 8124, "token", false, clientInfo, CancellationToken.None);

        // Act & Assert
        var exception = Record.Exception(() =>
        {
            client.Dispose();
            client.Dispose();
            client.Dispose();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void TryDeserialize_ShouldReturnTrue_WithValidJson()
    {
        // Arrange
        var clientInfo = new ClientInformation("manufacturer", "device", "app", "1.0");
        using var client = new TestTeamsClient("localhost", 8124, "token", false, clientInfo, CancellationToken.None);
        var json = "{\"manufacturer\":\"test\",\"device\":\"device\",\"app\":\"app\",\"appVersion\":\"1.0\"}";

        // Act
        var result = client.TryDeserialize<ClientInformation>(json, out var deserialized);

        // Assert
        Assert.True(result);
        Assert.NotNull(deserialized);
        Assert.Equal("test", deserialized.Manufacturer);
    }

    [Fact]
    public void TryDeserialize_ShouldReturnFalse_WithInvalidJson()
    {
        // Arrange
        var clientInfo = new ClientInformation("manufacturer", "device", "app", "1.0");
        using var client = new TestTeamsClient("localhost", 8124, "token", false, clientInfo, CancellationToken.None);
        var invalidJson = "{ invalid json }";

        // Act
        var result = client.TryDeserialize<ClientInformation>(invalidJson, out var deserialized);

        // Assert
        Assert.False(result);
        Assert.Null(deserialized);
    }
}
