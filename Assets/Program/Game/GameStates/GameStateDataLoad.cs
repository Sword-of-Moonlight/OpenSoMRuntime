using UnityEngine;

public class GameStateDataLoad : BaseState<GameState>
{
    public override string Name => "Data Load";

    public override void OnEnter()
    {
        GameManager.Instance.MenuManager.ShowSystemMessage("NOT IMPLEMENTED", OnSystemMessageComplete);

        base.OnEnter();
    }

    void OnSystemMessageComplete() =>
        StateMachine.SwitchState(GameState.MenuTitle);
}
