namespace MinimalPomodoro.Core;

public interface IClock
{
    DateTimeOffset Now { get; }
}
