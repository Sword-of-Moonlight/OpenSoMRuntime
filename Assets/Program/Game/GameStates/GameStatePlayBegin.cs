using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStatePlayBegin : BaseState<GameState>
{
    public override string Name => "PlayBegin";

    public override void OnEnter()
    {
        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync("SCNMapStub", LoadSceneMode.Additive);
        sceneLoadOperation.completed += OnSceneLoadCompleted;
    }

    void OnSceneLoadCompleted(AsyncOperation obj)
    {
        // Probably initial player set up...

        // Load the initial map
        MapController.Instance.LoadMap(GameManager.Instance.ProjectData.initialMap);

        // READY FOR PLAY!
        StateMachine.SwitchState(GameState.Play);
    }
}
