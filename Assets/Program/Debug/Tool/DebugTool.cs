using UnityEngine;

public class DebugTool : MonoBehaviour
{
    [field: SerializeField] public string ProjectPath { get; protected set; }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    protected virtual void Awake()
    {
        ResourceManager.Initialize();
        ResourceManager.AssignResourceRoot(ProjectPath);
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        ResourceManager.Dump();
        ResourceManager.Purge();
    }
}
