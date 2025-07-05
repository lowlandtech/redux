namespace LowlandTech.Redux.Tests;

[Scenario(
    "VCHIP-2800-SC007",
    "StateChanged event fires synchronously after dispatch",
    "Given a store with a StateChanged event handler",
    "When I dispatch an IncrementCounterAction",
    "Then the StateChanged event should be triggered")]
public class WhenStateChangedEventFires : WhenTestingForAsync<Store<int>>
{
    private Store<int> _store = null!;
    private bool _stateChangedFired;

    protected override Store<int> For()
    {
        _store = new Store<int>(Reducers.CounterReducer, 0);
        _store.StateChanged += () => _stateChangedFired = true;
        return _store;
    }

    protected override async Task WhenAsync()
    {
        await _store.DispatchAsync(new IncrementCounterAction(2));
    }

    [Fact]
    [Then("The StateChanged event should be triggered", "VCHIP-2800-UAC009")]
    public void ItShouldFireStateChanged()
    {
        _stateChangedFired.ShouldBeTrue();
    }
}