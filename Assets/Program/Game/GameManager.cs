using DG.Tweening;
using System;
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
    [field: SerializeField] public SoMLevelCurve LevelCurve { get; private set; }
    [field: SerializeField] public RenderingStyle RenderStyle { get; private set; }

    [field: SerializeField] public SomObjectRegistry ObjectRegistry { get; private set; }
    [field: SerializeField] public SomItemRegistery ItemRegistry { get; private set; }

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
    public string EditorPath { get; private set; }
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

        // Free our registry data...
        ObjectRegistry.Free();
        ItemRegistry.Free();

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
        // Testing Grabbing CMD for SOM-DB replacement support
        string[] arguments = Environment.GetCommandLineArgs();
        foreach (string arg in arguments)
            Logger.Info($"CMD ARG = {arg}");

        if (MultiGameMode)
        {
            ProjectPath = Path.Combine(Path.GetFullPath(Application.streamingAssetsPath), $"GameData_{MultiGameName}");
            DataPath = Path.Combine(Path.GetFullPath(Application.persistentDataPath), MultiGameName);
        }
        else
        {
            ProjectPath = Path.GetFullPath(Application.streamingAssetsPath);
            DataPath = Path.GetFullPath(Application.persistentDataPath);
        }

        // TO-DO: Implement this for SOM_DB support. Need to _CHECK_ the arguments before we fucking use them, lol... SOM_DB also takes a map ID, and allows immediate start when it is passed.
        // EditorPath = arguments[2];
        // ProjectPath = arguments[1];

        if (!Directory.Exists(DataPath))
            Directory.CreateDirectory(DataPath);

        // New Session Data
        SessionData = new SessionData();

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
        DOTween.SetTweensCapacity(128, 64);
    }

    /// <summary>
    /// Initialize...
    /// </summary>
    void InitializeGameData()
    {
        // Initialize Resource Manager
        ResourceManager.Initialize();
        ResourceManager.AssignResourceRoot(EditorPath);
        ResourceManager.AssignResourceRoot(ProjectPath);

        // Load actual project data...
        ProjectData.LoadLegacyProject(ProjectPath);

        if (ForceInitialMap >= 0)
            ProjectData.initialMap = (byte)(ForceInitialMap & 0x3F);

        // Initialize Game Components
        MenuManager.Initialize();
        PlayerController.Initialize(ProjectData);

        // Initialize Game Data
        LevelCurve.Initialize();

        // Follow this for future SoM data implementations!.. (Remove this comment when refactor is complete)
        ItemRegistry.Load();
        ObjectRegistry.Load();
    }
}
