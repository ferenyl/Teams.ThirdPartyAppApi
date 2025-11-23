using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using System.Buffers;

namespace Teams.ThirdPartyAppApi.Benchmarks;

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class BufferPoolBenchmarks
{
    private const int SmallBufferSize = 256;
    private const int MediumBufferSize = 4096;  // WebSocket default
    private const int LargeBufferSize = 65536;

    [Benchmark(Baseline = true, Description = "New Array - Small (256 bytes)")]
    public byte[] AllocateSmallArrayNew()
    {
        var buffer = new byte[SmallBufferSize];
        // Simulate usage
        buffer[0] = 1;
        return buffer;
    }

    [Benchmark(Description = "ArrayPool - Small (256 bytes)")]
    public byte[] AllocateSmallArrayPool()
    {
        var buffer = ArrayPool<byte>.Shared.Rent(SmallBufferSize);
        try
        {
            // Simulate usage
            buffer[0] = 1;
            return buffer;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    [Benchmark(Description = "New Array - Medium (4KB)")]
    public byte[] AllocateMediumArrayNew()
    {
        var buffer = new byte[MediumBufferSize];
        buffer[0] = 1;
        return buffer;
    }

    [Benchmark(Description = "ArrayPool - Medium (4KB)")]
    public byte[] AllocateMediumArrayPool()
    {
        var buffer = ArrayPool<byte>.Shared.Rent(MediumBufferSize);
        try
        {
            buffer[0] = 1;
            return buffer;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    [Benchmark(Description = "New Array - Large (64KB)")]
    public byte[] AllocateLargeArrayNew()
    {
        var buffer = new byte[LargeBufferSize];
        buffer[0] = 1;
        return buffer;
    }

    [Benchmark(Description = "ArrayPool - Large (64KB)")]
    public byte[] AllocateLargeArrayPool()
    {
        var buffer = ArrayPool<byte>.Shared.Rent(LargeBufferSize);
        try
        {
            buffer[0] = 1;
            return buffer;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    [Benchmark(Description = "Multiple allocations - New Array")]
    public void MultipleAllocationsNew()
    {
        for (int i = 0; i < 100; i++)
        {
            var buffer = new byte[MediumBufferSize];
            buffer[0] = (byte)i;
        }
    }

    [Benchmark(Description = "Multiple allocations - ArrayPool")]
    public void MultipleAllocationsPool()
    {
        for (int i = 0; i < 100; i++)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(MediumBufferSize);
            try
            {
                buffer[0] = (byte)i;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
