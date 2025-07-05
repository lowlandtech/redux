using LowlandTech.Redux.Infrastructure.Services;

namespace LowlandTech.Redux.Infrastructure.Handlers;

public static class Middlewares
{
    public static MiddlewareHandler<int> Logger = async (store, action, next) =>
    {
        Console.WriteLine($"[Middleware] Dispatching {action.GetType().Name}");
        var result = await next();
        Console.WriteLine($"[Middleware] New State: {store.GetState()}");
        return result;
    };
}