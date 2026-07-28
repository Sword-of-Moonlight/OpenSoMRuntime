using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Displays as "Axial Snap" in the Input Action editor's Processor dropdown
[InitializeOnLoad]
public class AxialSnapProcessor : InputProcessor<Vector2>
{
    [Tooltip("Clears center stick drift (radial deadzone).")]
    public float radialDeadzone = 0.12f;

    [Tooltip("Thickness of the snap band. Minor axis input below this threshold is zeroed out.")]
    public float axialSnapThreshold = 0.15f;

    // Register processor when Unity loads in the Editor
#if UNITY_EDITOR
    static AxialSnapProcessor()
    {
        Initialize();
    }
#endif

    // Register processor when running a built game
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        InputSystem.RegisterProcessor<AxialSnapProcessor>("AxialSnap");
    }

    public override Vector2 Process(Vector2 value, InputControl control)
    {
        // 1. Radial Deadzone
        if (value.sqrMagnitude < radialDeadzone * radialDeadzone)
        {
            return Vector2.zero;
        }

        Vector2 result = value;

        // 2. Horizontal Snap Band (Zeroes out minor Y movement when panning X)
        if (Mathf.Abs(value.y) < axialSnapThreshold)
        {
            result.y = 0f;
        }
        else
        {
            float signY = Mathf.Sign(value.y);
            result.y = signY * Mathf.InverseLerp(axialSnapThreshold, 1f, Mathf.Abs(value.y));
        }

        // 3. Vertical Snap Band (Zeroes out minor X movement when tilting Y)
        if (Mathf.Abs(value.x) < axialSnapThreshold)
        {
            result.x = 0f;
        }
        else
        {
            float signX = Mathf.Sign(value.x);
            result.x = signX * Mathf.InverseLerp(axialSnapThreshold, 1f, Mathf.Abs(value.x));
        }

        return result;
    }
}