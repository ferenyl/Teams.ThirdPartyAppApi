using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Teams.ThirdPartyAppApi.NewTeams;

public class NewTeamsClient : TeamsClientBase
{
    private readonly Subject<string> _tokenChanged = new();
    private readonly BehaviorSubject<bool> _isMutedChanged = new(false);
    private readonly BehaviorSubject<bool> _isHandRaisedChanged = new(false);
    private readonly BehaviorSubject<bool> _isInMeetingChanged = new(false);
    private readonly BehaviorSubject<bool> _isRecordingOnChanged = new(false);
    private readonly BehaviorSubject<bool> _isBackgroundBlurredChanged = new(false);
    private readonly BehaviorSubject<bool> _isSharingChanged = new(false);
    private readonly BehaviorSubject<bool> _hasUnreadMessagesChanged = new(false);
    private readonly BehaviorSubject<bool> _isVideoOnChanged = new(false);
    private readonly BehaviorSubject<bool> _canToggleMuteChanged = new(false);
    private readonly BehaviorSubject<bool> _canToggleVideoChanged = new(false);
    private readonly BehaviorSubject<bool> _canToggleHandChanged = new(false);
    private readonly BehaviorSubject<bool> _canToggleBlurChanged = new(false);
    private readonly BehaviorSubject<bool> _canLeaveChanged = new(false);
    private readonly BehaviorSubject<bool> _canReactChanged = new(false);
    private readonly BehaviorSubject<bool> _canToggleShareTrayChanged = new(false);
    private readonly BehaviorSubject<bool> _canToggleChatChanged = new(false);
    private readonly BehaviorSubject<bool> _canStopSharingChanged = new(false);
    private readonly BehaviorSubject<bool> _canPairChanged = new(false);
    private int RequestId { get; set; }
    private readonly string _manufacturer;
    private readonly string _device;
    private readonly string _app;
    private readonly string _appVersion;

    public NewTeamsClient(string url, string manufacturer, string device, string app, string appVersion, bool autoReconnect = true, CancellationToken cancellationToken = default) : base(url, string.Empty, autoReconnect, cancellationToken)
    {
        _manufacturer = manufacturer;
        _device = device;
        _app = app;
        _appVersion = appVersion;

        IsConnectedChanged
             .Select(IsConnected => IsConnected ? Observable.Empty<Unit>() : Observable.FromAsync(async () => await QueryState()))
             .Concat()
             .Subscribe();

        ReceivedMessages
            .Subscribe(OnReceived);
    }

    public IObservable<string> TokenChanged => _tokenChanged;
    public IObservable<bool> IsMutedChanged => _isMutedChanged;
    public IObservable<bool> IsHandRaisedChanged => _isHandRaisedChanged;
    public IObservable<bool> IsInMeetingChanged => _isInMeetingChanged;
    public IObservable<bool> IsRecordingOnChanged => _isRecordingOnChanged;
    public IObservable<bool> IsBackgroundBlurredChanged => _isBackgroundBlurredChanged;
    public IObservable<bool> IsSharingChanged => _isSharingChanged;
    public IObservable<bool> HasUnreadMessagesChanged => _hasUnreadMessagesChanged;
    public IObservable<bool> IsVideoOnChanged => _isVideoOnChanged;

    public IObservable<bool> CanToggleMuteChanged => _canToggleMuteChanged;
    public IObservable<bool> CanToggleVideoChanged => _canToggleVideoChanged;
    public IObservable<bool> CanToggleHandChanged => _canToggleHandChanged;
    public IObservable<bool> CanToggleBlurChanged => _canToggleBlurChanged;
    public IObservable<bool> CanLeaveChanged => _canLeaveChanged;
    public IObservable<bool> CanReactChanged => _canReactChanged;
    public IObservable<bool> CanToggleShareTrayChanged => _canToggleShareTrayChanged;
    public IObservable<bool> CanToggleChatChanged => _canToggleChatChanged;
    public IObservable<bool> CanStopSharingChanged => _canStopSharingChanged;
    public IObservable<bool> CanPairChanged => _canPairChanged;

    protected override Uri BuildUri()
    {
        var urlencoder = UrlEncoder.Default;
        var tokenString = string.IsNullOrEmpty(Token) ? "" : $"token={Token}&";
        return new($"ws://{Url}:8124?{tokenString}protocol-version=2.0.0&manufacturer={urlencoder.Encode(_manufacturer)}&device={urlencoder.Encode(_device)}&app={urlencoder.Encode(_app)}&app-version={urlencoder.Encode(_appVersion)}");
    }


    private async Task SendCommand(ClientMessage clientMessage)
    {
        clientMessage.RequestId = ++RequestId;

        var message = JsonSerializer.Serialize(clientMessage, _serializerOptions);

        await SendCommand(message);
    }

    protected void OnReceived(string json)
    {
        var message = JsonSerializer.Deserialize<ServerMessage>(json, _serializerOptions) ?? throw new InvalidDataException();

        if (!string.IsNullOrEmpty(message.ErrorMsg))
        {
            if (message.ErrorMsg.EndsWith("no active call"))
                return;
        }

        if (!string.IsNullOrEmpty(message.TokenRefresh))
        {
            _tokenChanged.OnNext(message.TokenRefresh);
            Token = message.TokenRefresh;
            return;
        }


        if (message.MeetingUpdate is null)
            return;

        // set values
        _isMutedChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsMuted);
        _isHandRaisedChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsHandRaised);
        _isInMeetingChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsInMeeting);
        _isRecordingOnChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsRecordingOn);
        _isBackgroundBlurredChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsBackgroundBlurred);
        _isSharingChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsSharing);
        _hasUnreadMessagesChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.HasUnreadMessages);
        _isVideoOnChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingState.IsVideoOn);

        _canToggleMuteChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleMute);
        _canToggleVideoChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleVideo);
        _canToggleHandChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleHand);
        _canToggleBlurChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleBlur);
        _canLeaveChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanLeave);
        _canReactChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanReact);
        _canToggleShareTrayChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleShareTray);
        _canToggleChatChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanToggleChat);
        _canStopSharingChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanStopSharing);
        _canPairChanged.OnNextIfValueChanged(message.MeetingUpdate.MeetingPermissions.CanPair);
    }

    public async Task ToggleMute() =>
        await SendCommand(ClientMessage.ToggleMute);
    public async Task ToggleVideo() =>
        await SendCommand(ClientMessage.ToggleVideo);
    public async Task ToggleHand() =>
        await SendCommand(ClientMessage.ToggleHand);
    public async Task ToggleBackgroundBlur() =>
        await SendCommand(ClientMessage.ToggleBackgroundBlur);
    public async Task LeaveCall() =>
        await SendCommand(ClientMessage.LeaveCall);
    public async Task StopSharing() =>
        await SendCommand(ClientMessage.StopSharing);
    public async Task QueryState() =>
        await SendCommand(ClientMessage.QueryState);
    public async Task SendReactionApplause() =>
        await SendCommand(ClientMessage.SendReactionApplause);
    public async Task SendReactionLaugh() =>
        await SendCommand(ClientMessage.SendReactionLaugh);
    public async Task SendReactionLike() =>
        await SendCommand(ClientMessage.SendReactionLike);
    public async Task SendReactionLove() =>
        await SendCommand(ClientMessage.SendReactionLove);
    public async Task SendReactionWow() =>
        await SendCommand(ClientMessage.SendReactionWow);
    public async Task ToggleUiChat() =>
        await SendCommand(ClientMessage.ToggleUiChat);
    public async Task ToggleUiShareTray() =>
        await SendCommand(ClientMessage.ToggleUiShareTray);
    public async Task ToggleSharing()
    {
        if (_isSharingChanged.Value)
        {
            await StopSharing();
        }
        else
        {
            await ToggleUiShareTray();
        }
    }
}
