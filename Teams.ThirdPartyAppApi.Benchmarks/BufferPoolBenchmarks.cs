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
    public void AllocateSmallArrayNew()
    {
        var buffer = new byte[SmallBufferSize];
        buffer[0] = 1;
    }

    [Benchmark(Description = "ArrayPool - Small (256 bytes)")]
    public void AllocateSmallArrayPool()
    {
        var buffer = ArrayPool<byte>.Shared.Rent(SmallBufferSize);
        try
        {
            buffer[0] = 1;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    [Benchmark(Description = "New Array - Medium (4KB)")]
    public void AllocateMediumArrayNew()
    {
        var buffer = new byte[MediumBufferSize];
        buffer[0] = 1;
    }

    [Benchmark(Description = "ArrayPool - Medium (4KB)")]
    public void AllocateMediumArrayPool()
    {
        var buffer = ArrayPool<byte>.Shared.Rent(MediumBufferSize);
        try
        {
            buffer[0] = 1;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    [Benchmark(Description = "New Array - Large (64KB)")]
    public void AllocateLargeArrayNew()
    {
        var buffer = new byte[LargeBufferSize];
        buffer[0] = 1;
    }

    [Benchmark(Description = "ArrayPool - Large (64KB)")]
    public void AllocateLargeArrayPool()
    {
        var buffer = ArrayPool<byte>.Shared.Rent(LargeBufferSize);
        try
        {
            buffer[0] = 1;
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
