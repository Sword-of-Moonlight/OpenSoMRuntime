using System;

public abstract class BaseState<T> where T : Enum
{
    StateMachine<T> stateMachine;

    /// <summary>
    /// Retrieve the parent state machine.
    /// </summary>
    public StateMachine<T> StateMachine => stateMachine;

    /// <summary>
    /// Retrives the state name, OR a default hash code if it was not overriden
    /// </summary>
    public virtual string Name => $"{nameof(T)} NoName!";

    public T StateKey { get; private set; }

    /// <summary>
    /// Executed when the state is registed with a state machine.
    /// </summary>
    /// <param name="stateMachine">The statemachine the state was registered with.</param>
    public void OnRegister(StateMachine<T> stateMachine, T stateKey)
    {
        this.stateMachine = stateMachine;
        StateKey = stateKey;
    }  

    /// <summary>
    /// Executed when the state is entered.
    /// </summary>
    public virtual void OnEnter()
    {
        #if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        if(stateMachine.VerboseLogging)
            Logger.Info($"{Name}::OnEnter()");
        #endif

        return;
    }

    /// <summary>
    /// Executed when the state is exited.
    /// </summary>
    public virtual void OnExit()
    {
        #if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        if (stateMachine.VerboseLogging)
            Logger.Info($"{Name}::OnExit()");
        #endif

        return;
    }

    /// <summary>
    /// Executed each time the state machine is ticked.
    /// </summary>
    public virtual void OnTick()
    {
        #if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        if (stateMachine.VerboseLogging)
            Logger.Info($"{Name}::OnTick()");
        #endif

        return;
    }
}
