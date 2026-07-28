using UnityEngine;

public class GameStateMenuInGame : BaseState<GameState>
{
    public override string Name => "MenuInGame";

    MenuInGame gameMenu;

    public override void OnEnter()
    {
        // Must disable time scale inside the menu...
        // TO-DO: A no pause menu might be an interesting setting for people from Dark Souls...
        Time.timeScale = 0;

        // Open menu...
        gameMenu = (GameManager.Instance.MenuManager.OpenMenu("InGame") as MenuInGame);
    }
}
