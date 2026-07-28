using UnityEngine;
using UnityEngine.InputSystem;

public class GameStatePlay : BaseState<GameState>
{
    public override void OnEnter()
    {
        // Enable HUD
        GameManager.Instance.MenuManager.HUD.Enable();

        // Set time scale to 1
        Time.timeScale = 1;

        // We want to be able to load the in game menu from here.
        InputAction openMenuAction = GameManager.Instance.InputActions.FindAction("UI/OpenMenu");
        openMenuAction.performed += OnOpenMenuAction;
        openMenuAction.Enable();

        base.OnEnter();
    }

    /// <summary>
    /// Input Event.<br/>
    /// Called when performing the "OpenMenu" action.
    /// </summary>
    void OnOpenMenuAction(InputAction.CallbackContext obj)
    {
        // Self unbind this event...
        InputAction openMenuAction = GameManager.Instance.InputActions.FindAction("UI/OpenMenu");
        openMenuAction.performed -= OnOpenMenuAction;
        openMenuAction.Disable();

        // We now want to load up the in game menu, by transitioning to the menu state.
        StateMachine.SwitchState(GameState.MenuInGame);
    }
}
