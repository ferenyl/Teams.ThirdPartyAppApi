using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Teams.ThirdPartyAppApi.Benchmarks;

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class ObservableBenchmarks
{
    private BehaviorSubject<bool> _boolSubject = null!;
    private BehaviorSubject<int> _intSubject = null!;
    private IDisposable _subscription = null!;
    private int _counter;

    [GlobalSetup]
    public void Setup()
    {
        _boolSubject = new BehaviorSubject<bool>(false);
        _intSubject = new BehaviorSubject<int>(0);
        _counter = 0;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _subscription?.Dispose();
        _boolSubject?.Dispose();
        _intSubject?.Dispose();
    }

    [Benchmark(Description = "Subscribe to BehaviorSubject")]
    public IDisposable SubscribeToBehaviorSubject()
    {
        return _boolSubject.Subscribe(_ => { });
    }

    [Benchmark(Description = "OnNext to BehaviorSubject (1 subscriber)")]
    public void OnNextSingleSubscriber()
    {
        using var sub = _boolSubject.Subscribe(_ => _counter++);
        _boolSubject.OnNext(true);
    }

    [Benchmark(Description = "OnNext to BehaviorSubject (10 subscribers)")]
    public void OnNextMultipleSubscribers()
    {
        var subs = new IDisposable[10];
        for (int i = 0; i < 10; i++)
        {
            subs[i] = _boolSubject.Subscribe(_ => _counter++);
        }

        _boolSubject.OnNext(true);

        foreach (var sub in subs)
        {
            sub.Dispose();
        }
    }

    [Benchmark(Description = "DistinctUntilChanged - same value")]
    public void DistinctUntilChangedSameValue()
    {
        using var sub = _boolSubject
            .DistinctUntilChanged()
            .Subscribe(_ => _counter++);
        
        _boolSubject.OnNext(false); // Same value, should not trigger
        _boolSubject.OnNext(false);
        _boolSubject.OnNext(false);
    }

    [Benchmark(Description = "DistinctUntilChanged - different values")]
    public void DistinctUntilChangedDifferentValues()
    {
        using var sub = _boolSubject
            .DistinctUntilChanged()
            .Subscribe(_ => _counter++);
        
        _boolSubject.OnNext(true);  // Different, should trigger
        _boolSubject.OnNext(false); // Different, should trigger
        _boolSubject.OnNext(true);  // Different, should trigger
    }

    [Benchmark(Description = "Select + DistinctUntilChanged")]
    public void SelectWithDistinctUntilChanged()
    {
        using var sub = _intSubject
            .Select(x => x % 2 == 0)
            .DistinctUntilChanged()
            .Subscribe(_ => _counter++);
        
        _intSubject.OnNext(1);
        _intSubject.OnNext(2);
        _intSubject.OnNext(3);
        _intSubject.OnNext(4);
    }

    [Benchmark(Description = "Where + Select pipeline")]
    public void WhereSelectPipeline()
    {
        using var sub = _intSubject
            .Where(x => x > 0)
            .Select(x => x * 2)
            .Subscribe(_ => _counter++);
        
        _intSubject.OnNext(1);
        _intSubject.OnNext(2);
        _intSubject.OnNext(3);
    }
}
