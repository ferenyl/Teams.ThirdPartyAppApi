using BenchmarkDotNet.Running;

namespace Teams.ThirdPartyAppApi.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
        {
            Console.WriteLine("Teams.ThirdPartyAppApi Performance Benchmarks");
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.WriteLine("Available benchmark suites:");
            Console.WriteLine("  all              - Run all benchmarks (takes longest)");
            Console.WriteLine("  websocket        - WebSocket message encoding/decoding");
            Console.WriteLine("  state            - State management (MeetingStateSnapshot)");
            Console.WriteLine("  buffer           - Buffer allocation (ArrayPool vs new)");
            Console.WriteLine("  json             - JSON serialization/deserialization");
            Console.WriteLine("  observable       - Reactive observables performance");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run -c Release -- <suite>");
            Console.WriteLine("  dotnet run -c Release -- all");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run -c Release -- buffer");
            Console.WriteLine("  dotnet run -c Release -- state");
            Console.WriteLine();
            return;
        }

        switch (args[0].ToLowerInvariant())
        {
            case "all":
                BenchmarkRunner.Run<WebSocketMessageBenchmarks>();
                BenchmarkRunner.Run<StateManagementBenchmarks>();
                BenchmarkRunner.Run<BufferPoolBenchmarks>();
                BenchmarkRunner.Run<JsonSerializationBenchmarks>();
                BenchmarkRunner.Run<ObservableBenchmarks>();
                break;
            
            case "websocket":
                BenchmarkRunner.Run<WebSocketMessageBenchmarks>();
                break;
            
            case "state":
                BenchmarkRunner.Run<StateManagementBenchmarks>();
                break;
            
            case "buffer":
                BenchmarkRunner.Run<BufferPoolBenchmarks>();
                break;
            
            case "json":
                BenchmarkRunner.Run<JsonSerializationBenchmarks>();
                break;
            
            case "observable":
                BenchmarkRunner.Run<ObservableBenchmarks>();
                break;
            
            default:
                Console.WriteLine($"Unknown benchmark suite: {args[0]}");
                Console.WriteLine("Run with --help to see available options.");
                return;
        }
    }
}
