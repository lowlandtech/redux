namespace LowlandTech.Redux.Infrastructure;

/// <summary>
/// Provides a collection of reducer methods for managing state transitions in a Redux-like pattern.
/// </summary>
/// <remarks>Reducers are responsible for determining how the state should change in response to actions. Each
/// reducer takes the current state and an action as input and returns the new state.</remarks>
public static class Reducers
{
    /// <summary>
    /// Processes an action to update the current state of a counter.
    /// </summary>
    /// <param name="state">The current state of the counter.</param>
    /// <param name="action">The action to be applied to the counter. Must implement <see cref="IAction"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated counter state after
    /// applying the action. If the action is not recognized, the original state is returned.</returns>
    public static async Task<int> CounterReducer(int state, IAction action)
    {
        await Task.Delay(10); // Simulate async work
        return action switch
        {
            IncrementCounterAction inc => state + inc.Amount,
            _ => state
        };
    }
}