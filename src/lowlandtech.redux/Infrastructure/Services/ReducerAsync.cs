namespace LowlandTech.Redux.Infrastructure.Services;

public delegate Task<TState> ReducerAsync<TState>(TState state, IAction action);