using UnityEngine;

/// <summary>
///RenderingStyle provides the ability to define a group of common graphical settings to suite an aesthetic
/// </summary>
[CreateAssetMenu(fileName = "RenderingStyle", menuName = "OpenSoM/Rendering Style")]
public class RenderingStyle : ScriptableObject
{
    [field: Header("Shader Configuration")]
    [field: SerializeField] public string ObjectStatic { get; private set; } = string.Empty;
    [field: SerializeField] public string ObjectAnimated { get; private set; } = string.Empty;

    [field: Header("Graphical Options")]
    [field: SerializeField] public bool EnableRealTimeShadows { get; private set; } = false;
}
