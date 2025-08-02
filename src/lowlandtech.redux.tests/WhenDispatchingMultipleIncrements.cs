using LowlandTech.Flows.Domain.Entities;

namespace LowlandTech.Redux.Tests;

[Scenario(
    "VCHIP-2800-SC003",
    "Dispatch multiple IncrementCounterAction instances",
    "Given a store with state 0",
    "When I dispatch IncrementCounterAction with Amount 2",
    "And I dispatch IncrementCounterAction with Amount 3",
    "Then the store state should be 5")]
public class WhenDispatchingMultipleIncrements : WhenTestingForAsync<Store<int>>
{
    private Store<int> _store = null!;

    protected override Store<int> For()
    {
        _store = new Store<int>(Reducers.CounterReducer, 0);
        return _store;
    }

    protected override async Task WhenAsync()
    {
        await _store.DispatchAsync(new IncrementCounterAction(2));
        await _store.DispatchAsync(new IncrementCounterAction(3));
    }

    [Fact]
    [Then("The store state should be 5", "VCHIP-2800-UAC003")]
    public void ItShouldAccumulateState()
    {
        _store.GetState().ShouldBe(5);
    }
}