using LowlandTech.Flows.Domain.Entities;

namespace LowlandTech.Redux.Tests;

[Scenario(
    "VCHIP-2800-SC004",
    "Middleware logs before and after reducer execution",
    "Given a store with the Logger middleware enabled",
    "When I dispatch an IncrementCounterAction",
    "Then the middleware should log the action name",
    "And the middleware should log the new state after reducer execution")]
public class WhenUsingLoggerMiddleware : WhenTestingForAsync<Store<int>>
{
    private Store<int> _store = null!;

    protected override Store<int> For()
    {
        _store = new Store<int>(
            Reducers.CounterReducer,
            0,
            Middlewares.Logger);
        return _store;
    }

    protected override async Task WhenAsync()
    {
        await _store.DispatchAsync(new IncrementCounterAction(1));
    }

    [Fact]
    [Then("The store state should be 1", "VCHIP-2800-UAC004")]
    public void ItShouldApplyMiddleware()
    {
        _store.GetState().ShouldBe(1);
    }
}