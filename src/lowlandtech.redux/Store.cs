namespace LowlandTech.Redux;

public class Store<TState> : IStore<TState>
{
    private readonly ReducerAsync<TState> _reducer;
    private readonly List<MiddlewareHandler<TState>> _middlewares;
    private readonly SemaphoreSlim _syncRoot = new(1, 1);

    private TState _lastState;
    private readonly Stack<TState> _history = new();
    private readonly Stack<TState> _future = new();

    public event Action StateChanged;
    public event Func<Task> StateChangedAsync;

    public Action<IAction, TState> OnAfterDispatch { get; set; }

    public Store(
        ReducerAsync<TState> reducer,
        TState initialState = default,
        params MiddlewareHandler<TState>[] middlewares)
    {
        _reducer = reducer ?? throw new ArgumentNullException(nameof(reducer));
        _middlewares = middlewares?.ToList() ?? new List<MiddlewareHandler<TState>>();
        _lastState = initialState;
    }

    public TState GetState() => _lastState;

    public TResult Select<TResult>(Func<TState, TResult> selector) =>
        selector(_lastState);

    public async Task<object> DispatchAsync(IAction action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        var index = -1;

        Task<object> Next()
        {
            index++;
            if (index < _middlewares.Count)
                return _middlewares[index](this, action, Next);
            else
                return ApplyReducer(action);
        }

        var result = await Next();

        OnAfterDispatch?.Invoke(action, _lastState);

        return result;
    }

    private async Task<object> ApplyReducer(IAction action)
    {
        await _syncRoot.WaitAsync();
        try
        {
            _history.Push(_lastState);
            _lastState = await _reducer(_lastState, action);
            _future.Clear();
        }
        finally
        {
            _syncRoot.Release();
        }

        StateChanged?.Invoke();

        if (StateChangedAsync != null)
        {
            var handlers = StateChangedAsync.GetInvocationList();
            foreach (Func<Task> handler in handlers)
            {
                await handler();
            }
        }

        return action;
    }

    public void Undo()
    {
        if (_history.Count == 0) return;
        _future.Push(_lastState);
        _lastState = _history.Pop();
        StateChanged?.Invoke();
    }

    public void Redo()
    {
        if (_future.Count == 0) return;
        _history.Push(_lastState);
        _lastState = _future.Pop();
        StateChanged?.Invoke();
    }
}