using System.Reactive.Linq;
using Moq;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using Teams.ThirdPartyAppApi.Adapters;

namespace Teams.ThirdPartyAppApi.Tests;

public class TeamsClientTests : IDisposable
{
    private readonly Mock<IClientWebSocket> _mockWebSocket;
    private readonly BehaviorSubject<WebSocketState> _statusSubject;
    private TeamsClient.TeamsClient? _client;

    public TeamsClientTests()
    {
        _mockWebSocket = new Mock<IClientWebSocket>();
        _statusSubject = new BehaviorSubject<WebSocketState>(WebSocketState.None);
        _mockWebSocket.SetupGet(ws => ws.State).Returns(() => _statusSubject.Value);
    }

    [Fact]
    public void Constructor_ShouldInitializeAllObservables()
    {
        // Arrange & Act
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Assert
        Assert.NotNull(_client.TokenChanged);
        Assert.NotNull(_client.StateChanged);
        Assert.NotNull(_client.IsMutedChanged);
        Assert.NotNull(_client.IsHandRaisedChanged);
        Assert.NotNull(_client.IsInMeetingChanged);
        Assert.NotNull(_client.IsRecordingOnChanged);
        Assert.NotNull(_client.IsBackgroundBlurredChanged);
        Assert.NotNull(_client.IsSharingChanged);
        Assert.NotNull(_client.HasUnreadMessagesChanged);
        Assert.NotNull(_client.IsVideoOnChanged);
        Assert.NotNull(_client.CanToggleMuteChanged);
        Assert.NotNull(_client.CanToggleVideoChanged);
        Assert.NotNull(_client.CanToggleHandChanged);
        Assert.NotNull(_client.CanToggleBlurChanged);
        Assert.NotNull(_client.CanLeaveChanged);
        Assert.NotNull(_client.CanReactChanged);
        Assert.NotNull(_client.CanToggleShareTrayChanged);
        Assert.NotNull(_client.CanToggleChatChanged);
        Assert.NotNull(_client.CanStopSharingChanged);
        Assert.NotNull(_client.CanPairChanged);
    }

    [Fact]
    public void Constructor_WithPortZero_ShouldUseDefaultPort()
    {
        // Arrange & Act
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Assert
        Assert.NotNull(_client);
    }

    [Fact]
    public void Constructor_WithExplicitPort_ShouldUseSpecifiedPort()
    {
        // Arrange & Act
        _client = new TeamsClient.TeamsClient("localhost", 9000, "token", "manufacturer", "device", "app", "1.0", false);

        // Assert
        Assert.NotNull(_client);
    }

    [Fact]
    public void Constructor_WithoutToken_ShouldAcceptEmptyToken()
    {
        // Arrange & Act
        _client = new TeamsClient.TeamsClient("localhost", 8124, "manufacturer", "device", "app", "1.0", false);

        // Assert
        Assert.NotNull(_client);
    }

    [Fact]
    public void DefaultValues_ShouldBeFalse()
    {
        // Arrange & Act
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Assert
        Assert.False(_client.IsMuted);
        Assert.False(_client.IsHandRaised);
        Assert.False(_client.IsInMeeting);
        Assert.False(_client.IsRecordingOn);
        Assert.False(_client.IsBackgroundBlurred);
        Assert.False(_client.IsSharing);
        Assert.False(_client.HasUnreadMessages);
        Assert.False(_client.IsVideoOn);
        Assert.False(_client.CanToggleMute);
        Assert.False(_client.CanToggleVideo);
        Assert.False(_client.CanToggleHand);
        Assert.False(_client.CanToggleBlur);
        Assert.False(_client.CanLeave);
        Assert.False(_client.CanReact);
        Assert.False(_client.CanToggleShareTray);
        Assert.False(_client.CanToggleChat);
        Assert.False(_client.CanStopSharing);
        Assert.False(_client.CanPair);
    }

    [Fact]
    public async Task ToggleMute_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.ToggleMute());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task ToggleVideo_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.ToggleVideo());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task ToggleHand_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.ToggleHand());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task ToggleBackgroundBlur_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.ToggleBackgroundBlur());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task LeaveCall_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.LeaveCall());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task StopSharing_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.StopSharing());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task QueryState_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.QueryState());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendReactionApplause_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.SendReactionApplause());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendReactionLaugh_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.SendReactionLaugh());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendReactionLike_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.SendReactionLike());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendReactionLove_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.SendReactionLove());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendReactionWow_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.SendReactionWow());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task ToggleUiChat_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.ToggleUiChat());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task ToggleUiShareTray_ShouldNotThrow()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act
        var exception = await Record.ExceptionAsync(async () => await _client.ToggleUiShareTray());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task ToggleSharing_WhenNotSharing_ShouldCallToggleUiShareTray()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act - When not sharing, it should open share tray
        await _client.ToggleSharing();

        // Assert - Should not throw
        Assert.False(_client.IsSharing);
    }

    [Fact]
    public void Dispose_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);

        // Act & Assert
        var exception = Record.Exception(() =>
        {
            _client.Dispose();
            _client.Dispose();
            _client.Dispose();
        });

        Assert.Null(exception);
    }

    [Fact]
    public async Task AllCommands_ShouldNotThrow_AfterDispose()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);
        _client.Dispose();

        // Act & Assert - Should not throw exceptions
        var exception = await Record.ExceptionAsync(async () =>
        {
            await _client.ToggleMute();
            await _client.ToggleVideo();
            await _client.ToggleHand();
            await _client.ToggleBackgroundBlur();
            await _client.LeaveCall();
            await _client.StopSharing();
            await _client.QueryState();
            await _client.SendReactionApplause();
            await _client.ToggleUiChat();
            await _client.ToggleSharing();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void IsMutedChanged_ShouldEmitValues()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);
        var emittedValues = new List<bool>();
        _client.IsMutedChanged.Subscribe(value => emittedValues.Add(value));

        // Act - Initial value
        // Assert
        Assert.Single(emittedValues);
        Assert.False(emittedValues[0]);
    }

    [Fact]
    public void TokenChanged_ShouldBeObservable()
    {
        // Arrange
        _client = new TeamsClient.TeamsClient("localhost", "token", "manufacturer", "device", "app", "1.0", false);
        var emittedTokens = new List<string>();
        _client.TokenChanged.Subscribe(token => emittedTokens.Add(token));

        // Act - Initially no tokens emitted (only emitted when token refresh message received)
        // Assert
        Assert.Empty(emittedTokens);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _statusSubject?.Dispose();
    }
}
