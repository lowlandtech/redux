using LowlandTech.Redux;
using LowlandTech.Redux.Actions;
using LowlandTech.Redux.Infrastructure;
using LowlandTech.Redux.Infrastructure.Handlers;

var store = new Store<int>(
    Reducers.CounterReducer,
    initialState: 0,
    Middlewares.Logger)
{
    OnAfterDispatch = (action, state) =>
        Console.WriteLine($"[AfterDispatch] Action {action.GetType().Name} applied, state is {state}")
};

store.StateChanged += () =>
    Console.WriteLine("[Event] State changed.");

store.StateChangedAsync += async() =>
{
    await Task.Delay(5);
    Console.WriteLine("[AsyncEvent] State changed (async).");
};

Console.WriteLine($"Initial State: {store.GetState()}");

await store.DispatchAsync(new IncrementCounterAction(5));

Console.WriteLine($"Current State: {store.GetState()}");

store.Undo();
Console.WriteLine($"After Undo: {store.GetState()}");

store.Redo();
Console.WriteLine($"After Redo: {store.GetState()}");