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
public record Store<TState> : IStore<TState>, IDisposable
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
    private readonly List<Action<TState>> _listeners = new();
    private bool _disposed;

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
        TState? initialState = default,
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
    /// <exception cref="ObjectDisposedException">Thrown if the store has been disposed.</exception>
    public TState GetState()
    {
        ThrowIfDisposed();
        return _lastState;
    }

    /// <summary>
    /// Projects the current state into a new form by applying the specified selector function.
    /// </summary>
    /// <typeparam name="TResult">The type of the value returned by the selector function.</typeparam>
    /// <param name="selector">A function that transforms the current state into a result of type <typeparamref name="TResult"/>.</param>
    /// <returns>The result of applying the <paramref name="selector"/> function to the current state.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the store has been disposed.</exception>
    public TResult Select<TResult>(Func<TState, TResult> selector)
    {
        ThrowIfDisposed();
        return selector(_lastState);
    }
    
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
    /// <exception cref="ObjectDisposedException">Thrown if the store has been disposed.</exception>
    public void PublishEvent(IEvent @event)
    {
        ThrowIfDisposed();
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
    /// <exception cref="ObjectDisposedException">Thrown if the store has been disposed.</exception>
    public async Task<object> DispatchAsync(IAction action)
    {
        ThrowIfDisposed();
        if (action == null) throw new ArgumentNullException(nameof(action));

        var index = -1;

        Task<object> Next(IAction? maybeAction)
        {
            index++;
            var effectiveAction = maybeAction ?? action;

            if (index < _middlewares.Count)
                return _middlewares[index](this, effectiveAction, Next);
            else
                return DispatchCore(effectiveAction);
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
    private async Task<object> DispatchCore(IAction action)
    {
        TState previousState;
        await _syncRoot.WaitAsync();
        try
        {
            previousState = _lastState;
            _history.Push(_lastState);
            _lastState = await _reducer(_lastState, action);
            _future.Clear();
        }
        finally
        {
            _syncRoot.Release();
        }

        // Only notify if state reference actually changed (reducer returned new state)
        if (!ReferenceEquals(previousState, _lastState))
        {
            StateChanged?.Invoke();

            if (StateChangedAsync != null)
            {
                var handlers = StateChangedAsync.GetInvocationList();
                foreach (Func<Task> handler in handlers)
                {
                    await handler();
                }
            }

            NotifyListeners();
        }

        return action;
    }

    /// <summary>
    /// Fire-and-forget convenience wrapper for UI event handlers that can't be async.
    /// Prefer <see cref="DispatchAsync"/> when you can await.
    /// </summary>
    /// <param name="action">The action to dispatch. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the store has been disposed.</exception>
    public void Dispatch(IAction action)
    {
        _ = DispatchAsync(action);
    }

    /// <summary>
    /// Reverts the state to the most recent previous state in the history.
    /// </summary>
    /// <remarks>This method restores the last saved state from the history stack and moves the current state
    /// to the future stack. If the history stack is empty, the method does nothing.</remarks>
    /// <exception cref="ObjectDisposedException">Thrown if the store has been disposed.</exception>
    public void Undo()
    {
        ThrowIfDisposed();
        if (_history.Count == 0) return;
        _future.Push(_lastState);
        _lastState = _history.Pop();
        StateChanged?.Invoke();
        NotifyListeners();
    }

    /// <summary>
    /// Reapplies the most recently undone state, restoring it as the current state.
    /// </summary>
    /// <remarks>This method moves the most recent state from the redo stack to the undo stack and updates the
    /// current state.  If there are no states available to redo, the method does nothing.</remarks>
    /// <exception cref="ObjectDisposedException">Thrown if the store has been disposed.</exception>
    public void Redo()
    {
        ThrowIfDisposed();
        if (_future.Count == 0) return;
        _history.Push(_lastState);
        _lastState = _future.Pop();
        StateChanged?.Invoke();
        NotifyListeners();
    }

    /// <summary>
    /// Adds a middleware handler to the store's pipeline.
    /// </summary>
    /// <param name="middleware">The middleware handler to add. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the store has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="middleware"/> is <see langword="null"/>.</exception>
    public void AddMiddleware(MiddlewareHandler<TState> middleware)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(middleware);
        _middlewares.Add(middleware);
    }

    /// <summary>
    /// Removes all middleware handlers from the store's pipeline.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the store has been disposed.</exception>
    public void ClearMiddlewares()
    {
        ThrowIfDisposed();
        _middlewares.Clear();
    }

    /// <summary>
    /// Gets the number of middleware handlers currently registered.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the store has been disposed.</exception>
    public int MiddlewareCount
    {
        get
        {
            ThrowIfDisposed();
            return _middlewares.Count;
        }
    }

    /// <summary>
    /// Subscribe to state changes. Returns an IDisposable to unsubscribe.
    /// </summary>
    /// <param name="listener">The callback to invoke when state changes. Cannot be <see langword="null"/>.</param>
    /// <returns>An IDisposable that will unsubscribe the listener when disposed.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the store has been disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="listener"/> is <see langword="null"/>.</exception>
    public IDisposable Subscribe(Action<TState> listener)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(listener);
        _listeners.Add(listener);
        return new Subscription(this, listener);
    }

    /// <summary>
    /// Unsubscribes a listener from state change notifications.
    /// </summary>
    /// <param name="listener">The listener to remove.</param>
    private void Unsubscribe(Action<TState> listener)
    {
        _listeners.Remove(listener);
    }

    /// <summary>
    /// Notifies all subscribed listeners of a state change.
    /// </summary>
    private void NotifyListeners()
    {
        // snapshot to avoid issues if listeners mutate during notification
        var snapshot = _listeners.ToArray();
        foreach (var listener in snapshot)
        {
            try { listener(_lastState); } catch { /* swallow listener exceptions */ }
        }
    }

    /// <summary>
    /// Represents a subscription to state changes that can be disposed to unsubscribe.
    /// </summary>
    private sealed class Subscription(Store<TState> store, Action<TState> listener) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            store.Unsubscribe(listener);
        }
    }

    /// <summary>
    /// Throws ObjectDisposedException if the store has been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the store has been disposed.</exception>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(Store<TState>));
        }
    }

    /// <summary>
    /// Disposes the store and releases its resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _syncRoot?.Dispose();
            _actions?.Dispose();
            _events?.Dispose();
            _disposed = true;
        }
    }
}