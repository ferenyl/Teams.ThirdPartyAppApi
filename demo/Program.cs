using Teams.ThirdPartyAppApi.NewTeams;
using Teams.ThirdPartyAppApi.Teams;
using static System.Console;

CancellationTokenSource cts = new();
CancelKeyPress += (source, args) =>
{
    Error.WriteLine("Cancelling...");

    args.Cancel = true;
    cts.Cancel();
};

WriteLine("Choose client:\r\n");
WriteLine("1. new client:\r\n");
WriteLine("2. old client:\r\n");
var clientChoice = ReadLine() ?? "";

switch (clientChoice)
{
    case "1":
        await NewTeams();
        break;
    case "2":
        await OldTeams();
        break;
    default:
        break;
}


async Task NewTeams()
{
    var Teams = new NewTeamsClient("127.0.0.1", "TeamsLocalApi", "TeamsLocalApi device", "TeamsLocalApi app", "1.0.0", autoReconnect: true, cts.Token) ?? throw new Exception("Could not create client");

    Teams.IsConnectedChanged
        .Subscribe(connected => WriteLine(connected ? "Event: Connected" : "Event: Disconnected"));

    WriteLine("Connecting...");
    await Teams.Connect();
    WriteLine("Connected...");

    var methods = Teams.GetType().GetMethods().Where(m => m.IsPublic && !m.Name.StartsWith("get_")).ToArray();

    string choice;

    do
    {
        WriteLine("Choose method:\r\n");

        for (var i = 0; i < methods.Length; i++)
            WriteLine("{0}:\t{1}", i, methods[i].Name);

        WriteLine("{0}:\t{1}", "q", "Quit");

        choice = ReadLine() ?? "";

        if (choice == "q")
            continue;

        switch (choice)
        {
            case "q":
                continue;
            case "c":
                cts.Cancel();
                break;
            case "m":
                await Teams.ToggleMute();
                break;
            case "k":
                await Teams.ToggleVideo();
                break;
            default:
                if (!int.TryParse(choice, out int call))
                    continue;

                _ = methods[call].Invoke(Teams, [CancellationToken.None]);
                break;
        }


    } while (choice != "q");
}


async Task OldTeams()
{

    var Teams = new TeamsClient("localhost", token: "", autoReconnect: true, cts.Token) ?? throw new Exception("Could not create client");

    Teams.IsConnectedChanged
        .Subscribe(connected => WriteLine(connected ? "Event: Connected" : "Event: Disconnected"));

    WriteLine("Connecting...");
    await Teams.Connect();
    WriteLine("Connected...");

    var methods = Teams.GetType().GetMethods().Where(m => m.IsPublic).ToArray();
    string choice;
    do
    {
        WriteLine("Choose method:\r\n");

        for (var i = 0; i < methods.Length; i++)
            WriteLine("{0}:\t{1}", i, methods[i].Name);

        WriteLine("{0}:\t{1}", "q", "Quit");

        choice = ReadLine() ?? "";

        if (choice == "q")
            continue;

        switch (choice)
        {
            case "q":
                continue;
            case "c":
                cts.Cancel();
                break;
            case "m+":
                await Teams.Mute();
                break;
            case "m-":
                await Teams.UnMute();
                break;
            default:
                if (!int.TryParse(choice, out int call))
                    continue;

                _ = methods[call].Invoke(Teams, [CancellationToken.None]);
                break;
        }


    } while (choice != "q");
}
