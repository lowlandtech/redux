namespace LowlandTech.Redux.Tests;

[Scenario(
    "VCHIP-2800-SC001",
    "Create a store with an initial state",
    "Given I have a reducer function",
    "And I provide an initial state of 0",
    "When I create a store",
    "Then the store state should be 0")]
public class WhenCreatingStore : WhenTestingForAsync<Store<int>>
{
    private Store<int> _store = null!;

    protected override Store<int> For()
    {
        _store = new Store<int>(
            Reducers.CounterReducer,
            initialState: 0);
        return _store;
    }

    [Fact]
    [Then("The store state should be 0", "VCHIP-2800-UAC001")]
    public void ItShouldHaveInitialState()
    {
        _store.GetState().ShouldBe(0);
    }
}