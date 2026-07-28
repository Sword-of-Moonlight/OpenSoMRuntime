using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

using System;

public class GameStateSequence : BaseState<GameState>
{
    public override string Name => "Sequence";

    readonly SoMSequenceType sequenceType;
    readonly GameState nextState;

    IDisposable anyButtonSubscription;

    // We will store a reference to our sequence menu here.
    MenuSequence sequenceMenu;

    /// <summary>
    /// Override Constructor. </b>
    /// </summary>
    public GameStateSequence(SoMSequenceType sequenceType, GameState nextState) : base()
    {
        this.sequenceType = sequenceType;
        this.nextState    = nextState;
    }

    public override void OnEnter()
    {
        // If the sequence is invalid, return to the next state...
        if (GameManager.Instance.ProjectData.sequences[(int)sequenceType].mode == SoMSequenceMode.None)
        {
            StateMachine.SwitchState(nextState);
            return;
        }

        // Open the title menu...
        sequenceMenu = (GameManager.Instance.MenuManager.OpenMenu("Sequence") as MenuSequence);
        sequenceMenu.SequenceComplete += OnSequenceCompleted;

        // We want to listen for any key to skip the sequence
        anyButtonSubscription = InputSystem.onAnyButtonPress.Call(OnAnyButtonPressed);

        // We now play the sequence with the sequence menu
        sequenceMenu.PlaySequence(GameManager.Instance.ProjectData.sequences[(int)sequenceType]);

        base.OnEnter();
    }

    public override void OnExit()
    {
        // We use the exit override to unbind our events
        sequenceMenu.SequenceComplete -= OnSequenceCompleted;

        // Close the menu
        GameManager.Instance.MenuManager.CloseMenu();
        sequenceMenu = null;

        base.OnExit();
    }
    /// <summary>
    /// Event Callback.<br/>
    /// Called when the sequence finishes.
    /// </summary>
    void OnSequenceCompleted()
    {
        // Dispose our button subscription
        anyButtonSubscription?.Dispose();

        // Move to next state
        StateMachine.SwitchState(nextState);
    }

    /// <summary>
    /// Input Callback.<br/>
    /// Listens for any button being pressed.
    /// </summary>
    void OnAnyButtonPressed(InputControl ctrl)
    {
        // Stop the sequence
        sequenceMenu.StopSequence();

        // Dispose our button subscription
        anyButtonSubscription?.Dispose();
    }
}
