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

    public TeamsClient(string url, int port, string token, string manufacturer, string device, string app, string appVersion, bool autoReconnect = true, CancellationToken cancellationToken = default)
    : base(url, port, token, autoReconnect, new ClientInformation(manufacturer, device, app, appVersion), cancellationToken)
    {
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
    
    public IObservable<bool> IsMutedChanged => _stateChanged.Select(s => s.IsMuted).DistinctUntilChanged();
    public IObservable<bool> IsHandRaisedChanged => _stateChanged.Select(s => s.IsHandRaised).DistinctUntilChanged();
    public IObservable<bool> IsInMeetingChanged => _stateChanged.Select(s => s.IsInMeeting).DistinctUntilChanged();
    public IObservable<bool> IsRecordingOnChanged => _stateChanged.Select(s => s.IsRecordingOn).DistinctUntilChanged();
    public IObservable<bool> IsBackgroundBlurredChanged => _stateChanged.Select(s => s.IsBackgroundBlurred).DistinctUntilChanged();
    public IObservable<bool> IsSharingChanged => _stateChanged.Select(s => s.IsSharing).DistinctUntilChanged();
    public IObservable<bool> HasUnreadMessagesChanged => _stateChanged.Select(s => s.HasUnreadMessages).DistinctUntilChanged();
    public IObservable<bool> IsVideoOnChanged => _stateChanged.Select(s => s.IsVideoOn).DistinctUntilChanged();

    public IObservable<bool> CanToggleMuteChanged => _stateChanged.Select(s => s.CanToggleMute).DistinctUntilChanged();
    public IObservable<bool> CanToggleVideoChanged => _stateChanged.Select(s => s.CanToggleVideo).DistinctUntilChanged();
    public IObservable<bool> CanToggleHandChanged => _stateChanged.Select(s => s.CanToggleHand).DistinctUntilChanged();
    public IObservable<bool> CanToggleBlurChanged => _stateChanged.Select(s => s.CanToggleBlur).DistinctUntilChanged();
    public IObservable<bool> CanLeaveChanged => _stateChanged.Select(s => s.CanLeave).DistinctUntilChanged();
    public IObservable<bool> CanReactChanged => _stateChanged.Select(s => s.CanReact).DistinctUntilChanged();
    public IObservable<bool> CanToggleShareTrayChanged => _stateChanged.Select(s => s.CanToggleShareTray).DistinctUntilChanged();
    public IObservable<bool> CanToggleChatChanged => _stateChanged.Select(s => s.CanToggleChat).DistinctUntilChanged();
    public IObservable<bool> CanStopSharingChanged => _stateChanged.Select(s => s.CanStopSharing).DistinctUntilChanged();
    public IObservable<bool> CanPairChanged => _stateChanged.Select(s => s.CanPair).DistinctUntilChanged();

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