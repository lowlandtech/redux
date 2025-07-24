namespace LowlandTech.Redux;

/// <summary>
/// Represents a state management store that maintains the state of type <typeparamref name="TState"/>  and provides
/// mechanisms for state updates, middleware processing, and undo/redo functionality.
/// </summary>
/// <remarks>The <see cref="Store{TState}"/> class is designed to manage application state in a predictable
/// manner.  It supports asynchronous reducers, middleware for processing actions, and undo/redo functionality for state
/// changes.  State changes trigger the <see cref="StateChanged"/> and <see cref="StateChangedAsync"/> events,  allowing
/// subscribers to react to updates.</remarks>
/// <typeparam name="TState">The type of the state managed by the store.</typeparam>
public record Store<TState> : IStore<TState>
{
    /// <summary>
    /// Represents a subject that tracks and notifies observers of <see cref="IAction"/> events.
    /// </summary>
    /// <remarks>This field is used internally to manage and propagate actions within the system.  It allows
    /// observers to subscribe and react to <see cref="IAction"/> events as they occur.</remarks>
    private readonly Subject<IAction> _actions = new();

    /// <summary>
    /// Represents a subject that emits events of type <see cref="IEvent"/>.
    /// </summary>
    /// <remarks>This field is used internally to manage and broadcast events to subscribers. It is a readonly
    /// field and cannot be modified after initialization.</remarks>
    private readonly Subject<IEvent> _events = new();

    private readonly ReducerAsync<TState> _reducer;
    private readonly List<MiddlewareHandler<TState>> _middlewares;
    private readonly SemaphoreSlim _syncRoot = new(1, 1);

    private TState _lastState;
    private readonly Stack<TState> _history = new();
    private readonly Stack<TState> _future = new();

    /// <summary>
    /// Occurs when the state of the object changes.
    /// </summary>
    /// <remarks>Subscribe to this event to be notified whenever the state of the object is updated.  The
    /// event handler will be invoked with no parameters.</remarks>
    public event Action? StateChanged;

    /// <summary>
    /// Occurs when the state of the object changes asynchronously.
    /// </summary>
    /// <remarks>This event is triggered to notify subscribers of a state change.  Subscribers can handle the
    /// event by providing an asynchronous callback method.</remarks>
    public event Func<Task>? StateChangedAsync;

    /// <summary>
    /// Gets or sets the action to be executed after an action has been dispatched.
    /// </summary>
    /// <remarks>The action receives the dispatched action and the associated state as parameters. This can be
    /// used to perform post-dispatch operations such as logging, state validation,  or triggering side
    /// effects.</remarks>
    public Action<IAction, TState>? OnAfterDispatch { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Store{TState}"/> class, which manages state updates using a reducer
    /// function and optional middleware.
    /// </summary>
    /// <param name="reducer">The asynchronous reducer function that processes actions and returns the updated state. This parameter cannot be
    /// <see langword="null"/>.</param>
    /// <param name="initialState">The initial state of the store. If not specified, the default value of <typeparamref name="TState"/> is used.</param>
    /// <param name="middlewares">An optional array of middleware handlers that can intercept and modify actions or state updates. If no
    /// middleware is provided, an empty list is used.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="reducer"/> is <see langword="null"/>.</exception>
    public Store(
        ReducerAsync<TState> reducer,
        TState initialState = default,
        params MiddlewareHandler<TState>[] middlewares)
    {
        _reducer = reducer ?? throw new ArgumentNullException(nameof(reducer));
        _middlewares = middlewares?.ToList() ?? new List<MiddlewareHandler<TState>>();
        _lastState = initialState;
    }

    /// <summary>
    /// Retrieves the most recent state of the object.
    /// </summary>
    /// <returns>The last recorded state of type <typeparamref name="TState"/>.</returns>
    public TState GetState() => _lastState;

    /// <summary>
    /// Projects the current state into a new form by applying the specified selector function.
    /// </summary>
    /// <typeparam name="TResult">The type of the value returned by the selector function.</typeparam>
    /// <param name="selector">A function that transforms the current state into a result of type <typeparamref name="TResult"/>.</param>
    /// <returns>The result of applying the <paramref name="selector"/> function to the current state.</returns>
    public TResult Select<TResult>(Func<TState, TResult> selector) =>
        selector(_lastState);
    
    /// <summary>
    /// Gets an observable sequence of events.
    /// </summary>
    /// <remarks>Subscribers to this property will receive notifications for each event in the sequence. 
    /// Ensure proper disposal of subscriptions to avoid memory leaks.</remarks>
    public IObservable<IEvent> Events => _events.AsObservable();

    /// <summary>
    /// Publishes the specified event to all subscribed observers.
    /// </summary>
    /// <param name="event">The event to be published. Cannot be <see langword="null"/>.</param>
    public void PublishEvent(IEvent @event)
    {
        _events.OnNext(@event);
    }

    /// <summary>
    /// Gets an observable sequence of actions.
    /// </summary>
    /// <remarks>Subscribers to this observable will receive notifications for each action emitted.  Ensure
    /// proper subscription management to avoid memory leaks or unintended behavior.</remarks>
    public IObservable<IAction> Actions => _actions.AsObservable();

    /// <summary>
    /// Dispatches an action to the middleware pipeline and applies the reducer to update the state.
    /// </summary>
    /// <remarks>The method processes the provided action through a chain of middleware components. Each
    /// middleware can either handle the action or pass it to the next middleware in the chain. If no middleware handles
    /// the action, the reducer is applied to update the state. After the action is processed, the <see
    /// cref="OnAfterDispatch"/> event is invoked, if subscribed, with the dispatched action and the updated
    /// state.</remarks>
    /// <param name="action">The action to be dispatched. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result contains the value returned by the
    /// middleware pipeline or the reducer.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
    public async Task<object> DispatchAsync(IAction action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        var index = -1;

        Task<object> Next(IAction? maybeAction)
        {
            index++;
            var effectiveAction = maybeAction ?? action;

            if (index < _middlewares.Count)
                return _middlewares[index](this, effectiveAction, Next);
            else
                return Dispatch(effectiveAction);
        }

        var result = await Next(null);

        OnAfterDispatch?.Invoke(action, _lastState);

        return result;
    }

    /// <summary>
    /// Applies the specified action to the current state using the reducer function and updates the state.
    /// </summary>
    /// <remarks>This method ensures thread-safe state updates by using a synchronization mechanism. It saves
    /// the current state to the history stack before applying the reducer function to compute the new state. After the
    /// state is updated, the future state stack is cleared, and state change notifications are triggered synchronously
    /// and asynchronously.</remarks>
    /// <param name="action">The action to be applied to the current state. Cannot be null.</param>
    /// <returns>The action that was applied, returned as an object. This allows the caller to confirm the action processed.</returns>
    public async Task<object> Dispatch(IAction action)
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

    /// <summary>
    /// Reverts the state to the most recent previous state in the history.
    /// </summary>
    /// <remarks>This method restores the last saved state from the history stack and moves the current state
    /// to the future stack. If the history stack is empty, the method does nothing.</remarks>
    public void Undo()
    {
        if (_history.Count == 0) return;
        _future.Push(_lastState);
        _lastState = _history.Pop();
        StateChanged?.Invoke();
    }

    /// <summary>
    /// Reapplies the most recently undone state, restoring it as the current state.
    /// </summary>
    /// <remarks>This method moves the most recent state from the redo stack to the undo stack and updates the
    /// current state.  If there are no states available to redo, the method does nothing.</remarks>
    public void Redo()
    {
        if (_future.Count == 0) return;
        _history.Push(_lastState);
        _lastState = _future.Pop();
        StateChanged?.Invoke();
    }
}