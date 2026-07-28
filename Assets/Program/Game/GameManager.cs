using DG.Tweening;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Entry point for all game logic
/// </summary>
public class GameManager : MonoBehaviour
{
    [field: Header("References (External)")]
    /// <summary>SoM Project Data</summary>
    [field: SerializeField] public SoMProjectData ProjectData { get; private set; }
    [field: SerializeField] public SoMObjectData ObjectData { get; private set; }
    [field: SerializeField] public SoMLevelCurve LevelCurve { get; private set; }

    [field: SerializeField] public InputActionAsset InputActions { get; private set; }
    [field: SerializeField] public MenuManager MenuManager { get; private set; }
    [field: SerializeField] public PlayerController PlayerController { get; private set; }

    [field: SerializeField] public SessionData SessionData { get; private set; }

    [field: Header("Multi Game Configuration")]
    [field: Tooltip("Set to use Multi Game Mode")]
    [field: SerializeField] public bool MultiGameMode { get; private set; } // We should always be using this, really...
    [field: Tooltip("Set to the name of the game to load")]
    [field: SerializeField] public string MultiGameName { get; private set; }

    // Public Data
    public GameStateMachine StateMachine { get; private set; }
    public string ProjectPath { get; private set; }
    public string DataPath { get; private set; }

    [field: Header("Debugging")]
    [field: SerializeField] public bool StartGameInstantly { get; private set; } = false;
    [field: SerializeField] public int ForceInitialMap { get; private set; } = -1;

    [field: Header("Tweaks (TO-DO: Move to SO config)")]
    [field: SerializeField] public bool BoostCameraZFarBehindFog = false;

    /// <summary>Singleton Instance</summary>
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    void Awake()
    {
        // Singleton Implementation.
        if (Instance != null)
            throw new DuplicateSingletonException();

        Instance = this;

        // Setup
        SetupRuntimeEnviroment();
        SetupUnityEnviroment();

        // Initialize
        InitializeGameData();
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// Clean up...
    /// </summary>
    void OnApplicationQuit()
    {
        // Shut down the menu manager, freeing all menu related assets
        MenuManager.Shutdown();

        // CURRENTLY FULL OF MEMORY LEAKS: we need to make purge have a 'forceReleaseAll' mode which clears out all assets.
        ResourceManager.Purge();
        ResourceManager.Dump();
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    void FixedUpdate()
    {
        StateMachine.Tick();
    }

    /// <summary>
    /// Sets up the runtime enviroment.
    /// </summary>
    void SetupRuntimeEnviroment()
    {
        if (MultiGameMode)
        {
            ProjectPath = Path.Combine(Path.GetFullPath(Application.streamingAssetsPath), $"GameData_{MultiGameName}");
            DataPath    = Path.Combine(Path.GetFullPath(Application.persistentDataPath), MultiGameName);
        }        
        else
        {
            ProjectPath = Path.GetFullPath(Application.streamingAssetsPath);
            DataPath    = Path.GetFullPath(Application.persistentDataPath);
        }

        if (!Directory.Exists(DataPath))
            Directory.CreateDirectory(DataPath);

        Logger.Info($"Paths: {{\n\tProject = '{ProjectPath}',\n\tData = '{DataPath}'\n}}");

        // New Session Data
        SessionData = new SessionData();

        // Initialize Resource Manager
        ResourceManager.Initialize(ProjectPath);

        // Initialize State Machine
        StateMachine = new GameStateMachine();
    }

    /// <summary>
    /// Sets up the unity enviroment
    /// </summary>
    void SetupUnityEnviroment()
    {
        // Unity Time Settings
        Time.fixedDeltaTime = 1f / 64f;

        // Initialize DO Tween
        DOTween.Init(false, false, LogBehaviour.Default);
        DOTween.SetTweensCapacity(64, 32);
    }

    /// <summary>
    /// Initialize...
    /// </summary>
    void InitializeGameData()
    {
        ProjectData.LoadLegacyProject(ProjectPath);

        if (ForceInitialMap >= 0)
            ProjectData.initialMap = (byte)(ForceInitialMap & 0x3F);

        // Initialize Game Components
        MenuManager.Initialize();
        PlayerController.Initialize(ProjectData);

        // Initialize Game Data
        LevelCurve.Initialize();
        ObjectData.Load();
    }
}
