using System.Reactive.Subjects;

namespace Teams.ThirdPartyAppApi;

internal static class ObservableExtensions
{
    public static void OnNextIfValueChanged<T>(this BehaviorSubject<T> subject, T value)
    {
        if (subject.IsDisposed)
            return;

        if (subject.TryGetValue(out var oldValue) && EqualityComparer<T>.Default.Equals(oldValue, value))
        {
            subject.OnNext(value);
        }
    }
}