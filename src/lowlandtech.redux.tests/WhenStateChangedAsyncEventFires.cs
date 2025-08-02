using LowlandTech.Flows.Domain.Entities;

namespace LowlandTech.Redux.Tests;

[Scenario(
    "VCHIP-2800-SC008",
    "StateChangedAsync event fires asynchronously after dispatch",
    "Given a store with a StateChangedAsync event handler",
    "When I dispatch an IncrementCounterAction",
    "Then the StateChangedAsync event should be triggered asynchronously")]
public class WhenStateChangedAsyncEventFires : WhenTestingForAsync<Store<int>>
{
    private Store<int> _store = null!;
    private bool _stateChangedAsyncFired;

    protected override Store<int> For()
    {
        _store = new Store<int>(Reducers.CounterReducer, 0);
        _store.StateChangedAsync += async () =>
        {
            await Task.Delay(1);
            _stateChangedAsyncFired = true;
        };
        return _store;
    }

    protected override async Task WhenAsync()
    {
        await _store.DispatchAsync(new IncrementCounterAction(3));
        // Give the async event time to complete
        await Task.Delay(10);
    }

    [Fact]
    [Then("The StateChangedAsync event should be triggered", "VCHIP-2800-UAC010")]
    public void ItShouldFireStateChangedAsync()
    {
        _stateChangedAsyncFired.ShouldBeTrue();
    }
}