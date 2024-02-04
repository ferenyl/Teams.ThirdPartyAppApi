namespace Teams.ThirdPartyAppApi;

public class ClientInformation
{
    public string Manufacturer { get; set; } = string.Empty;
    public string Device { get; set; } = string.Empty;
    public string App { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;

    public ClientInformation(string manufacturer, string device, string app, string appVersion)
    {
        Manufacturer = manufacturer;
        Device = device;
        App = app;
        AppVersion = appVersion;
    }

    public ClientInformation()
    {
    }
}