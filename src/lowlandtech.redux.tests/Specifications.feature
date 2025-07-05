Feature: Redux Store and Middleware
  As a developer
  I want to use an asynchronous Redux store with middleware, reducers, and undo/redo
  So that I can manage application state in a predictable and testable way

  @VCHIP-2800-SC001
  Scenario: Create a store with an initial state
    Given I have a reducer function
    And I provide an initial state of 0
    When I create a store
    Then the store state should be 0

  @VCHIP-2800-SC002
  Scenario: Dispatch an IncrementCounterAction and update state
    Given a store with state 0
    When I dispatch an IncrementCounterAction with Amount 5
    Then the store state should be 5

  @VCHIP-2800-SC003
  Scenario: Dispatch multiple IncrementCounterAction instances
    Given a store with state 0
    When I dispatch IncrementCounterAction with Amount 2
    And I dispatch IncrementCounterAction with Amount 3
    Then the store state should be 5

  @VCHIP-2800-SC004
  Scenario: Middleware logs before and after reducer execution
    Given a store with the Logger middleware enabled
    When I dispatch an IncrementCounterAction
    Then the middleware should log the action name
    And the middleware should log the new state after reducer execution

  @VCHIP-2800-SC005
  Scenario: Reducer is invoked asynchronously
    Given a store with an asynchronous reducer
    When I dispatch an IncrementCounterAction
    Then the reducer should await an artificial delay before returning the new state

  @VCHIP-2800-SC006
  Scenario: OnAfterDispatch handler is called after dispatch
    Given a store with an OnAfterDispatch handler configured
    When I dispatch an IncrementCounterAction
    Then the OnAfterDispatch handler should be invoked with the action and new state

  @VCHIP-2800-SC007
  Scenario: StateChanged event fires synchronously after dispatch
    Given a store with a StateChanged event handler
    When I dispatch an IncrementCounterAction
    Then the StateChanged event should be triggered

  @VCHIP-2800-SC008
  Scenario: StateChangedAsync event fires asynchronously after dispatch
    Given a store with a StateChangedAsync event handler
    When I dispatch an IncrementCounterAction
    Then the StateChangedAsync event should be triggered asynchronously

  @VCHIP-2800-SC009
  Scenario: Undo reverts the last state change
    Given a store with state 5
    And I have dispatched an IncrementCounterAction
    When I call Undo
    Then the state should revert to 5

  @VCHIP-2800-SC010
  Scenario: Redo re-applies the undone state
    Given a store where Undo has been called
    When I call Redo
    Then the state should return to the state after the original dispatch

  @VCHIP-2800-SC011
  Scenario: Select projects part of the state
    Given a store with state 10
    When I call Select with a selector that doubles the state
    Then the result should be 20
