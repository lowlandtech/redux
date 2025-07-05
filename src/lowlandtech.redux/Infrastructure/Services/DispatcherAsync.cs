namespace LowlandTech.Redux.Infrastructure.Services;

/// <summary>
/// Represents a delegate that asynchronously dispatches an action and returns a result.
/// </summary>
/// <param name="action">The action to be dispatched. Must not be <see langword="null"/>.</param>
/// <returns>A task that represents the asynchronous operation. The task's result is an object containing the outcome of the
/// dispatched action.</returns>
public delegate Task<object> DispatcherAsync(IAction action);