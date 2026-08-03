using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class AxialSnapProcessor : InputProcessor<Vector2>
{
    public float radialDeadzone = 0.12f;
    public float axialSnapThreshold = 0.15f;

    #if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    #endif
    private static void Initialize()
    {
        InputSystem.RegisterProcessor<AxialSnapProcessor>("AxialSnap");
    }

    public override Vector2 Process(Vector2 value, InputControl control)
    {
        if (value.sqrMagnitude < radialDeadzone * radialDeadzone)
            return Vector2.zero;
        
        // Vertical Snap
        if (Mathf.Abs(value.y) < axialSnapThreshold)
            value.y = 0f;
        else
            value.y = Mathf.Sign(value.y) * Mathf.InverseLerp(axialSnapThreshold, 1f, Mathf.Abs(value.y));

        // Horizontal Snap
        if (Mathf.Abs(value.x) < axialSnapThreshold)
            value.x = 0f;
        else
            value.x = Mathf.Sign(value.x) * Mathf.InverseLerp(axialSnapThreshold, 1f, Mathf.Abs(value.x));

        return value;
    }
}