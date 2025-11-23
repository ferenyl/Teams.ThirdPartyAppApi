using System.Text.Json;
using Teams.ThirdPartyAppApi.TeamsClient;

namespace Teams.ThirdPartyAppApi.Tests;

public class ClientMessageTests
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void ClientMessage_ShouldSerializeCorrectly_WithParameters()
    {
        // Arrange
        var message = new ClientMessage("test-action", "test-param")
        {
            RequestId = 1
        };

        // Act
        var json = JsonSerializer.Serialize(message, _serializerOptions);
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("test-action", deserialized["action"].GetString());
        Assert.Equal(1, deserialized["requestId"].GetInt32());
        Assert.True(deserialized.ContainsKey("parameters"));
    }

    [Fact]
    public void ClientMessage_ShouldSerializeCorrectly_WithoutParameters()
    {
        // Arrange
        var message = new ClientMessage("test-action", null)
        {
            RequestId = 2
        };

        // Act
        var json = JsonSerializer.Serialize(message, _serializerOptions);

        // Assert
        Assert.Contains("\"action\":\"test-action\"", json);
        Assert.Contains("\"requestId\":2", json);
    }

    [Fact]
    public void ToggleMute_ShouldHaveCorrectAction()
    {
        // Arrange & Act
        var message = ClientMessage.ToggleMute;

        // Assert
        Assert.Equal("toggle-mute", message.Action);
    }

    [Fact]
    public void ToggleVideo_ShouldHaveCorrectAction()
    {
        // Arrange & Act
        var message = ClientMessage.ToggleVideo;

        // Assert
        Assert.Equal("toggle-video", message.Action);
    }

    [Fact]
    public void ToggleHand_ShouldHaveCorrectAction()
    {
        // Arrange & Act
        var message = ClientMessage.ToggleHand;

        // Assert
        Assert.Equal("toggle-hand", message.Action);
    }

    [Fact]
    public void ToggleBackgroundBlur_ShouldHaveCorrectAction()
    {
        // Arrange & Act
        var message = ClientMessage.ToggleBackgroundBlur;

        // Assert
        Assert.Equal("toggle-background-blur", message.Action);
    }

    [Fact]
    public void LeaveCall_ShouldHaveCorrectAction()
    {
        // Arrange & Act
        var message = ClientMessage.LeaveCall;

        // Assert
        Assert.Equal("leave-call", message.Action);
    }

    [Fact]
    public void StopSharing_ShouldHaveCorrectAction()
    {
        // Arrange & Act
        var message = ClientMessage.StopSharing;

        // Assert
        Assert.Equal("stop-sharing", message.Action);
    }

    [Fact]
    public void QueryState_ShouldHaveCorrectAction()
    {
        // Arrange & Act
        var message = ClientMessage.QueryState;

        // Assert
        Assert.Equal("query-state", message.Action);
    }

    [Fact]
    public void SendReactionApplause_ShouldHaveCorrectActionAndParameter()
    {
        // Arrange & Act
        var message = ClientMessage.SendReactionApplause;

        // Assert
        Assert.Equal("send-reaction", message.Action);
        Assert.Equal("applause", message.Parameters.Type);
    }

    [Fact]
    public void SendReactionLaugh_ShouldHaveCorrectActionAndParameter()
    {
        // Arrange & Act
        var message = ClientMessage.SendReactionLaugh;

        // Assert
        Assert.Equal("send-reaction", message.Action);
        Assert.Equal("laugh", message.Parameters.Type);
    }

    [Fact]
    public void SendReactionLike_ShouldHaveCorrectActionAndParameter()
    {
        // Arrange & Act
        var message = ClientMessage.SendReactionLike;

        // Assert
        Assert.Equal("send-reaction", message.Action);
        Assert.Equal("like", message.Parameters.Type);
    }

    [Fact]
    public void SendReactionLove_ShouldHaveCorrectActionAndParameter()
    {
        // Arrange & Act
        var message = ClientMessage.SendReactionLove;

        // Assert
        Assert.Equal("send-reaction", message.Action);
        Assert.Equal("love", message.Parameters.Type);
    }

    [Fact]
    public void SendReactionWow_ShouldHaveCorrectActionAndParameter()
    {
        // Arrange & Act
        var message = ClientMessage.SendReactionWow;

        // Assert
        Assert.Equal("send-reaction", message.Action);
        Assert.Equal("wow", message.Parameters.Type);
    }

    [Fact]
    public void ToggleUiChat_ShouldHaveCorrectActionAndParameter()
    {
        // Arrange & Act
        var message = ClientMessage.ToggleUiChat;

        // Assert
        Assert.Equal("toggle-ui", message.Action);
        Assert.Equal("chat", message.Parameters.Type);
    }

    [Fact]
    public void ToggleUiShareTray_ShouldHaveCorrectActionAndParameter()
    {
        // Arrange & Act
        var message = ClientMessage.ToggleUiShareTray;

        // Assert
        Assert.Equal("toggle-ui", message.Action);
        Assert.Equal("share-tray", message.Parameters.Type);
    }

    [Fact]
    public void RequestId_ShouldBeSettable()
    {
        // Arrange
        var message = ClientMessage.QueryState;

        // Act
        message.RequestId = 42;

        // Assert
        Assert.Equal(42, message.RequestId);
    }
}
