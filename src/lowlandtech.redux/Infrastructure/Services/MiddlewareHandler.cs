namespace LowlandTech.Redux.Infrastructure.Services;

public delegate Task<object> MiddlewareHandler<TState>(
    IStore<TState> store,
    IAction action,
    Func<Task<object>> next);