# Third-party app API Client
![main](https://github.com/ferenyl/Teams.ThirdPartyAppApi/actions/workflows/dotnet.yml/badge.svg?branch=main) ![NuGet Version](https://img.shields.io/nuget/v/Teams.ThirdPartyAppApi?style=flat)


Use this Package to communicate with the [Microsoft Teams Third-party app API](https://support.microsoft.com/en-us/office/connect-third-party-devices-to-teams-aabca9f2-47bb-407f-9f9b-81a104a883d6)

## Clients
There are two clients available in this package:
- `TeamsClient` - for interacting with the legacy Teams app
- `NewTeamsClient` - for interacting with the new Teams app

### TeamsClient
This client is used to interact with the legacy Teams app. It is used to send and receive messages from the Teams app.

#### usage
``` csharp
//TeamsClient(string url, string token = "", bool autoReconnect = false, CancellationToken cancellationToken = default)
new TeamsClient("127.0.01", "token from app", true, cts)
```


### NewTeamsClient
This client is used to interact with the new Teams app. It is used to send and receive messages from the Teams app. New Teams only accepts connetcion from localhost so this must run on the same computer as teams. But you can use a socket proxy to use it remotly

This client do not require a initial token. But teams will return a token for future use. To save that token you must subscribe to NewTeamsClient.TokenChanged

``` c#
//NewTeamsClient(string url, string manufacturer, string device, string app, string appVersion, bool autoReconnect = true, CancellationToken cancellationToken = default)
var client = new NewTeamsClient("127.0.0.1", "companyName", "company device", "teams integgrator", "1.0", true, cts)
client.
    TokenChanged
    .Subscribe(token => SaveToken)
```

#### usage
``` csharp
//NewTeamsClient(string url, string manufacturer, string device, string app, string appVersion, bool autoReconnect = true, CancellationToken cancellationToken = default)
new NewTeamsClient("127.0.0.1", "companyName", "company device", "teams integgrator", "1.0", true, cts)
```

## Activate third party app api
### old teams
Go to settings -> privacy and scroll down to third party api and manage. Then activate it and copy the token.

### New teams 
go to setttings -> privacy scroll to third party api and manage. Activate and go to a meeting. When in a meeting, send a command to teams. A permission popup will appear, press approve.



## Reactive
This library uses Reactive dotnet https://github.com/dotnet/reactive.
Every change in the client is pushed to the client as an observable. The client can then subscribe to the observable to get the changes.

### Example
``` csharp
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

## Demo app
There is a demo app in the solution that shows how to use the clients. The demo app is a console app that connects to the Teams app and sends and receives messages.

## Contribute
If you want to contribute to this project, please create a pull request. We will review the pull request and merge it if it is a good fit for the project.

## Bugs and feature requests
If you find a bug or have a feature request, please create an issue in the issue tracker. We will review the issue and try to fix it as soon as possible.