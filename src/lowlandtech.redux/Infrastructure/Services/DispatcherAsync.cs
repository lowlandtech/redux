namespace LowlandTech.Redux.Infrastructure.Services;

public delegate Task<object> DispatcherAsync(IAction action);