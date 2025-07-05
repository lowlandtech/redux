namespace LowlandTech.Redux.Infrastructure.Handlers;

/// <summary>
/// Provides predefined middleware for handling actions and state in a store.
/// </summary>
/// <remarks>Middleware is a mechanism for intercepting and processing actions dispatched to a store.  The <see
/// cref="Logger"/> middleware logs the type of the dispatched action and the resulting state  after the action is
/// processed. This can be useful for debugging or monitoring state changes.</remarks>
public static class Middlewares
{
    /// <summary>
    /// A middleware handler that logs the dispatched action and the resulting state.
    /// </summary>
    /// <remarks>This middleware logs the type of the action being dispatched and the new state of the store
    /// after the action is processed. It can be used to monitor the flow of actions and state changes in the
    /// application.</remarks>
    public static MiddlewareHandler<int> Logger = async (store, action, next) =>
    {
        Console.WriteLine($"[Middleware] Dispatching {action.GetType().Name}");
        var result = await next();
        Console.WriteLine($"[Middleware] New State: {store.GetState()}");
        return result;
    };
}