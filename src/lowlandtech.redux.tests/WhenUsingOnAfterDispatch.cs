namespace LowlandTech.Redux.Tests;

[Scenario(
    "VCHIP-2800-SC006",
    "OnAfterDispatch handler is called after dispatch",
    "Given a store with an OnAfterDispatch handler configured",
    "When I dispatch an IncrementCounterAction",
    "Then the OnAfterDispatch handler should be invoked with the action and new state")]
public class WhenUsingOnAfterDispatch : WhenTestingForAsync<Store<int>>
{
    private Store<int> _store = null!;
    private bool _onAfterDispatchCalled;

    protected override Store<int> For()
    {
        _store = new Store<int>(Reducers.CounterReducer, 0);
        _store.OnAfterDispatch = (action, state) => _onAfterDispatchCalled = true;
        return _store;
    }

    protected override async Task WhenAsync()
    {
        await _store.DispatchAsync(new IncrementCounterAction(5));
    }

    [Fact]
    [Then("The OnAfterDispatch handler should be invoked", "VCHIP-2800-UAC008")]
    public void ItShouldInvokeOnAfterDispatch()
    {
        _onAfterDispatchCalled.ShouldBeTrue();
    }
}