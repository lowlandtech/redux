namespace LowlandTech.Redux.Tests;

[Scenario(
    "VCHIP-2800-SC010",
    "Redo re-applies the undone state",
    "Given a store where Undo has been called",
    "When I call Redo",
    "Then the state should return to the state after the original dispatch")]
public class WhenRedoing : WhenTestingForAsync<Store<int>>
{
    private Store<int> _store = null!;

    protected override Store<int> For()
    {
        _store = new Store<int>(Reducers.CounterReducer, 0);
        return _store;
    }

    protected override async Task WhenAsync()
    {
        await _store.DispatchAsync(new IncrementCounterAction(10));
        _store.Undo();
        _store.Redo();
    }

    [Fact]
    [Then("The store state should be 10", "VCHIP-2800-UAC007")]
    public void ItShouldRedoState()
    {
        _store.GetState().ShouldBe(10);
    }
}