namespace LowlandTech.Redux.Abstractions;

/// <summary>
/// Defines a handler that processes an action within a store and optionally delegates further processing.
/// </summary>
/// <typeparam name="TAction">The type of the action to be handled. Must implement <see cref="IAction"/>.</typeparam>
/// <typeparam name="TState">The type of the state managed by the store.</typeparam>
public interface IHandler<in TAction, in TState>
    where TAction : IAction
{
    /// <summary>
    /// Handles the specified action by processing it with the given state store and invoking the next middleware in the
    /// pipeline.
    /// </summary>
    /// <remarks>This method is typically used in middleware pipelines to process actions in a sequential
    /// manner.  Implementations may modify the state, perform side effects, or short-circuit the pipeline by not
    /// invoking <paramref name="next"/>.</remarks>
    /// <param name="store">The state store used to retrieve or update the current state during the action handling process.</param>
    /// <param name="action">The action to be handled. This represents the operation or event to process.</param>
    /// <param name="next">A delegate that invokes the next middleware in the pipeline.  This function must be called to pass control to
    /// subsequent handlers.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the outcome of the action handling, 
    /// which may vary depending on the middleware implementation.</returns>
    Task<object> HandleAsync(
        IStore<TState> store,
        TAction action,
        Func<Task<object>> next
    );
}