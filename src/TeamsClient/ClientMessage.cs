using System.Text.Json.Serialization;

namespace Teams.ThirdPartyAppApi.TeamsClient;

internal class ClientMessage(string action, string? parameters)
{
    public string Action { get; private set; } = action;
    public Param Parameters { get; private set; } = new(parameters);
    public int RequestId { get; set; }

    public static ClientMessage ToggleMute = new("toggle-mute", null);
    public static ClientMessage ToggleVideo = new("toggle-video", null);
    public static ClientMessage ToggleHand = new("toggle-hand", null);
    public static ClientMessage ToggleBackgroundBlur = new("toggle-background-blur", null);
    public static ClientMessage LeaveCall = new("leave-call", null);
    public static ClientMessage StopSharing = new("stop-sharing", null);
    public static ClientMessage QueryState = new("query-state", null);
    public static ClientMessage SendReactionApplause = new("send-reaction", "applause");
    public static ClientMessage SendReactionLaugh = new("send-reaction", "laugh");
    public static ClientMessage SendReactionLike = new("send-reaction", "like");
    public static ClientMessage SendReactionLove = new("send-reaction", "love");
    public static ClientMessage SendReactionWow = new("send-reaction", "wow");
    public static ClientMessage ToggleUiChat = new("toggle-ui", "chat");
    public static ClientMessage ToggleUiShareTray = new("toggle-ui", "share-tray");
}

public class Param(string? type)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; } = type;
};
