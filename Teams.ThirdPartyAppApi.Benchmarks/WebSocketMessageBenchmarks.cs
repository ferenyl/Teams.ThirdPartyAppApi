using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using System.Text;

namespace Teams.ThirdPartyAppApi.Benchmarks;

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class WebSocketMessageBenchmarks
{
    private const string SmallMessage = "Hello Teams";
    private const string MediumMessage = "{\"action\":\"toggle-mute\",\"parameters\":{\"type\":null},\"requestId\":123}";
    private string _largeMessage = null!;
    private string _hugeMessage = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Simulate realistic Teams API messages
        _largeMessage = GenerateRealisticMessage(500);
        _hugeMessage = GenerateRealisticMessage(5000);
    }

    private string GenerateRealisticMessage(int size)
    {
        var sb = new StringBuilder();
        sb.Append("{\"requestId\":1,\"response\":\"Success\",\"meetingUpdate\":{\"meetingState\":{");
        sb.Append("\"isMuted\":true,\"isHandRaised\":false,\"isInMeeting\":true,");
        sb.Append("\"isRecordingOn\":false,\"isBackgroundBlurred\":true},");
        sb.Append("\"meetingPermissions\":{\"canToggleMute\":true,\"canToggleVideo\":true}},");
        sb.Append("\"extraData\":\"");
        sb.Append(new string('x', Math.Max(0, size - 200)));
        sb.Append("\"}");
        return sb.ToString();
    }

    [Benchmark(Description = "UTF8 Encode - Small (12 bytes)")]
    public byte[] EncodeSmallMessage()
    {
        return Encoding.UTF8.GetBytes(SmallMessage);
    }

    [Benchmark(Description = "UTF8 Encode - Medium (79 bytes)")]
    public byte[] EncodeMediumMessage()
    {
        return Encoding.UTF8.GetBytes(MediumMessage);
    }

    [Benchmark(Description = "UTF8 Encode - Large (500 bytes)")]
    public byte[] EncodeLargeMessage()
    {
        return Encoding.UTF8.GetBytes(_largeMessage);
    }

    [Benchmark(Description = "UTF8 Encode - Huge (5KB)")]
    public byte[] EncodeHugeMessage()
    {
        return Encoding.UTF8.GetBytes(_hugeMessage);
    }

    [Benchmark(Description = "UTF8 Decode - Medium")]
    public string DecodeMediumMessage()
    {
        var bytes = Encoding.UTF8.GetBytes(MediumMessage);
        return Encoding.UTF8.GetString(bytes);
    }

    [Benchmark(Description = "UTF8 Decode - Large")]
    public string DecodeLargeMessage()
    {
        var bytes = Encoding.UTF8.GetBytes(_largeMessage);
        return Encoding.UTF8.GetString(bytes);
    }
}
