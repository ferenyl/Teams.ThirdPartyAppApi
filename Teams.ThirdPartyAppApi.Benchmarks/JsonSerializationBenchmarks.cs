using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using System.Text.Json;
using Teams.ThirdPartyAppApi.TeamsClient;

namespace Teams.ThirdPartyAppApi.Benchmarks;

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class JsonSerializationBenchmarks
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private string _simpleCommandJson = null!;
    private string _complexServerMessageJson = null!;
    private ClientMessage _clientMessage = null!;

    [GlobalSetup]
    public void Setup()
    {
        _clientMessage = new ClientMessage("toggle-mute", null) { RequestId = 1 };
        
        _simpleCommandJson = "{\"action\":\"toggle-mute\",\"parameters\":{\"type\":null},\"requestId\":1}";
        
        _complexServerMessageJson = @"{
            ""requestId"": 1,
            ""response"": ""Success"",
            ""tokenRefresh"": ""token123"",
            ""meetingUpdate"": {
                ""meetingState"": {
                    ""isMuted"": true,
                    ""isHandRaised"": false,
                    ""isInMeeting"": true,
                    ""isRecordingOn"": false,
                    ""isBackgroundBlurred"": true,
                    ""isSharing"": false,
                    ""hasUnreadMessages"": true,
                    ""isVideoOn"": false
                },
                ""meetingPermissions"": {
                    ""canToggleMute"": true,
                    ""canToggleVideo"": true,
                    ""canToggleHand"": true,
                    ""canToggleBlur"": true,
                    ""canLeave"": true,
                    ""canReact"": true,
                    ""canToggleShareTray"": true,
                    ""canToggleChat"": true,
                    ""canStopSharing"": false,
                    ""canPair"": true
                }
            }
        }";
    }

    [Benchmark(Description = "Serialize ClientMessage")]
    public string SerializeClientMessage()
    {
        return JsonSerializer.Serialize(_clientMessage, _options);
    }

    [Benchmark(Description = "Deserialize simple command")]
    public object? DeserializeSimpleCommand()
    {
        return JsonSerializer.Deserialize<ClientMessage>(_simpleCommandJson, _options);
    }

    [Benchmark(Description = "Deserialize complex ServerMessage")]
    public object? DeserializeComplexServerMessage()
    {
        return JsonSerializer.Deserialize<ServerMessage>(_complexServerMessageJson, _options);
    }

    [Benchmark(Description = "Serialize + Deserialize round-trip")]
    public object? SerializeDeserializeRoundTrip()
    {
        var json = JsonSerializer.Serialize(_clientMessage, _options);
        return JsonSerializer.Deserialize<ClientMessage>(json, _options);
    }
}
