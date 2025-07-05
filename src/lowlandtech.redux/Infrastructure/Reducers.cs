namespace LowlandTech.Redux.Infrastructure;

public static class Reducers
{
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