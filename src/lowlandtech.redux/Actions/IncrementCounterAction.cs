namespace LowlandTech.Redux.Actions;

public record IncrementCounterAction(int Amount) : IAction
{
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
}