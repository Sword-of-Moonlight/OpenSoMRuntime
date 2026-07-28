using System;

public class RuntimeEventTrigger
{
    /// <summary>How the event is triggered</summary>
    public RuntimeEventTriggerType TriggerType { get; set; }

    /// <summary>
    /// Returns true when the event can be run.
    /// </summary>
    /// <returns></returns>
    public virtual bool CanRun(RuntimeEventTriggerContext context)
        => true;
}

[Flags]
public enum RuntimeEventTriggerType : byte
{
    None             = 0x00,
    Examine          = 1 << 0,
    UseItemNear      = 1 << 1,  
    OverlapRectangle = 1 << 2,  // DONE
    OverlapCircle    = 1 << 3,  // DONE
    EntityDeath      = 1 << 4,
    AlwaysActive     = 1 << 5,  // DONE (Just use RuntimeEventTrigger as is)
    UseItemGlobal    = 1 << 6   // DONE
}