using System.ComponentModel;
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



    public TeamsClient(string url, string token = "", bool autoReconnect = false, CancellationToken cancellationToken = default) : base(url, token, autoReconnect, cancellationToken)
    {

        IsConnectedChanged
             .Select(IsConnected => IsConnected 
                ? Observable.Empty<Unit>() 
                : Observable.FromAsync(async () =>  await SendCommand(MeetingAction.QueryMeetingState)))
             .Concat()
             .Subscribe();

        ReceivedMessages
            .Subscribe(OnReceived);
        
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
        return new($"ws://{Url}:8124?token={Token}");
    }

    protected void OnReceived(string json)
    {
        var message = JsonSerializer.Deserialize<ServerMessage>(json, _serializerOptions) ?? throw new InvalidDataException();

        if (!string.IsNullOrEmpty(message.ErrorMsg))
        {
            if (message.ErrorMsg.EndsWith("no active call"))
                return;

            throw new Exception(message.ErrorMsg);
        }

        if (message.MeetingUpdate is null)
            return;

        _isMutedChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsMuted);
        _isCameraOnChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsCameraOn);
        _isHandRaisedChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsHandRaised);
        _isInMeetingChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsInMeeting);
        _isRecordingOnChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsRecordingOn);
        _isBackgroundBlurredChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsBackgroundBlurred);
        _canToggleMuteChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleMute);
        _canToggleVideoChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleVideo);
        _canToggleHandChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleHand);
        _canToggleBlurChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleBlur);
        _canToggleRecordChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleRecord);
        _canLeaveChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanLeave);
        _canReactChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanReact);
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

    public async Task ToggleBackgroundBlur()
    {
        if (_isBackgroundBlurredChanged.Value)
        {
            await SendCommand(MeetingAction.UnblurBackground);
        }
        else
        {
            await SendCommand(MeetingAction.BlurBackground);
        }
    }

    public async Task BlurBackground() => 
        await SendCommand(MeetingAction.BlurBackground);

    public async Task UnblurBackground() => 
        await SendCommand(MeetingAction.UnblurBackground);
}