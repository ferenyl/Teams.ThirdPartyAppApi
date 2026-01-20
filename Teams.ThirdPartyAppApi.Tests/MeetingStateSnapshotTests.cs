using Teams.ThirdPartyAppApi.TeamsClient;

namespace Teams.ThirdPartyAppApi.Tests;

public class MeetingStateSnapshotTests
{
    [Fact]
    public void Default_ShouldReturnStateWithAllFalseValues()
    {
        // Act
        var state = MeetingStateSnapshot.Default;

        // Assert
        Assert.False(state.IsMuted);
        Assert.False(state.IsHandRaised);
        Assert.False(state.IsInMeeting);
        Assert.False(state.IsRecordingOn);
        Assert.False(state.IsBackgroundBlurred);
        Assert.False(state.IsSharing);
        Assert.False(state.HasUnreadMessages);
        Assert.False(state.IsVideoOn);
        Assert.False(state.CanToggleMute);
        Assert.False(state.CanToggleVideo);
        Assert.False(state.CanToggleHand);
        Assert.False(state.CanToggleBlur);
        Assert.False(state.CanLeave);
        Assert.False(state.CanReact);
        Assert.False(state.CanToggleShareTray);
        Assert.False(state.CanToggleChat);
        Assert.False(state.CanStopSharing);
        Assert.False(state.CanPair);
    }

    [Fact]
    public void With_ShouldUpdateOnlySpecifiedProperties()
    {
        // Arrange
        var state = MeetingStateSnapshot.Default;

        // Act
        var newState = state.With(isMuted: true, isInMeeting: true);

        // Assert
        Assert.True(newState.IsMuted);
        Assert.True(newState.IsInMeeting);
        Assert.False(newState.IsHandRaised);
        Assert.False(newState.IsVideoOn);
    }

    [Fact]
    public void With_ShouldKeepUnchangedProperties()
    {
        // Arrange
        var state = new MeetingStateSnapshot
        {
            IsMuted = true,
            IsHandRaised = true,
            IsInMeeting = true,
            CanToggleMute = true
        };

        // Act
        var newState = state.With(isVideoOn: true);

        // Assert
        Assert.True(newState.IsMuted);
        Assert.True(newState.IsHandRaised);
        Assert.True(newState.IsInMeeting);
        Assert.True(newState.CanToggleMute);
        Assert.True(newState.IsVideoOn);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenAllPropertiesMatch()
    {
        // Arrange
        var state1 = new MeetingStateSnapshot
        {
            IsMuted = true,
            IsHandRaised = false,
            IsInMeeting = true
        };

        var state2 = new MeetingStateSnapshot
        {
            IsMuted = true,
            IsHandRaised = false,
            IsInMeeting = true
        };

        // Act & Assert
        Assert.True(state1.Equals(state2));
        Assert.True(state2.Equals(state1));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenAnyPropertyDiffers()
    {
        // Arrange
        var state1 = new MeetingStateSnapshot
        {
            IsMuted = true,
            IsHandRaised = false
        };

        var state2 = new MeetingStateSnapshot
        {
            IsMuted = true,
            IsHandRaised = true
        };

        // Act & Assert
        Assert.False(state1.Equals(state2));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparedWithNull()
    {
        // Arrange
        var state = MeetingStateSnapshot.Default;

        // Act & Assert
        Assert.False(object.Equals(state, null));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparedWithDifferentType()
    {
        // Arrange
        var state = MeetingStateSnapshot.Default;
        object differentType = "not a state";

        // Act & Assert
        Assert.False(state.Equals(differentType));
    }

    [Fact]
    public void GetHashCode_ShouldBeSame_ForEqualStates()
    {
        // Arrange
        var state1 = new MeetingStateSnapshot
        {
            IsMuted = true,
            IsInMeeting = true,
            CanToggleMute = true
        };

        var state2 = new MeetingStateSnapshot
        {
            IsMuted = true,
            IsInMeeting = true,
            CanToggleMute = true
        };

        // Act & Assert
        Assert.Equal(state1.GetHashCode(), state2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldBeDifferent_ForDifferentStates()
    {
        // Arrange
        var state1 = new MeetingStateSnapshot { IsMuted = true };
        var state2 = new MeetingStateSnapshot { IsMuted = false };

        // Act
        var hash1 = state1.GetHashCode();
        var hash2 = state2.GetHashCode();

        // Assert - Not guaranteed but highly likely to be different
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Constructor_ShouldCreateStateWithSpecifiedValues()
    {
        // Act
        var state = new MeetingStateSnapshot
        {
            IsMuted = true,
            IsHandRaised = true,
            IsInMeeting = true,
            IsRecordingOn = false,
            IsBackgroundBlurred = true,
            IsSharing = false,
            HasUnreadMessages = true,
            IsVideoOn = false,
            CanToggleMute = true,
            CanToggleVideo = true,
            CanToggleHand = false,
            CanToggleBlur = true,
            CanLeave = true,
            CanReact = false,
            CanToggleShareTray = true,
            CanToggleChat = false,
            CanStopSharing = false,
            CanPair = true
        };

        // Assert
        Assert.True(state.IsMuted);
        Assert.True(state.IsHandRaised);
        Assert.True(state.IsInMeeting);
        Assert.False(state.IsRecordingOn);
        Assert.True(state.IsBackgroundBlurred);
        Assert.False(state.IsSharing);
        Assert.True(state.HasUnreadMessages);
        Assert.False(state.IsVideoOn);
        Assert.True(state.CanToggleMute);
        Assert.True(state.CanToggleVideo);
        Assert.False(state.CanToggleHand);
        Assert.True(state.CanToggleBlur);
        Assert.True(state.CanLeave);
        Assert.False(state.CanReact);
        Assert.True(state.CanToggleShareTray);
        Assert.False(state.CanToggleChat);
        Assert.False(state.CanStopSharing);
        Assert.True(state.CanPair);
    }

    [Fact]
    public void With_ShouldCreateNewInstance_NotModifyOriginal()
    {
        // Arrange
        var original = new MeetingStateSnapshot { IsMuted = true };

        // Act
        var modified = original.With(isHandRaised: true);

        // Assert
        Assert.True(original.IsMuted);
        Assert.False(original.IsHandRaised);
        Assert.True(modified.IsMuted);
        Assert.True(modified.IsHandRaised);
        Assert.NotSame(original, modified);
    }
}
