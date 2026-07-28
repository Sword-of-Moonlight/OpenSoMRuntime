using UnityEngine;

public class RuntimeEventTriggerContext
{
    /// Event Constant
    public Vector3 EventPosition { get; set; } = Vector3.zero;

    /// <summary>Player Position</summary>
    public Vector3 PlayerPosition { get; set; } = Vector3.zero;

    /// <summary>Player Rotation</summary>
    public Quaternion PlayerRotation { get; set; } = Quaternion.identity;

    /// <summary>Used Item ID (This Frame)</summary>
    public int UsedItemID { get; set; } = -1;
}
