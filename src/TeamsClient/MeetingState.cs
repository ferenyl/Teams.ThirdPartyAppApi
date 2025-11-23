namespace Teams.ThirdPartyAppApi.TeamsClient;

public class MeetingStateSnapshot
{
    public bool IsMuted { get; init; }
    public bool IsHandRaised { get; init; }
    public bool IsInMeeting { get; init; }
    public bool IsRecordingOn { get; init; }
    public bool IsBackgroundBlurred { get; init; }
    public bool IsSharing { get; init; }
    public bool HasUnreadMessages { get; init; }
    public bool IsVideoOn { get; init; }
    public bool CanToggleMute { get; init; }
    public bool CanToggleVideo { get; init; }
    public bool CanToggleHand { get; init; }
    public bool CanToggleBlur { get; init; }
    public bool CanLeave { get; init; }
    public bool CanReact { get; init; }
    public bool CanToggleShareTray { get; init; }
    public bool CanToggleChat { get; init; }
    public bool CanStopSharing { get; init; }
    public bool CanPair { get; init; }

    public static MeetingStateSnapshot Default => new();

    public MeetingStateSnapshot()
    {
    }

    public MeetingStateSnapshot With(
        bool? isMuted = null,
        bool? isHandRaised = null,
        bool? isInMeeting = null,
        bool? isRecordingOn = null,
        bool? isBackgroundBlurred = null,
        bool? isSharing = null,
        bool? hasUnreadMessages = null,
        bool? isVideoOn = null,
        bool? canToggleMute = null,
        bool? canToggleVideo = null,
        bool? canToggleHand = null,
        bool? canToggleBlur = null,
        bool? canLeave = null,
        bool? canReact = null,
        bool? canToggleShareTray = null,
        bool? canToggleChat = null,
        bool? canStopSharing = null,
        bool? canPair = null)
    {
        return new MeetingStateSnapshot
        {
            IsMuted = isMuted ?? IsMuted,
            IsHandRaised = isHandRaised ?? IsHandRaised,
            IsInMeeting = isInMeeting ?? IsInMeeting,
            IsRecordingOn = isRecordingOn ?? IsRecordingOn,
            IsBackgroundBlurred = isBackgroundBlurred ?? IsBackgroundBlurred,
            IsSharing = isSharing ?? IsSharing,
            HasUnreadMessages = hasUnreadMessages ?? HasUnreadMessages,
            IsVideoOn = isVideoOn ?? IsVideoOn,
            CanToggleMute = canToggleMute ?? CanToggleMute,
            CanToggleVideo = canToggleVideo ?? CanToggleVideo,
            CanToggleHand = canToggleHand ?? CanToggleHand,
            CanToggleBlur = canToggleBlur ?? CanToggleBlur,
            CanLeave = canLeave ?? CanLeave,
            CanReact = canReact ?? CanReact,
            CanToggleShareTray = canToggleShareTray ?? CanToggleShareTray,
            CanToggleChat = canToggleChat ?? CanToggleChat,
            CanStopSharing = canStopSharing ?? CanStopSharing,
            CanPair = canPair ?? CanPair
        };
    }

    public override bool Equals(object? obj)
    {
        if (obj is not MeetingStateSnapshot other) return false;
        
        return IsMuted == other.IsMuted &&
               IsHandRaised == other.IsHandRaised &&
               IsInMeeting == other.IsInMeeting &&
               IsRecordingOn == other.IsRecordingOn &&
               IsBackgroundBlurred == other.IsBackgroundBlurred &&
               IsSharing == other.IsSharing &&
               HasUnreadMessages == other.HasUnreadMessages &&
               IsVideoOn == other.IsVideoOn &&
               CanToggleMute == other.CanToggleMute &&
               CanToggleVideo == other.CanToggleVideo &&
               CanToggleHand == other.CanToggleHand &&
               CanToggleBlur == other.CanToggleBlur &&
               CanLeave == other.CanLeave &&
               CanReact == other.CanReact &&
               CanToggleShareTray == other.CanToggleShareTray &&
               CanToggleChat == other.CanToggleChat &&
               CanStopSharing == other.CanStopSharing &&
               CanPair == other.CanPair;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(IsMuted);
        hash.Add(IsHandRaised);
        hash.Add(IsInMeeting);
        hash.Add(IsRecordingOn);
        hash.Add(IsBackgroundBlurred);
        hash.Add(IsSharing);
        hash.Add(HasUnreadMessages);
        hash.Add(IsVideoOn);
        hash.Add(CanToggleMute);
        hash.Add(CanToggleVideo);
        hash.Add(CanToggleHand);
        hash.Add(CanToggleBlur);
        hash.Add(CanLeave);
        hash.Add(CanReact);
        hash.Add(CanToggleShareTray);
        hash.Add(CanToggleChat);
        hash.Add(CanStopSharing);
        hash.Add(CanPair);
        return hash.ToHashCode();
    }
}
