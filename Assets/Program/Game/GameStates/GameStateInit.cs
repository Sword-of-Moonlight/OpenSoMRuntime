using UnityEngine;

public class GameStateInit : BaseState<GameState>
{
    public override string Name => "Init";

    public override void OnEnter()
    {
        // Switch to sequence...
        if (GameManager.Instance.StartGameInstantly)
            StateMachine.SwitchState(GameState.PlayBegin);
        else
            StateMachine.SwitchState(GameState.MenuSplash);

        base.OnEnter();
    }
}
