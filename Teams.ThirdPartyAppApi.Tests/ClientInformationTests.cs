namespace Teams.ThirdPartyAppApi.Tests;

public class ClientInformationTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        // Arrange
        var manufacturer = "TestManufacturer";
        var device = "TestDevice";
        var app = "TestApp";
        var appVersion = "1.0.0";

        // Act
        var clientInfo = new ClientInformation(manufacturer, device, app, appVersion);

        // Assert
        Assert.Equal(manufacturer, clientInfo.Manufacturer);
        Assert.Equal(device, clientInfo.Device);
        Assert.Equal(app, clientInfo.App);
        Assert.Equal(appVersion, clientInfo.AppVersion);
    }

    [Fact]
    public void DefaultConstructor_ShouldSetEmptyStrings()
    {
        // Act
        var clientInfo = new ClientInformation();

        // Assert
        Assert.Equal(string.Empty, clientInfo.Manufacturer);
        Assert.Equal(string.Empty, clientInfo.Device);
        Assert.Equal(string.Empty, clientInfo.App);
        Assert.Equal(string.Empty, clientInfo.AppVersion);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var clientInfo = new ClientInformation();

        // Act
        clientInfo.Manufacturer = "NewManufacturer";
        clientInfo.Device = "NewDevice";
        clientInfo.App = "NewApp";
        clientInfo.AppVersion = "2.0.0";

        // Assert
        Assert.Equal("NewManufacturer", clientInfo.Manufacturer);
        Assert.Equal("NewDevice", clientInfo.Device);
        Assert.Equal("NewApp", clientInfo.App);
        Assert.Equal("2.0.0", clientInfo.AppVersion);
    }
}
