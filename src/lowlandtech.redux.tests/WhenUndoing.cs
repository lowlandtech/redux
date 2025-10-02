namespace LowlandTech.Redux.Tests;

[Scenario(
    "VCHIP-2800-SC009",
    "Undo reverts the last state change",
    "Given a store with state 5",
    "And I have dispatched an IncrementCounterAction",
    "When I call Undo",
    "Then the state should revert to 5")]
public class WhenUndoing : WhenTestingForAsync<Store<int>>
{
    private Store<int> _store = null!;

    protected override Store<int> For()
    {
        _store = new Store<int>(Reducers.CounterReducer, 5);
        return _store;
    }

    protected override async Task WhenAsync()
    {
        await _store.DispatchAsync(new IncrementCounterAction(5));
        _store.Undo();
    }

    [Fact]
    [Then("The store state should be 5", "VCHIP-2800-UAC006")]
    public void ItShouldUndoState()
    {
        _store.GetState().ShouldBe(5);
    }
}