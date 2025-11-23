using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using System.Reactive.Subjects;
using Teams.ThirdPartyAppApi.TeamsClient;

namespace Teams.ThirdPartyAppApi.Benchmarks;

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class StateManagementBenchmarks
{
    private BehaviorSubject<MeetingStateSnapshot> _stateSubject = null!;
    private MeetingStateSnapshot _initialState = null!;
    private MeetingStateSnapshot _changedState = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _initialState = MeetingStateSnapshot.Default;
        _changedState = new MeetingStateSnapshot
        {
            IsMuted = true,
            IsHandRaised = true,
            IsInMeeting = true,
            IsVideoOn = true,
            CanToggleMute = true,
            CanToggleVideo = true
        };
        _stateSubject = new BehaviorSubject<MeetingStateSnapshot>(_initialState);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _stateSubject?.Dispose();
    }

    [Benchmark(Description = "Create default state")]
    public MeetingStateSnapshot CreateDefaultState()
    {
        return MeetingStateSnapshot.Default;
    }

    [Benchmark(Description = "Create state with values")]
    public MeetingStateSnapshot CreateStateWithValues()
    {
        return new MeetingStateSnapshot
        {
            IsMuted = true,
            IsHandRaised = false,
            IsInMeeting = true,
            IsRecordingOn = false,
            IsBackgroundBlurred = true,
            IsSharing = false,
            HasUnreadMessages = true,
            IsVideoOn = false,
            CanToggleMute = true,
            CanToggleVideo = true,
            CanToggleHand = false,
            CanToggleBlur = true,
            CanLeave = true,
            CanReact = false,
            CanToggleShareTray = true,
            CanToggleChat = false,
            CanStopSharing = false,
            CanPair = true
        };
    }

    [Benchmark(Description = "State With() method - single property")]
    public MeetingStateSnapshot StateWithSingleProperty()
    {
        return _initialState.With(isMuted: true);
    }

    [Benchmark(Description = "State With() method - multiple properties")]
    public MeetingStateSnapshot StateWithMultipleProperties()
    {
        return _initialState.With(
            isMuted: true, 
            isHandRaised: true, 
            isInMeeting: true,
            isVideoOn: true);
    }

    [Benchmark(Description = "State Equals() - equal states")]
    public bool StateEqualsTrue()
    {
        return _initialState.Equals(_initialState);
    }

    [Benchmark(Description = "State Equals() - different states")]
    public bool StateEqualsFalse()
    {
        return _initialState.Equals(_changedState);
    }

    [Benchmark(Description = "State GetHashCode()")]
    public int StateGetHashCode()
    {
        return _initialState.GetHashCode();
    }

    [Benchmark(Description = "Emit state to BehaviorSubject")]
    public void EmitStateToSubject()
    {
        _stateSubject.OnNext(_changedState);
    }

    [Benchmark(Description = "Read state from BehaviorSubject")]
    public MeetingStateSnapshot ReadStateFromSubject()
    {
        return _stateSubject.Value;
    }
}
