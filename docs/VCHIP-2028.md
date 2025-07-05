# Specification: VCHIP-2800 – LowlandTech.Redux

## 📄 Overview

LowlandTech.Redux is a .NET library that provides asynchronous, testable, Redux-inspired state management with undo/redo capabilities and middleware support.

This specification describes the goals, scenarios, and user acceptance criteria (UAC) for the library.

---

## 🎯 Objectives

- Provide predictable state management for .NET applications.
- Support asynchronous reducers to simulate real-world delays (e.g., network calls).
- Allow middleware pipelines for logging and side effects.
- Enable undo and redo of state changes.
- Support event notifications (`StateChanged` and `StateChangedAsync`).
- Facilitate test automation with BDD scenario annotations.

---

## 🧩 Scope

The scope includes:

- Core state store implementation.
- Reducer and middleware composition.
- Undo/Redo mechanics.
- State change notification events.
- Test infrastructure and specification bindings.

---

## 📘 Features & Scenarios

### Scenario VCHIP-2800-SC001
**Title:** Create a store with an initial state  
**Given:** A reducer function  
**And:** An initial state of 0  
**When:** A store is created  
**Then:** The store state should be 0  
**UAC:** VCHIP-2800-UAC001

---

### Scenario VCHIP-2800-SC002
**Title:** Dispatch an IncrementCounterAction and update state  
**Given:** A store with state 0  
**When:** Dispatching IncrementCounterAction with Amount 5  
**Then:** The store state should be 5  
**UAC:** VCHIP-2800-UAC002

---

### Scenario VCHIP-2800-SC003
**Title:** Dispatch multiple IncrementCounterAction instances  
**Given:** A store with state 0  
**When:** Dispatching IncrementCounterAction with Amount 2 and 3  
**Then:** The store state should be 5  
**UAC:** VCHIP-2800-UAC003

---

### Scenario VCHIP-2800-SC004
**Title:** Middleware logs before and after reducer execution  
**Given:** A store with the Logger middleware enabled  
**When:** Dispatching IncrementCounterAction  
**Then:** Middleware should log the action name and new state  
**UAC:** VCHIP-2800-UAC004

---

### Scenario VCHIP-2800-SC005
**Title:** Reducer is invoked asynchronously  
**Given:** A store with an asynchronous reducer  
**When:** Dispatching IncrementCounterAction  
**Then:** Reducer should await a delay before returning the new state  
**UAC:** VCHIP-2800-UAC005

---

### Scenario VCHIP-2800-SC006
**Title:** OnAfterDispatch handler is called after dispatch  
**Given:** A store with an OnAfterDispatch handler configured  
**When:** Dispatching IncrementCounterAction  
**Then:** The OnAfterDispatch handler should be invoked  
**UAC:** VCHIP-2800-UAC008

---

### Scenario VCHIP-2800-SC007
**Title:** StateChanged event fires synchronously after dispatch  
**Given:** A store with a StateChanged event handler  
**When:** Dispatching IncrementCounterAction  
**Then:** The StateChanged event should be triggered  
**UAC:** VCHIP-2800-UAC009

---

### Scenario VCHIP-2800-SC008
**Title:** StateChangedAsync event fires asynchronously after dispatch  
**Given:** A store with a StateChangedAsync event handler  
**When:** Dispatching IncrementCounterAction  
**Then:** The StateChangedAsync event should be triggered asynchronously  
**UAC:** VCHIP-2800-UAC010

---

### Scenario VCHIP-2800-SC009
**Title:** Undo reverts the last state change  
**Given:** A store with state 5  
**And:** An IncrementCounterAction dispatched  
**When:** Calling Undo  
**Then:** State should revert to 5  
**UAC:** VCHIP-2800-UAC006

---

### Scenario VCHIP-2800-SC010
**Title:** Redo re-applies the undone state  
**Given:** A store where Undo has been called  
**When:** Calling Redo  
**Then:** State should return to the state after the original dispatch  
**UAC:** VCHIP-2800-UAC007

---

### Scenario VCHIP-2800-SC011
**Title:** Select projects part of the state  
**Given:** A store with state 10  
**When:** Calling Select with a selector that doubles the state  
**Then:** Result should be 20  
**UAC:** VCHIP-2800-UAC011

---

## ✅ Acceptance Criteria

Each scenario will be validated by:
- **BDD scenario tests** annotated with `ScenarioAttribute` and `ThenAttribute`.
- Verification of state consistency (`GetState()`).
- Verification of side effects (e.g., event firing and handler invocation).
- Undo and redo correctness.
- Middleware behavior.

---

## 🧪 Test Automation

All acceptance tests are implemented in `LowlandTech.Redux.Tests`:
- `WhenDispatchingIncrement`
- `WhenUndoing`
- `WhenRedoing`
- `WhenUsingLoggerMiddleware`
- `WhenUsingOnAfterDispatch`
- `WhenStateChangedEventFires`
- `WhenStateChangedAsyncEventFires`
- `WhenSelectingState`

---

## 🛠️ Technical Notes

- Reducers must implement `ReducerAsync<TState>`.
- Middleware must implement `MiddlewareHandler<TState>`.
- Undo/Redo uses internal stacks (`_history` and `_future`).
- Event invocations must be thread-safe and awaitable.
- Scenarios rely on `xUnit` and `Shouldly` for assertions.

---

## 📅 Revision History

| Version      | Date       | Author            | Changes                              |
| ------------ | ---------- | ----------------- | ------------------------------------ |
| 2025.7.1     | 2025-07-05 | LowlandTech Team  | Initial specification for VCHIP-2800 |

---

