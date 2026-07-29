/// <summary>
/// Runtime event target is used for event filtering between parents, and execution time.<br/>
/// It is a binding between a "thing" and the event itself.
/// </summary>
public readonly struct RuntimeEventTarget
{
    /// <summary>Default Target Definition</summary>
    public static readonly RuntimeEventTarget Default = new RuntimeEventTarget(RuntimeEventTargetType.None, 0);

    /// <summary>
    /// The target type allows filtering between different target types.
    /// </summary>
    public readonly RuntimeEventTargetType type;

    /// <summary>
    /// If type is none or system, it is supposed to be 0; otherwise, it is an ID to an NPC, Object or Enemy.
    /// </summary>
    public readonly short id;

    /// <summary>
    /// Default Constructor.<br/>
    /// Constructs from raw data
    /// </summary>
    public RuntimeEventTarget(RuntimeEventTargetType type, short id)
    {
        this.type = type;
        this.id   = id;
    }
}
