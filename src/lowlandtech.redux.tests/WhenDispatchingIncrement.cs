namespace LowlandTech.Redux.Tests;

[Scenario(
    "VCHIP-2800-SC002",
    "Dispatch an IncrementCounterAction and update state",
    "Given a store with state 0",
    "When I dispatch an IncrementCounterAction with Amount 5",
    "Then the store state should be 5")]
public class WhenDispatchingIncrement : WhenTestingForAsync<Store<int>>
{
    private Store<int> _store = null!;

    protected override Store<int> For()
    {
        _store = new Store<int>(Reducers.CounterReducer, 0);
        return _store;
    }

    protected override async Task WhenAsync()
    {
        await _store.DispatchAsync(new IncrementCounterAction(5));
    }

    [Fact]
    [Then("The store state should be 5", "VCHIP-2800-UAC002")]
    public void ItShouldUpdateState()
    {
        _store.GetState().ShouldBe(5);
    }
}