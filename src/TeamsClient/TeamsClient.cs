using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Teams.ThirdPartyAppApi.TeamsClient;

public class TeamsClient : TeamsClientBase, IDisposable
{
    private readonly Subject<string> _tokenChanged = new();
    private readonly BehaviorSubject<MeetingStateSnapshot> _stateChanged = new(MeetingStateSnapshot.Default);
    private readonly IDisposable? _connectedSubscription;
    private readonly IDisposable? _receivedSubscription;
    private bool _disposed;
    private int _requestId;

    // Cached observables to prevent creating new pipelines on each access
    private readonly IObservable<bool> _isMutedChanged;
    private readonly IObservable<bool> _isHandRaisedChanged;
    private readonly IObservable<bool> _isInMeetingChanged;
    private readonly IObservable<bool> _isRecordingOnChanged;
    private readonly IObservable<bool> _isBackgroundBlurredChanged;
    private readonly IObservable<bool> _isSharingChanged;
    private readonly IObservable<bool> _hasUnreadMessagesChanged;
    private readonly IObservable<bool> _isVideoOnChanged;
    private readonly IObservable<bool> _canToggleMuteChanged;
    private readonly IObservable<bool> _canToggleVideoChanged;
    private readonly IObservable<bool> _canToggleHandChanged;
    private readonly IObservable<bool> _canToggleBlurChanged;
    private readonly IObservable<bool> _canLeaveChanged;
    private readonly IObservable<bool> _canReactChanged;
    private readonly IObservable<bool> _canToggleShareTrayChanged;
    private readonly IObservable<bool> _canToggleChatChanged;
    private readonly IObservable<bool> _canStopSharingChanged;
    private readonly IObservable<bool> _canPairChanged;

    public TeamsClient(string url, int port, string token, string manufacturer, string device, string app, string appVersion, bool autoReconnect = true, CancellationToken cancellationToken = default)
    : base(url, port, token, autoReconnect, new ClientInformation(manufacturer, device, app, appVersion), cancellationToken)
    {
        // Initialize cached observables once to prevent memory leaks
        _isMutedChanged = _stateChanged.Select(s => s.IsMuted).DistinctUntilChanged();
        _isHandRaisedChanged = _stateChanged.Select(s => s.IsHandRaised).DistinctUntilChanged();
        _isInMeetingChanged = _stateChanged.Select(s => s.IsInMeeting).DistinctUntilChanged();
        _isRecordingOnChanged = _stateChanged.Select(s => s.IsRecordingOn).DistinctUntilChanged();
        _isBackgroundBlurredChanged = _stateChanged.Select(s => s.IsBackgroundBlurred).DistinctUntilChanged();
        _isSharingChanged = _stateChanged.Select(s => s.IsSharing).DistinctUntilChanged();
        _hasUnreadMessagesChanged = _stateChanged.Select(s => s.HasUnreadMessages).DistinctUntilChanged();
        _isVideoOnChanged = _stateChanged.Select(s => s.IsVideoOn).DistinctUntilChanged();
        _canToggleMuteChanged = _stateChanged.Select(s => s.CanToggleMute).DistinctUntilChanged();
        _canToggleVideoChanged = _stateChanged.Select(s => s.CanToggleVideo).DistinctUntilChanged();
        _canToggleHandChanged = _stateChanged.Select(s => s.CanToggleHand).DistinctUntilChanged();
        _canToggleBlurChanged = _stateChanged.Select(s => s.CanToggleBlur).DistinctUntilChanged();
        _canLeaveChanged = _stateChanged.Select(s => s.CanLeave).DistinctUntilChanged();
        _canReactChanged = _stateChanged.Select(s => s.CanReact).DistinctUntilChanged();
        _canToggleShareTrayChanged = _stateChanged.Select(s => s.CanToggleShareTray).DistinctUntilChanged();
        _canToggleChatChanged = _stateChanged.Select(s => s.CanToggleChat).DistinctUntilChanged();
        _canStopSharingChanged = _stateChanged.Select(s => s.CanStopSharing).DistinctUntilChanged();
        _canPairChanged = _stateChanged.Select(s => s.CanPair).DistinctUntilChanged();

        _connectedSubscription = IsConnectedChanged
             .Select(IsConnected => IsConnected ? Observable.FromAsync(async () => await QueryState()) : OnNotConnected())
             .Concat()
             .Subscribe();

        _receivedSubscription = ReceivedMessages
            .Subscribe(OnReceived);
    }

    public TeamsClient(string url, string token, string manufacturer, string device, string app, string appVersion, bool autoReconnect = true, CancellationToken cancellationToken = default)
    : this(url: url, port: 0, token, manufacturer: manufacturer, device: device, app: app, appVersion: appVersion, autoReconnect: autoReconnect, cancellationToken: cancellationToken)
    {
    }

    public TeamsClient(string url, int port, string manufacturer, string device, string app, string appVersion, bool autoReconnect = true, CancellationToken cancellationToken = default)
   : this(url: url, port: port, string.Empty, manufacturer: manufacturer, device: device, app: app, appVersion: appVersion, autoReconnect: autoReconnect, cancellationToken: cancellationToken)
    {
    }



    public IObservable<string> TokenChanged => _tokenChanged;
    public IObservable<MeetingStateSnapshot> StateChanged => _stateChanged;
    
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

    public bool IsMuted => _stateChanged.Value.IsMuted;
    public bool IsHandRaised => _stateChanged.Value.IsHandRaised;
    public bool IsInMeeting => _stateChanged.Value.IsInMeeting;
    public bool IsRecordingOn => _stateChanged.Value.IsRecordingOn;
    public bool IsBackgroundBlurred => _stateChanged.Value.IsBackgroundBlurred;
    public bool IsSharing => _stateChanged.Value.IsSharing;
    public bool HasUnreadMessages => _stateChanged.Value.HasUnreadMessages;
    public bool IsVideoOn => _stateChanged.Value.IsVideoOn;
    public bool CanToggleMute => _stateChanged.Value.CanToggleMute;
    public bool CanToggleVideo => _stateChanged.Value.CanToggleVideo;
    public bool CanToggleHand => _stateChanged.Value.CanToggleHand;
    public bool CanToggleBlur => _stateChanged.Value.CanToggleBlur;
    public bool CanLeave => _stateChanged.Value.CanLeave;
    public bool CanReact => _stateChanged.Value.CanReact;
    public bool CanToggleShareTray => _stateChanged.Value.CanToggleShareTray;
    public bool CanToggleChat => _stateChanged.Value.CanToggleChat;
    public bool CanStopSharing => _stateChanged.Value.CanStopSharing;
    public bool CanPair => _stateChanged.Value.CanPair;

    protected override Uri BuildUri()
    {
        var urlencoder = UrlEncoder.Default;
        var tokenString = string.IsNullOrEmpty(Token) ? "" : $"token={Token}&";
        return new($"ws://{Url}:{Port}?{tokenString}protocol-version=2.0.0&manufacturer={urlencoder.Encode(_clientInformation.Manufacturer)}&device={urlencoder.Encode(_clientInformation.Device)}&app={urlencoder.Encode(_clientInformation.App)}&app-version={urlencoder.Encode(_clientInformation.AppVersion)}");
    }


    private async Task SendCommand(ClientMessage clientMessage)
    {
        if (_disposed) return;
        
        try
        {
            clientMessage.RequestId = Interlocked.Increment(ref _requestId);
            var message = JsonSerializer.Serialize(clientMessage, _serializerOptions);
            await SendCommand(message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"SendCommand error: {ex}");
        }
    }

    protected void OnReceived(string json)
    {
        if (_disposed) return;
        
        if (!TryDeserialize<ServerMessage>(json, out var message) || message is null)
            return;

        if (!string.IsNullOrEmpty(message.ErrorMsg))
        {
            Console.Error.WriteLine($"Server error: {message.ErrorMsg}");
            if (message.ErrorMsg.EndsWith("no active call"))
                return;
        }

        if (!string.IsNullOrEmpty(message.TokenRefresh))
        {
            _tokenChanged.OnNext(message.TokenRefresh);
            Token = message.TokenRefresh;
            return;
        }

        if (message.Response == "Success")
        {
            return;
        }

        if (message.MeetingUpdate is null)
            return;

        var newState = new MeetingStateSnapshot
        {
            IsMuted = message.MeetingUpdate.MeetingState.IsMuted,
            IsHandRaised = message.MeetingUpdate.MeetingState.IsHandRaised,
            IsInMeeting = message.MeetingUpdate.MeetingState.IsInMeeting,
            IsRecordingOn = message.MeetingUpdate.MeetingState.IsRecordingOn,
            IsBackgroundBlurred = message.MeetingUpdate.MeetingState.IsBackgroundBlurred,
            IsSharing = message.MeetingUpdate.MeetingState.IsSharing,
            HasUnreadMessages = message.MeetingUpdate.MeetingState.HasUnreadMessages,
            IsVideoOn = message.MeetingUpdate.MeetingState.IsVideoOn,
            CanToggleMute = message.MeetingUpdate.MeetingPermissions.CanToggleMute,
            CanToggleVideo = message.MeetingUpdate.MeetingPermissions.CanToggleVideo,
            CanToggleHand = message.MeetingUpdate.MeetingPermissions.CanToggleHand,
            CanToggleBlur = message.MeetingUpdate.MeetingPermissions.CanToggleBlur,
            CanLeave = message.MeetingUpdate.MeetingPermissions.CanLeave,
            CanReact = message.MeetingUpdate.MeetingPermissions.CanReact,
            CanToggleShareTray = message.MeetingUpdate.MeetingPermissions.CanToggleShareTray,
            CanToggleChat = message.MeetingUpdate.MeetingPermissions.CanToggleChat,
            CanStopSharing = message.MeetingUpdate.MeetingPermissions.CanStopSharing,
            CanPair = message.MeetingUpdate.MeetingPermissions.CanPair
        };

        if (!newState.Equals(_stateChanged.Value))
        {
            _stateChanged.OnNext(newState);
        }
    }

    private IObservable<Unit> OnNotConnected()
    {
        var defaultState = MeetingStateSnapshot.Default;
        if (!defaultState.Equals(_stateChanged.Value))
        {
            _stateChanged.OnNext(defaultState);
        }

        return Observable.Empty<Unit>();
    }

    public async Task ToggleMute() => await SendCommand(ClientMessage.ToggleMute).ConfigureAwait(false);
    public async Task ToggleVideo() => await SendCommand(ClientMessage.ToggleVideo).ConfigureAwait(false);
    public async Task ToggleHand() => await SendCommand(ClientMessage.ToggleHand).ConfigureAwait(false);
    public async Task ToggleBackgroundBlur() => await SendCommand(ClientMessage.ToggleBackgroundBlur).ConfigureAwait(false);
    public async Task LeaveCall() => await SendCommand(ClientMessage.LeaveCall).ConfigureAwait(false);
    public async Task StopSharing() => await SendCommand(ClientMessage.StopSharing).ConfigureAwait(false);
    public async Task QueryState() => await SendCommand(ClientMessage.QueryState).ConfigureAwait(false);
    public async Task SendReactionApplause() => await SendCommand(ClientMessage.SendReactionApplause).ConfigureAwait(false);
    public async Task SendReactionLaugh() => await SendCommand(ClientMessage.SendReactionLaugh).ConfigureAwait(false);
    public async Task SendReactionLike() => await SendCommand(ClientMessage.SendReactionLike).ConfigureAwait(false);
    public async Task SendReactionLove() => await SendCommand(ClientMessage.SendReactionLove).ConfigureAwait(false);
    public async Task SendReactionWow() => await SendCommand(ClientMessage.SendReactionWow).ConfigureAwait(false);
    public async Task ToggleUiChat() => await SendCommand(ClientMessage.ToggleUiChat).ConfigureAwait(false);
    public async Task ToggleUiShareTray() => await SendCommand(ClientMessage.ToggleUiShareTray).ConfigureAwait(false);
    public async Task ToggleSharing()
    {
        if (_disposed) return;
        
        if (_stateChanged.Value.IsSharing)
        {
            await StopSharing().ConfigureAwait(false);
        }
        else
        {
            await ToggleUiShareTray().ConfigureAwait(false);
        }
    }

    public override void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        _connectedSubscription?.Dispose();
        _receivedSubscription?.Dispose();
        
        _tokenChanged?.Dispose();
        _stateChanged?.Dispose();
        
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}