using Teams.ThirdPartyAppApi.TeamsClient;
using static System.Console;

CancellationTokenSource cts = new();
CancelKeyPress += (source, args) =>
{
    Error.WriteLine("Cancelling...");

    args.Cancel = true;
    cts.Cancel();
};
var teamsIp = Environment.GetEnvironmentVariable("teamsip", EnvironmentVariableTarget.User) ?? "127.0.0.1";
var portEnv = int.TryParse(Environment.GetEnvironmentVariable("teamsport", EnvironmentVariableTarget.User), out var result);
var teamsPort = portEnv ? result : 8124;
var teamsToken = Environment.GetEnvironmentVariable("teamstoken", EnvironmentVariableTarget.User) ?? string.Empty;


await NewTeams();



async Task NewTeams()
{
    var Teams = new TeamsClient(teamsIp, teamsPort, teamsToken, "TeamsLocalApi", "TeamsLocalApi device", "TeamsLocalApi app", "1.0.0", autoReconnect: true, cts.Token) ?? throw new Exception("Could not create client");

    Teams.IsConnectedChanged
        .Subscribe(connected => WriteLine(connected ? "Event: Connected" : "Event: Disconnected"));
    Teams.IsInMeetingChanged
        .Subscribe(connected => WriteLine(connected ? "Event: InMeeting" : "Event: NotInMeeting"));
    Teams.TokenChanged
        .Subscribe(token => WriteLine($"Event: TokenChanged: {token}"));

    WriteLine("Connecting...");
    await Teams.Connect();
    WriteLine("Connected...");

    var methods = Teams.GetType().GetMethods().Where(m => m.IsPublic && !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_")).ToArray();

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
                if (!int.TryParse(choice, out var call))
                    continue;

                _ = methods[call].Invoke(Teams, []);
                break;
        }


    } while (choice != "q");
}
