namespace LowlandTech.Redux.Infrastructure.Services;

/// <summary>
/// Represents an asynchronous function that computes a new state based on the current state and an action.
/// </summary>
/// <typeparam name="TState">The type of the state object.</typeparam>
/// <param name="state">The current state. This parameter cannot be null.</param>
/// <param name="action">The action to be applied to the state. This parameter cannot be null.</param>
/// <returns>A task that represents the asynchronous operation. The task result contains the new state after the action is
/// applied.</returns>
public delegate Task<TState> ReducerAsync<TState>(TState state, IAction action);