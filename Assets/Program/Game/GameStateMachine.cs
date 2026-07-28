using UnityEngine;

public enum GameState : uint
{
    // Critical States
    Init = 0x00000000,

    // Sequence States
    SequenceTitle     = 0x00000010,
    SequenceOpen      = 0x00000011,
    SequenceGameEndA  = 0x00000012,
    SequenceGameEndB  = 0x00000013,
    SequenceGameEndC  = 0x00000014,
    SequenceStaffRoll = 0x00000015,

    // Menu States
    MenuSplash = 0x00000040,
    MenuTitle  = 0x00000041,
    MenuInGame = 0x00000042,

    // Data States
    DataLoad   = 0x00000080,

    // Play States
    PlayBegin = 0x00001000,
    Play      = 0x00001001,
}

public class GameStateMachine : StateMachine<GameState>
{
    public GameStateMachine() : base(false)
    {
        // State Registration
        Register(GameState.Init, new GameStateInit(), true);
        Register(GameState.SequenceTitle, new GameStateSequence(SoMSequenceType.Title, GameState.MenuTitle));
        Register(GameState.SequenceOpen, new GameStateSequence(SoMSequenceType.Opening, GameState.PlayBegin));

        Register(GameState.MenuSplash, new GameStateSplash());
        Register(GameState.MenuTitle, new GameStateTitle());
        Register(GameState.MenuInGame, new GameStateMenuInGame());

        Register(GameState.DataLoad, new GameStateDataLoad());

        Register(GameState.PlayBegin, new GameStatePlayBegin());
        Register(GameState.Play, new GameStatePlay());
    }
}
