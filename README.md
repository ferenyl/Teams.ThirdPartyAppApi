# Third-party app API Client

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
This client is used to interact with the new Teams app. It is used to send and receive messages from the Teams app. It only works localy until the Teams allows connections from another connection than localhost.

#### usage
``` csharp
//NewTeamsClient(string url, string manufacturer, string device, string app, string appVersion, bool autoReconnect = true, CancellationToken cancellationToken = default)
new NewTeamsClient("127.0.0.1", "companyName", "company device", "teams integgrator", "1.0", true, cts)
```

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