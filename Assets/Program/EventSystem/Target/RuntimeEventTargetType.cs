/// <summary>
/// The target type of a runtime event
/// </summary>
public enum RuntimeEventTargetType : sbyte
{
    /// <summary>The event is child to an NPC, and an NPC is its target</summary>
    NPC = 0,

    /// <summary>The event is child to an enemy, and an enemy is its target</summary>
    Enemy = 1,

    /// <summary>The event is child to an object, and an object is its target</summary>
    Object = 2,

    /// <summary>The event is called by the system</summary>
    System = -2,

    /// <summary>The event is unused (empty slot)</summary>
    None = -1
}