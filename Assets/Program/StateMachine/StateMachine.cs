using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T> where T : Enum
{
    // The statemap stores all possible states the player can be in
    readonly Dictionary<T, BaseState<T>> stateMap;

    // The current state is the... current state :)
    protected BaseState<T> currentState;

    // The next state is the state the state machine will transition to after finishing the current tick.
    protected BaseState<T> nextState = null;

    // The last state is the state the state machine was previously in.
    protected BaseState<T> lastState = null;

    // Properties
    public bool VerboseLogging { get; set; } = false;

    public BaseState<T> LastState    => lastState;

    public BaseState<T> CurrentState => currentState;

    /// <summary>
    /// Default Constructor.
    /// </summary>
    public StateMachine(bool verboseLogging)
    {
        // Create state map.
        stateMap = new Dictionary<T, BaseState<T>>();

        VerboseLogging = verboseLogging;
    }

    /// <summary>
    /// Registers a state with the state machine
    /// </summary>
    /// <param name="stateKey">The key to register the state as</param>
    /// <param name="state">The state object</param>
    /// <param name="makeCurrent">If the state should be forced as current.</param>
    public void Register(T stateKey, BaseState<T> state, bool makeCurrent = false)
    {
        if (stateMap.ContainsKey(stateKey))
            throw new Exception($"State Map already contains a definition for {stateKey}!");

        stateMap[stateKey] = state ?? throw new Exception($"Cannot register null state for {stateKey}");

        if (makeCurrent)
            nextState = stateMap[stateKey];

        state.OnRegister(this, stateKey);
    }

    /// <summary>
    /// Updates the statemachine, changes state if requested...
    /// </summary>
    public void Tick()
    {
        // If the current state is null, we might be setting the first state...
        if(currentState == null)
        {
            if (nextState == null)
                throw new Exception("State Machine has no current or next state!");

            // We must be setting the initial state.
            currentState = nextState;
            nextState    = null;

            // Run on enter for the initial state.
            currentState.OnEnter();
        }

        // Tick the current state, assuming it exits...
        currentState.OnTick();

        // Was a new state requested?
        if(nextState != null)
        {
            // Switch states
            lastState    = currentState;
            currentState = null;

            // Run exit for the last state, if it was valid
            lastState?.OnExit();
        }
    }

    /// <summary>
    /// Forces the state machine to reset to default...
    /// </summary>
    public void Restart(T initialState)
    {
        currentState = null;
        lastState    = null;
        nextState    = null;

        SwitchState(initialState);
    }

    /// <summary>
    /// Checks to see if a state with the given key exists.
    /// </summary>
    /// <param name="stateKey">The key of the state we're checking for</param>
    /// <returns>True if the state exists, False otherwise</inheritdoc>/></returns>
    public bool StateExists(T stateKey) =>
        stateMap.ContainsKey(stateKey);

    /// <summary>
    /// Prepares to switch to the next state, but will not perform it until the current tick has completed.
    /// </summary>
    /// <param name="stateKey">The key of the state we want to switch to</param>
    public void SwitchState(T to)
    {
        // Only allow switching to the next state if we don't already have a next state set up.
        if (nextState != null)
            Logger.Warn("Cancelled State Switch: Next State already set.");
        else
            nextState = stateMap[to];
    }   
}
