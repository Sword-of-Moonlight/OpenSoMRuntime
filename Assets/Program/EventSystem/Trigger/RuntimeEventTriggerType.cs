using System;

/// <summary>
/// Event trigger type
/// </summary>
[Flags]
public enum RuntimeEventTriggerType : byte
{
    /// <summary>
    /// The event has no trigger (unused event)
    /// </summary>
    None = 0x00,

    /// <summary>
    /// The event is triggered by examining the parent entity (valid for NPC, Enemy, Object)
    /// </summary>
    Examine = 1 << 0,

    /// <summary>
    /// The event is triggered by using an item in proximity to the parent entity (valid for NPC, Enemy, Object)
    /// </summary>
    UseItemNear = 1 << 1,

    /// <summary>
    /// The event is triggered by the player overlapping an rectangle zone around the parent entity (valid for NPC, Enemy, Object)
    /// </summary>
    OverlapRectangle = 1 << 2,

    /// <summary>
    /// The event is triggered by the player overlapping an circle around the parent entity (valid for NPC, Enemy, Object)
    /// </summary>
    OverlapCircle = 1 << 3,

    /// <summary>
    /// The event is triggered by the death of the parent entity (valid for NPC, Enemy)
    /// </summary>
    EntityDeath = 1 << 4,

    /// <summary>
    /// The event is always active (valid for NPC, Enemy, Object... Possibily system if EVT file hacked)
    /// </summary>
    AlwaysActive = 1 << 5,

    /// <summary>
    /// The event is triggered by using an item anywhere (valid for NPC, Enemy, Object... Possibily system if EVT file hacked)
    /// </summary>
    UseItemGlobal = 1 << 6
}
