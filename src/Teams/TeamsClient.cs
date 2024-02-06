using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;

namespace Teams.ThirdPartyAppApi.Teams;

public class TeamsClient : TeamsClientBase
{
    private readonly BehaviorSubject<bool> _whenIsMutedChanged = new(false);
    private readonly BehaviorSubject<bool> _whenIsCameraOnChanged = new(false);
    private readonly BehaviorSubject<bool> _whenIsHandRaisedChanged = new(false);
    private readonly BehaviorSubject<bool> _whenIsInMeetingChanged = new(false);
    private readonly BehaviorSubject<bool> _whenIsRecordingOnChanged = new(false);
    private readonly BehaviorSubject<bool> _whenIsBackgroundBlurredChanged = new(false);
    private readonly BehaviorSubject<bool> _whenCanToggleMuteChanged = new(false);
    private readonly BehaviorSubject<bool> _whenCanToggleVideoChanged = new(false);
    private readonly BehaviorSubject<bool> _whenCanToggleHandChanged = new(false);
    private readonly BehaviorSubject<bool> _whenCanToggleBlurChanged = new(false);
    private readonly BehaviorSubject<bool> _whenCanToggleRecordChanged = new(false);
    private readonly BehaviorSubject<bool> _whenCanLeaveChanged = new(false);
    private readonly BehaviorSubject<bool> _whenCanReactChanged = new(false);



    public string Manufacturer { get; set; } = "Elgato";
    public string Device { get; set; } = "Stream Deck";

    public TeamsClient(string url, string port, string token = "", bool autoReconnect = false, CancellationToken cancellationToken = default) : base(url, port, token, autoReconnect, new ClientInformation(), cancellationToken)
    {

        IsConnectedChanged
             .Select(IsConnected => !IsConnected
                ? OnNotConnected()
                : Observable.FromAsync(async () => await SendCommand(MeetingAction.QueryMeetingState)))
             .Concat()
             .Subscribe();

        ReceivedMessages
            .Subscribe(OnReceived);

    }

    public TeamsClient(string url, string token = "", bool autoReconnect = false, CancellationToken cancellationToken = default) : this(url, string.Empty, token, autoReconnect, cancellationToken)
    {
    }

    public IObservable<bool> IsMutedChanged => _whenIsMutedChanged;
    public IObservable<bool> IsCameraOnChanged => _whenIsCameraOnChanged;
    public IObservable<bool> IsHandRaisedChanged => _whenIsHandRaisedChanged;
    public IObservable<bool> IsInMeetingChanged => _whenIsInMeetingChanged;
    public IObservable<bool> IsRecordingOnChanged => _whenIsRecordingOnChanged;
    public IObservable<bool> IsBackgroundBlurredChanged => _whenIsBackgroundBlurredChanged;
    public IObservable<bool> CanToggleMuteChanged => _whenCanToggleMuteChanged;
    public IObservable<bool> CanToggleVideoChanged => _whenCanToggleVideoChanged;
    public IObservable<bool> CanToggleHandChanged => _whenCanToggleHandChanged;

    public IObservable<bool> CanToggleBlurChanged => _whenCanToggleBlurChanged;
    public IObservable<bool> CanToggleRecordChanged => _whenCanToggleRecordChanged;
    public IObservable<bool> CanLeaveChanged => _whenCanLeaveChanged;
    public IObservable<bool> CanReactChanged => _whenCanReactChanged;

    public bool IsMuted => _whenIsMutedChanged.Value;
    public bool IsCameraOn => _whenIsCameraOnChanged.Value;
    public bool IsHandRaised => _whenIsHandRaisedChanged.Value;
    public bool IsInMeeting => _whenIsInMeetingChanged.Value;
    public bool IsRecordingOn => _whenIsRecordingOnChanged.Value;
    public bool IsBackgroundBlurred => _whenIsBackgroundBlurredChanged.Value;
    public bool CanToggleMute => _whenCanToggleMuteChanged.Value;
    public bool CanToggleVideo => _whenCanToggleVideoChanged.Value;
    public bool CanToggleHand => _whenCanToggleHandChanged.Value;
    public bool CanToggleBlur => _whenCanToggleBlurChanged.Value;
    public bool CanToggleRecord => _whenCanToggleRecordChanged.Value;
    public bool CanLeave => _whenCanLeaveChanged.Value;
    public bool CanReact => _whenCanReactChanged.Value;


    protected override Uri BuildUri()
    {
        return new($"ws://{Url}:{Port}?token={Token}");
    }

    protected void OnReceived(string json)
    {
        if (!TryDeserialize<ServerMessage>(json, out var message) || message is null)
            return;

        if (!string.IsNullOrEmpty(message.ErrorMsg))
        {
            if (message.ErrorMsg.EndsWith("no active call"))
                return;

            throw new Exception(message.ErrorMsg);
        }

        if (message.MeetingUpdate is null)
            return;

        _whenIsMutedChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsMuted);
        _whenIsCameraOnChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsCameraOn);
        _whenIsHandRaisedChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsHandRaised);
        _whenIsInMeetingChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsInMeeting);
        _whenIsRecordingOnChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsRecordingOn);
        _whenIsBackgroundBlurredChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsBackgroundBlurred);
        _whenCanToggleMuteChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleMute);
        _whenCanToggleVideoChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleVideo);
        _whenCanToggleHandChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleHand);
        _whenCanToggleBlurChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleBlur);
        _whenCanToggleRecordChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleRecord);
        _whenCanLeaveChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanLeave);
        _whenCanReactChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanReact);
    }

    private IObservable<Unit> OnNotConnected()
    {
        _whenIsMutedChanged.OnNextIfValueChanged(false);
        _whenIsCameraOnChanged.OnNextIfValueChanged(false);
        _whenIsHandRaisedChanged.OnNextIfValueChanged(false);
        _whenIsInMeetingChanged.OnNextIfValueChanged(false);
        _whenIsRecordingOnChanged.OnNextIfValueChanged(false);
        _whenIsBackgroundBlurredChanged.OnNextIfValueChanged(false);
        _whenCanToggleMuteChanged.OnNextIfValueChanged(false);
        _whenCanToggleVideoChanged.OnNextIfValueChanged(false);
        _whenCanToggleHandChanged.OnNextIfValueChanged(false);
        _whenCanToggleBlurChanged.OnNextIfValueChanged(false);
        _whenCanToggleRecordChanged.OnNextIfValueChanged(false);
        _whenCanLeaveChanged.OnNextIfValueChanged(false);
        _whenCanReactChanged.OnNextIfValueChanged(false);

        return Observable.Empty<Unit>();
    }

    private async Task SendCommand(MeetingAction action)
    {

        var message = JsonSerializer.Serialize(new ClientMessage()
        {
            Action = action,
            Device = Device,
            Manufacturer = Manufacturer,
        }, _serializerOptions);

        await SendCommand(message);
    }

    public async Task LeaveCall()
        => await SendCommand(MeetingAction.LeaveCall);

    public async Task ReactApplause()
        => await SendCommand(MeetingAction.ReactApplause);

    public async Task ReactLaugh()
        => await SendCommand(MeetingAction.ReactLaugh);

    public async Task ReactLike()
        => await SendCommand(MeetingAction.ReactLike);

    public async Task ReactLove()
        => await SendCommand(MeetingAction.ReactLove);

    public async Task ReactWow()
        => await SendCommand(MeetingAction.ReactWow);

    public async Task UpdateState()
        => await SendCommand(MeetingAction.QueryMeetingState);

    public async Task ToggleMute()
        => await SendCommand(MeetingAction.ToggleMute);

    public async Task Mute()
        => await SendCommand(MeetingAction.Mute);

    public async Task UnMute()
        => await SendCommand(MeetingAction.Unmute);

    public async Task ToggleVideo() =>
        await SendCommand(MeetingAction.ToggleVideo);

    public async Task ShowVideo() =>
        await SendCommand(MeetingAction.ShowVideo);

    public async Task HideVideo() =>
        await SendCommand(MeetingAction.HideVideo);

    public async Task ToggleHand() =>
        await SendCommand(MeetingAction.ToggleHand);

    public async Task RaiseHand() =>
        await SendCommand(MeetingAction.RaiseHand);

    public async Task LowerHand() =>
        await SendCommand(MeetingAction.LowerHand);

    public async Task ToggleRecordings() =>
        await SendCommand(MeetingAction.ToggleRecording);

    public async Task StartRecording() =>
        await SendCommand(MeetingAction.StartRecording);

    public async Task StopRecording() =>
        await SendCommand(MeetingAction.StartRecording);

    public async Task ToggleBackgroundBlur() =>
        await SendCommand(_whenIsBackgroundBlurredChanged.Value ? MeetingAction.UnblurBackground : MeetingAction.BlurBackground);

    public async Task BlurBackground() =>
        await SendCommand(MeetingAction.BlurBackground);

    public async Task UnblurBackground() =>
        await SendCommand(MeetingAction.UnblurBackground);
}