namespace LowlandTech.Redux.Tests;

[Scenario(
    "VCHIP-2800-SC005",
    "Reducer is invoked asynchronously",
    "Given a store with an asynchronous reducer",
    "When I dispatch an IncrementCounterAction",
    "Then the reducer should await an artificial delay before returning the new state")]
public class WhenUsingAsyncReducer : WhenTestingForAsync<Store<int>>
{
    private Store<int> _store = null!;

    protected override Store<int> For()
    {
        _store = new Store<int>(
            Reducers.CounterReducer,
            0);
        return _store;
    }

    protected override async Task WhenAsync()
    {
        await _store.DispatchAsync(new IncrementCounterAction(3));
    }

    [Fact]
    [Then("The store state should be 3", "VCHIP-2800-UAC005")]
    public void ItShouldAwaitReducer()
    {
        _store.GetState().ShouldBe(3);
    }
}