/// <summary>
/// Runtime Event Trigger is used to evaluate if a given context will caused the event to run
/// </summary>
public readonly struct RuntimeEventTrigger
{
    /// <summary>Default Trigger Definition</summary>
    public static readonly RuntimeEventTrigger Default = new RuntimeEventTrigger(RuntimeEventTriggerType.None, 255, 0, 0F, 0F, 0F);

    /// <summary>The event trigger type</summary>
    public readonly RuntimeEventTriggerType type;

    /// <summary>An item ID which can trigger the event (default = 0xFF)</summary>
    public readonly byte itemId;

    /// <summary>The maximum angle between the player and parent before activating</summary>
    public readonly short activationConeAngle;

    /// <summary>The size of the trigger rectangle on the X axis</summary>
    public readonly float rectangleX;

    /// <summary>The size of the trigger rectangle on the Z axis</summary>
    public readonly float rectangleZ;

    /// <summary>The radius of the trigger</summary>
    public readonly float radius;

    /// <summary>
    /// Default Constructor.<br/>
    /// Constructs from raw data
    /// </summary>
    public RuntimeEventTrigger(RuntimeEventTriggerType type, byte itemId, short coneAngle, float rectX, float rectZ, float radius)
    {
        this.type           = type;
        this.itemId         = itemId;
        activationConeAngle = coneAngle;
        rectangleX          = rectX;
        rectangleZ          = rectZ;
        this.radius         = radius;
    }
}