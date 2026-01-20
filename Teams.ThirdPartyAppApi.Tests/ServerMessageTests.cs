using System.Text.Json;
using Teams.ThirdPartyAppApi.TeamsClient;

namespace Teams.ThirdPartyAppApi.Tests;

public class ServerMessageTests
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void ServerMessage_ShouldDeserialize_WithAllProperties()
    {
        // Arrange
        var json = @"{
            ""requestId"": 1,
            ""response"": ""Success"",
            ""errorMsg"": """",
            ""tokenRefresh"": ""newToken123"",
            ""meetingUpdate"": {
                ""meetingState"": {
                    ""isMuted"": true,
                    ""isHandRaised"": false,
                    ""isInMeeting"": true,
                    ""isRecordingOn"": false,
                    ""isBackgroundBlurred"": true,
                    ""isSharing"": false,
                    ""hasUnreadMessages"": true,
                    ""isVideoOn"": false
                },
                ""meetingPermissions"": {
                    ""canToggleMute"": true,
                    ""canToggleVideo"": true,
                    ""canToggleHand"": true,
                    ""canToggleBlur"": true,
                    ""canLeave"": true,
                    ""canReact"": true,
                    ""canToggleShareTray"": true,
                    ""canToggleChat"": true,
                    ""canStopSharing"": false,
                    ""canPair"": true
                }
            }
        }";

        // Act
        var message = JsonSerializer.Deserialize<ServerMessage>(json, _serializerOptions);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(1, message.RequestId);
        Assert.Equal("Success", message.Response);
        Assert.Equal(string.Empty, message.ErrorMsg);
        Assert.Equal("newToken123", message.TokenRefresh);
        
        Assert.NotNull(message.MeetingUpdate);
        Assert.NotNull(message.MeetingUpdate.MeetingState);
        Assert.True(message.MeetingUpdate.MeetingState.IsMuted);
        Assert.False(message.MeetingUpdate.MeetingState.IsHandRaised);
        Assert.True(message.MeetingUpdate.MeetingState.IsInMeeting);
        Assert.False(message.MeetingUpdate.MeetingState.IsRecordingOn);
        Assert.True(message.MeetingUpdate.MeetingState.IsBackgroundBlurred);
        Assert.False(message.MeetingUpdate.MeetingState.IsSharing);
        Assert.True(message.MeetingUpdate.MeetingState.HasUnreadMessages);
        Assert.False(message.MeetingUpdate.MeetingState.IsVideoOn);
        
        Assert.NotNull(message.MeetingUpdate.MeetingPermissions);
        Assert.True(message.MeetingUpdate.MeetingPermissions.CanToggleMute);
        Assert.True(message.MeetingUpdate.MeetingPermissions.CanToggleVideo);
        Assert.True(message.MeetingUpdate.MeetingPermissions.CanToggleHand);
        Assert.True(message.MeetingUpdate.MeetingPermissions.CanToggleBlur);
        Assert.True(message.MeetingUpdate.MeetingPermissions.CanLeave);
        Assert.True(message.MeetingUpdate.MeetingPermissions.CanReact);
        Assert.True(message.MeetingUpdate.MeetingPermissions.CanToggleShareTray);
        Assert.True(message.MeetingUpdate.MeetingPermissions.CanToggleChat);
        Assert.False(message.MeetingUpdate.MeetingPermissions.CanStopSharing);
        Assert.True(message.MeetingUpdate.MeetingPermissions.CanPair);
    }

    [Fact]
    public void ServerMessage_ShouldDeserialize_ErrorMessage()
    {
        // Arrange
        var json = @"{
            ""requestId"": 2,
            ""response"": ""Error"",
            ""errorMsg"": ""no active call""
        }";

        // Act
        var message = JsonSerializer.Deserialize<ServerMessage>(json, _serializerOptions);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(2, message.RequestId);
        Assert.Equal("Error", message.Response);
        Assert.Equal("no active call", message.ErrorMsg);
    }

    [Fact]
    public void ServerMessage_ShouldDeserialize_TokenRefresh()
    {
        // Arrange
        var json = @"{
            ""requestId"": 3,
            ""tokenRefresh"": ""refreshedToken456""
        }";

        // Act
        var message = JsonSerializer.Deserialize<ServerMessage>(json, _serializerOptions);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(3, message.RequestId);
        Assert.Equal("refreshedToken456", message.TokenRefresh);
    }

    [Fact]
    public void MeetingState_ShouldHaveDefaultValues()
    {
        // Act
        var meetingState = new MeetingState();

        // Assert
        Assert.False(meetingState.IsMuted);
        Assert.False(meetingState.IsHandRaised);
        Assert.False(meetingState.IsInMeeting);
        Assert.False(meetingState.IsRecordingOn);
        Assert.False(meetingState.IsBackgroundBlurred);
        Assert.False(meetingState.IsSharing);
        Assert.False(meetingState.HasUnreadMessages);
        Assert.False(meetingState.IsVideoOn);
    }

    [Fact]
    public void MeetingPermissions_ShouldHaveDefaultValues()
    {
        // Act
        var permissions = new MeetingPermissions();

        // Assert
        Assert.False(permissions.CanToggleMute);
        Assert.False(permissions.CanToggleVideo);
        Assert.False(permissions.CanToggleHand);
        Assert.False(permissions.CanToggleBlur);
        Assert.False(permissions.CanLeave);
        Assert.False(permissions.CanReact);
        Assert.False(permissions.CanToggleShareTray);
        Assert.False(permissions.CanToggleChat);
        Assert.False(permissions.CanStopSharing);
        Assert.False(permissions.CanPair);
    }
}
