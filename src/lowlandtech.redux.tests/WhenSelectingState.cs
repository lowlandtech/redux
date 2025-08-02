using LowlandTech.Flows.Domain.Entities;

namespace LowlandTech.Redux.Tests;

[Scenario(
    "VCHIP-2800-SC011",
    "Select projects part of the state",
    "Given a store with state 10",
    "When I call Select with a selector that doubles the state",
    "Then the result should be 20")]
public class WhenSelectingState : WhenTestingForAsync<Store<int>>
{
    private Store<int> _store = null!;
    private int _selectedResult;

    protected override Store<int> For()
    {
        _store = new Store<int>(Reducers.CounterReducer, 10);
        return _store;
    }

    protected override Task WhenAsync()
    {
        _selectedResult = _store.Select(s => s * 2);
        return Task.CompletedTask;
    }

    [Fact]
    [Then("The selected result should be 20", "VCHIP-2800-UAC011")]
    public void ItShouldSelectProjectedValue()
    {
        _selectedResult.ShouldBe(20);
    }
}