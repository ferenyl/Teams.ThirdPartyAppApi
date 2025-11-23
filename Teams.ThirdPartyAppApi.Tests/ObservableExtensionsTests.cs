using System.Reactive.Subjects;

namespace Teams.ThirdPartyAppApi.Tests;

public class ObservableExtensionsTests
{
    [Fact]
    public void OnNextIfValueChanged_ShouldNotEmit_WhenValueIsTheSame()
    {
        // Arrange
        var subject = new BehaviorSubject<int>(5);
        var emittedValues = new List<int>();
        subject.Subscribe(value => emittedValues.Add(value));

        // Act
        subject.OnNextIfValueChanged(5);

        // Assert
        Assert.Single(emittedValues);
        Assert.Equal(5, emittedValues[0]);
    }

    [Fact]
    public void OnNextIfValueChanged_ShouldEmit_WhenValueIsChanged()
    {
        // Arrange
        var subject = new BehaviorSubject<int>(5);
        var emittedValues = new List<int>();
        subject.Subscribe(value => emittedValues.Add(value));

        // Act
        subject.OnNextIfValueChanged(10);

        // Assert
        Assert.Equal(2, emittedValues.Count);
        Assert.Equal(5, emittedValues[0]);
        Assert.Equal(10, emittedValues[1]);
    }

    [Fact]
    public void OnNextIfValueChanged_ShouldNotThrow_WhenSubjectIsDisposed()
    {
        // Arrange
        var subject = new BehaviorSubject<int>(5);
        subject.Dispose();

        // Act & Assert
        var exception = Record.Exception(() => subject.OnNextIfValueChanged(10));
        Assert.Null(exception);
    }

    [Fact]
    public void OnNextIfValueChanged_ShouldWorkWithStrings()
    {
        // Arrange
        var subject = new BehaviorSubject<string>("hello");
        var emittedValues = new List<string>();
        subject.Subscribe(value => emittedValues.Add(value));

        // Act
        subject.OnNextIfValueChanged("hello");
        subject.OnNextIfValueChanged("world");

        // Assert
        Assert.Equal(2, emittedValues.Count);
        Assert.Equal("hello", emittedValues[0]);
        Assert.Equal("world", emittedValues[1]);
    }

    [Fact]
    public void OnNextIfValueChanged_ShouldWorkWithBooleans()
    {
        // Arrange
        var subject = new BehaviorSubject<bool>(false);
        var emittedValues = new List<bool>();
        subject.Subscribe(value => emittedValues.Add(value));

        // Act
        subject.OnNextIfValueChanged(false);
        subject.OnNextIfValueChanged(true);
        subject.OnNextIfValueChanged(true);
        subject.OnNextIfValueChanged(false);

        // Assert
        Assert.Equal(3, emittedValues.Count);
        Assert.False(emittedValues[0]);
        Assert.True(emittedValues[1]);
        Assert.False(emittedValues[2]);
    }
}
