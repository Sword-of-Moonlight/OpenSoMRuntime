using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateSplash : BaseState<GameState>
{
    public override string Name => "Splash";

    // We will store a reference to our title menu here.
    MenuSplash splashMenu;

    public override void OnEnter()
    {
        // Open the title menu...
        splashMenu = (GameManager.Instance.MenuManager.OpenMenu("Splash") as MenuSplash);
        splashMenu.DisplayComplete += OnDisplayComplete;

        base.OnEnter();
    }

    public override void OnExit()
    {
        // We use the exit override to unbind our events
        splashMenu.DisplayComplete -= OnDisplayComplete;

        GameManager.Instance.MenuManager.CloseMenu();
        splashMenu = null;

        base.OnExit();
    }

    /// <summary>
    /// Event Callback.<br/>
    /// Called after display of the splash has completed
    /// </summary>
    void OnDisplayComplete() =>
        StateMachine.SwitchState(GameState.SequenceTitle);
}
