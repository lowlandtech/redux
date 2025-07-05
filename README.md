# LowlandTech.Redux

**LowlandTech.Redux** is an asynchronous, testable Redux-inspired state management library for .NET, supporting:
- Async reducers with simulated delays
- Middleware pipelines
- Undo/Redo functionality
- Event-driven state updates

---

## ? Features

- Asynchronous `ReducerAsync<TState>`
- Middleware support (`MiddlewareHandler<TState>`)
- `Undo()` and `Redo()` operations
- `StateChanged` and `StateChangedAsync` events
- `OnAfterDispatch` handler for custom post-processing
- BDD-style scenario and acceptance tests

---

## ?? Use Cases

### 1?? Async Reducer and Middleware

Create a store with an async reducer and logging middleware:

```csharp
var store = new Store<int>(
    Reducers.CounterReducer,
    initialState: 0,
    Middlewares.Logger
);

store.OnAfterDispatch = (action, state) =>
    Console.WriteLine($"[AfterDispatch] Action {action.GetType().Name} applied, state is {state}");

store.StateChanged += () =>
    Console.WriteLine("[Event] State changed.");

store.StateChangedAsync += async () =>
{
    await Task.Delay(5);
    Console.WriteLine("[AsyncEvent] State changed (async).");
};

await store.DispatchAsync(new IncrementCounterAction(5));

Console.WriteLine($"Current State: {store.GetState()}");
```

**Expected Output:**

```
[Middleware] Dispatching IncrementCounterAction
[Middleware] New State: 5
[AfterDispatch] Action IncrementCounterAction applied, state is 5
[Event] State changed.
[AsyncEvent] State changed (async).
Current State: 5
```

? **Test Example:**

```csharp
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
        _store = new Store<int>(Reducers.CounterReducer, 0, Middlewares.Logger);
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
```

---

### 2?? Undo/Redo State Management

Track history and revert or re-apply state changes:

```csharp
var store = new Store<int>(Reducers.CounterReducer, 0);

await store.DispatchAsync(new IncrementCounterAction(10));
Console.WriteLine($"After Dispatch: {store.GetState()}"); // 10

store.Undo();
Console.WriteLine($"After Undo: {store.GetState()}"); // 0

store.Redo();
Console.WriteLine($"After Redo: {store.GetState()}"); // 10
```

? **Test Example:**

```csharp
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
```

---

## ?? Running Tests

The solution uses **xUnit** and custom `ScenarioAttribute` / `ThenAttribute` for BDD-style test reporting.

Run all tests:

```bash
dotnet test
```

---

## ?? Project Structure

- `LowlandTech.Redux` - Core library
- `LowlandTech.Redux.Tests` - BDD scenario tests
- `LowlandTech.Testing.Features` - BDD test attributes

---

## ?? License

MIT License.  
(c) LowlandTech Contributors.

