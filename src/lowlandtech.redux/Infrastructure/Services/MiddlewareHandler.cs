namespace LowlandTech.Redux.Infrastructure.Services;

/// <summary>
/// Represents a delegate that processes middleware logic in a state management pipeline.
/// </summary>
/// <remarks>Middleware can use this delegate to inspect, modify, or short-circuit the processing of actions in
/// the pipeline. To continue processing, the middleware should call the <paramref name="next"/> function.</remarks>
/// <typeparam name="TState">The type of the state managed by the store.</typeparam>
/// <param name="store">The store that provides access to the current state and dispatching actions.</param>
/// <param name="action">The action being dispatched through the middleware pipeline.</param>
/// <param name="next">A function that invokes the next middleware in the pipeline.  The returned task represents the result of the next
/// middleware or the final result if this is the last middleware.</param>
/// <returns>A task that represents the asynchronous operation. The task's result is the output of the middleware or the final
/// result of the pipeline.</returns>
public delegate Task<object> MiddlewareHandler<in TState>(
    IStore<TState> store,
    IAction action,
    Func<Task<object>> next);