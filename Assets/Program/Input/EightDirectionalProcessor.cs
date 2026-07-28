using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Snaps Vector2 input direction to 8 cardinal/ordinal directions (every 45 degrees)
/// while preserving the analog input magnitude.
/// </summary>
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class EightDirectionalProcessor : InputProcessor<Vector2>
{
#if UNITY_EDITOR
    static EightDirectionalProcessor()
    {
        Initialize();
    }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        InputSystem.RegisterProcessor<EightDirectionalProcessor>();
    }

    public override Vector2 Process(Vector2 value, InputControl control)
    {
        float magnitude = value.magnitude;

        // Deadzone check
        if (magnitude < 0.001f)
            return Vector2.zero;

        // 1. Calculate angle in radians
        float angle = Mathf.Atan2(value.y, value.x);

        // 2. Snap angle to nearest 45-degree increment (PI / 4 radians)
        float step = Mathf.PI / 4f;
        float snappedAngle = Mathf.Round(angle / step) * step;

        // 3. Reconstruct unit direction vector
        Vector2 snappedDirection = new Vector2(Mathf.Cos(snappedAngle), Mathf.Sin(snappedAngle));

        // Clean up floating-point precision noise on exact 0 axes
        if (Mathf.Abs(snappedDirection.x) < 0.001f) snappedDirection.x = 0f;
        if (Mathf.Abs(snappedDirection.y) < 0.001f) snappedDirection.y = 0f;

        // 4. Scale unit direction by original analog magnitude
        return snappedDirection * magnitude;
    }
}