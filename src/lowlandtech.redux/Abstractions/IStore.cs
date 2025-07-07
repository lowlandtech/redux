namespace LowlandTech.Redux.Abstractions;

/// <summary>
/// Represents a store that manages state and provides mechanisms for dispatching actions,  selecting state, and
/// handling state changes.
/// </summary>
/// <typeparam name="TState">The type of the state managed by the store.</typeparam>
public interface IStore<out TState>
{
    /// <summary>
    /// Gets an observable sequence of events.
    /// </summary>
    /// <remarks>Subscribers to this property will receive notifications for each event in the sequence. 
    /// Ensure proper disposal of subscriptions to avoid memory leaks.</remarks>
    public IObservable<IEvent> Events { get; }

    /// <summary>
    /// Publishes the specified event to all subscribed observers.
    /// </summary>
    /// <param name="event">The event to be published. Cannot be <see langword="null"/>.</param>
    public void PublishEvent(IEvent @event);

    /// <summary>
    /// Gets an observable sequence of actions.
    /// </summary>
    /// <remarks>Subscribers to this observable will receive notifications for each action emitted.  Ensure
    /// proper subscription management to avoid memory leaks or unintended behavior.</remarks>
    public IObservable<IAction> Actions { get; }

    /// <summary>
    /// Dispatches the specified action asynchronously and returns the result of the operation.
    /// </summary>
    /// <remarks>The exact nature of the result depends on the implementation of the action being dispatched.
    /// Ensure that the action is properly configured before calling this method.</remarks>
    /// <param name="action">The action to be dispatched. This parameter cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task's result contains the outcome of the dispatched
    /// action.</returns>
    Task<object> DispatchAsync(IAction action);

    /// <summary>
    /// Retrieves the current state of the object.
    /// </summary>
    /// <returns>The current state of the object as an instance of <typeparamref name="TState"/>.</returns>
    TState GetState();

    /// <summary>
    /// Projects the current state into a new form by applying the specified selector function.
    /// </summary>
    /// <typeparam name="TResult">The type of the value returned by the selector function.</typeparam>
    /// <param name="selector">A function that transforms the current state into a value of type <typeparamref name="TResult"/>.</param>
    /// <returns>The result of applying the <paramref name="selector"/> function to the current state.</returns>
    TResult Select<TResult>(Func<TState, TResult> selector);

    /// <summary>
    /// Occurs when the state of the object changes.
    /// </summary>
    /// <remarks>Subscribe to this event to be notified whenever the state of the object changes.  The event
    /// handler will be invoked with no parameters.</remarks>
    event Action StateChanged;

    /// <summary>
    /// Occurs when the state of the object changes asynchronously.
    /// </summary>
    /// <remarks>Subscribers to this event should provide a delegate that returns a <see cref="Task"/>. The
    /// event is triggered when the state changes, allowing subscribers to perform asynchronous operations in
    /// response.</remarks>
    event Func<Task> StateChangedAsync;

    /// <summary>
    /// Reverts the last operation performed, restoring the state to what it was before the operation.
    /// </summary>
    /// <remarks>This method is typically used to implement undo functionality in applications where actions
    /// can be reversed. If no operation has been performed, calling this method may have no effect.</remarks>
    void Undo();

    /// <summary>
    /// Reapplies the most recently undone operation, if available.
    /// </summary>
    /// <remarks>This method restores the state to what it was before the last undo operation. If there are no
    /// operations to redo, calling this method has no effect.</remarks>
    void Redo();
}