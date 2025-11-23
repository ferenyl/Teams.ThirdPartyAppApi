# Third-party app API Client

![main](https://github.com/ferenyl/Teams.ThirdPartyAppApi/actions/workflows/dotnet.yml/badge.svg?branch=main) ![NuGet Version](https://img.shields.io/nuget/v/Teams.ThirdPartyAppApi?style=flat)

A .NET library to communicate with the [Microsoft Teams Third-party app API](https://support.microsoft.com/en-us/office/connect-third-party-devices-to-teams-aabca9f2-47bb-407f-9f9b-81a104a883d6), enabling you to control Teams meetings programmatically.

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package Teams.ThirdPartyAppApi
```

Or via Package Manager Console:

```powershell
Install-Package Teams.ThirdPartyAppApi
```

## Requirements

- .NET 8.0 or higher
- Microsoft Teams (new Teams client)
- Must run on the same machine as Teams (localhost only) or use a socket proxy for remote connections

## Quick Start

```cs
using Teams.ThirdPartyAppApi.TeamsClient;
using System.Reactive.Linq;

// Create a cancellation token
var cts = new CancellationTokenSource();

// Create the Teams client
using var client = new TeamsClient(
    url: "127.0.0.1",
    manufacturer: "YourCompany",
    device: "YourDevice", 
    app: "YourApp",
    appVersion: "1.0",
    autoReconnect: true,
    cancellationToken: cts.Token
);

// Subscribe to connection changes
client.IsConnectedChanged.Subscribe(connected => 
{
    Console.WriteLine($"Connection status: {connected}");
});

// Subscribe to mute state changes
client.IsMutedChanged.Subscribe(isMuted => 
{
    Console.WriteLine($"Muted: {isMuted}");
});

// Connect to Teams
await client.Connect();

// Toggle mute when in a meeting
if (client.IsInMeeting && client.CanToggleMute)
{
    await client.ToggleMute();
}

// Remember to dispose when done
// (or use 'using' statement as shown above)
```

## Clients

- `TeamsClient` - for interacting with the new Teams app

### TeamsClient

The `TeamsClient` interacts with the new Teams app via WebSocket connection.

**Important Notes:**
- New Teams only accepts connections from localhost
- The client must run on the same computer as Teams
- Use a socket proxy for remote connections
- No initial token required (Teams returns a token for future use)

#### Constructor Parameters

```cs
TeamsClient(
    string url,              // WebSocket URL (typically "127.0.0.1")
    string manufacturer,     // Your company name
    string device,          // Device name/identifier
    string app,             // Your application name
    string appVersion,      // Your application version
    bool autoReconnect = true,     // Auto-reconnect on disconnect
    CancellationToken cancellationToken = default
)
```

**Alternative constructors:**
```cs
// Without token (Teams will provide one)
new TeamsClient("127.0.0.1", "manufacturer", "device", "app", "1.0");

// With custom port
new TeamsClient("127.0.0.1", 8124, "manufacturer", "device", "app", "1.0");

// With token and custom port
new TeamsClient("127.0.0.1", 8124, "existingToken", "manufacturer", "device", "app", "1.0");
```

#### Token Management

Save the token for future use:

```cs
client.TokenChanged.Subscribe(token => 
{
    // Save token to persistent storage
    SaveTokenToFile(token);
    Console.WriteLine($"New token received: {token}");
});
```

## Available Commands

Control Teams meetings with these methods:

```cs
// Meeting controls
await client.ToggleMute();              // Toggle microphone
await client.ToggleVideo();             // Toggle camera
await client.ToggleHand();              // Raise/lower hand
await client.ToggleBackgroundBlur();    // Toggle background blur
await client.LeaveCall();               // Leave meeting
await client.StopSharing();             // Stop screen sharing
await client.ToggleSharing();           // Toggle sharing (start/stop)

// UI controls
await client.ToggleUiChat();           // Open/close chat panel
await client.ToggleUiShareTray();      // Open/close share tray

// Reactions
await client.SendReactionApplause();   // Send applause reaction
await client.SendReactionLaugh();      // Send laugh reaction
await client.SendReactionLike();       // Send like reaction
await client.SendReactionLove();       // Send love reaction
await client.SendReactionWow();        // Send wow reaction

// State query
await client.QueryState();             // Manually request current state

// Connection management
await client.Connect();                // Connect to Teams
await client.Disconnect();             // Disconnect from Teams
await client.Reconnect();              // Reconnect to Teams
```

## Setup: Activate Third-party App API

Before using the library, you must enable the API in Teams:

1. Open Teams Settings → Privacy
2. Scroll to "Third-party app API" and click "Manage"
3. Enable the API
4. Join a meeting
5. Send a command from your application (e.g., `ToggleMute()`)
6. A permission popup will appear in Teams - click "Approve"

The API is now activated and ready to use.

## Reactive

This library uses Reactive dotnet <https://github.com/dotnet/reactive>.
Every change in the client is pushed to the client as an observable. The client can then subscribe to the observable to get the changes.

### Observing Connection State

```cs
_teamsClient.IsConnectedChanged
    .SubscribeAsync(async (connected) =>
    {
        if (connected)
        {
            await WhenConnected();
        }
        else
        {
            await WhenDisconnected();
        }
    });
```

### Observing Meeting State Changes

You can observe the meeting state in three different ways:

#### 1. Observe Individual Properties (Recommended)

Subscribe to specific properties that you're interested in. Each observable only emits when that specific value changes:

```cs
// Listen to mute state changes only
_teamsClient.IsMutedChanged
    .Subscribe(isMuted => 
    {
        Console.WriteLine($"Mute state changed: {isMuted}");
        UpdateMuteButton(isMuted);
    });

// Listen to video state changes only
_teamsClient.IsVideoOnChanged
    .Subscribe(isVideoOn => 
    {
        Console.WriteLine($"Video state changed: {isVideoOn}");
        UpdateVideoButton(isVideoOn);
    });

// Listen to multiple specific properties
_teamsClient.IsInMeetingChanged
    .Subscribe(isInMeeting => HandleMeetingStatusChange(isInMeeting));
    
_teamsClient.IsHandRaisedChanged
    .Subscribe(isHandRaised => UpdateHandRaisedIndicator(isHandRaised));
```

#### 2. Observe Complete State Object

Subscribe to the entire state object to react to multiple changes at once:

```cs
_teamsClient.StateChanged
    .Subscribe(state => 
    {
        Console.WriteLine($"State update - Muted: {state.IsMuted}, " +
                         $"InMeeting: {state.IsInMeeting}, " +
                         $"Video: {state.IsVideoOn}");
        
        UpdateUI(state);
    });
```

#### 3. Access Current State

Get the current state synchronously without subscribing:

```cs
bool isMuted = _teamsClient.IsMuted;
bool isInMeeting = _teamsClient.IsInMeeting;
bool canToggleMute = _teamsClient.CanToggleMute;

if (_teamsClient.IsInMeeting && _teamsClient.CanToggleMute)
{
    await _teamsClient.ToggleMute();
}
```

### Available State Properties

**Meeting State:**
- `IsMuted` / `IsMutedChanged`
- `IsHandRaised` / `IsHandRaisedChanged`
- `IsInMeeting` / `IsInMeetingChanged`
- `IsRecordingOn` / `IsRecordingOnChanged`
- `IsBackgroundBlurred` / `IsBackgroundBlurredChanged`
- `IsSharing` / `IsSharingChanged`
- `HasUnreadMessages` / `HasUnreadMessagesChanged`
- `IsVideoOn` / `IsVideoOnChanged`

**Meeting Permissions:**
- `CanToggleMute` / `CanToggleMuteChanged`
- `CanToggleVideo` / `CanToggleVideoChanged`
- `CanToggleHand` / `CanToggleHandChanged`
- `CanToggleBlur` / `CanToggleBlurChanged`
- `CanLeave` / `CanLeaveChanged`
- `CanReact` / `CanReactChanged`
- `CanToggleShareTray` / `CanToggleShareTrayChanged`
- `CanToggleChat` / `CanToggleChatChanged`
- `CanStopSharing` / `CanStopSharingChanged`
- `CanPair` / `CanPairChanged`

## Resource Management

Always dispose of the client when done to prevent memory leaks:

```cs
// Option 1: Using statement (recommended)
using var client = new TeamsClient("127.0.0.1", "manufacturer", "device", "app", "1.0");
// Client is automatically disposed when leaving scope

// Option 2: Explicit disposal
var client = new TeamsClient("127.0.0.1", "manufacturer", "device", "app", "1.0");
try
{
    await client.Connect();
    // Use client...
}
finally
{
    client.Dispose();
}

// Option 3: With cancellation token
var cts = new CancellationTokenSource();
using var client = new TeamsClient("127.0.0.1", "manufacturer", "device", "app", "1.0", 
    cancellationToken: cts.Token);

// Cancel and cleanup
cts.Cancel();
```

## Error Handling

```cs
try
{
    using var client = new TeamsClient("127.0.0.1", "manufacturer", "device", "app", "1.0");
    await client.Connect();
    
    if (client.IsInMeeting && client.CanToggleMute)
    {
        await client.ToggleMute();
    }
    else
    {
        Console.WriteLine("Cannot toggle mute - not in meeting or no permission");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Demo App

There is a complete demo console application in the solution that demonstrates:
- Connecting to Teams
- Subscribing to state changes
- Sending commands
- Token management
- Proper resource disposal

See the `demo` folder for the full example.

## Contribute

If you want to contribute to this project, please create a pull request. We will review the pull request and merge it if it is a good fit for the project.

## Bugs and feature requests

If you find a bug or have a feature request, please create an issue in the issue tracker. We will review the issue and try to fix it as soon as possible.

