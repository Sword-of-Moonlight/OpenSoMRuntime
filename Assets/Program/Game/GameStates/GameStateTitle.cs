using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateTitle : BaseState<GameState>
{
    public override string Name => "Title";

    // We will store a reference to our title menu here.
    MenuTitle titleMenu;

    public override void OnEnter()
    {
        // Open the title menu...
        titleMenu = (GameManager.Instance.MenuManager.OpenMenu("Title") as MenuTitle);
        titleMenu.NewGame      += OnNewGame;
        titleMenu.ContinueGame += OnContinueGame;

        base.OnEnter();
    }

    public override void OnExit()
    {
        // We use the exit override to unbind our events
        titleMenu.NewGame      -= OnNewGame;
        titleMenu.ContinueGame -= OnContinueGame;

        GameManager.Instance.MenuManager.CloseMenu();
        titleMenu = null;

        base.OnExit();
    }

    /// <summary>
    /// Event Callback.<br/>
    /// Handles starting a new game
    /// </summary>
    void OnNewGame() =>
        StateMachine.SwitchState(GameState.SequenceOpen);


    /// <summary>
    /// Event Callback.<br/>
    /// Handles opening the load save data menu (TO-DO)
    /// </summary>
    void OnContinueGame() =>
        StateMachine.SwitchState(GameState.DataLoad);
}
