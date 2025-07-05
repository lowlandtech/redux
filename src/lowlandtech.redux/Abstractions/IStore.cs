namespace LowlandTech.Redux.Abstractions;

public interface IStore<TState>
{
    Task<object> DispatchAsync(IAction action);
    TState GetState();
    TResult Select<TResult>(Func<TState, TResult> selector);
    event Action StateChanged;
    event Func<Task> StateChangedAsync;
    void Undo();
    void Redo();
}