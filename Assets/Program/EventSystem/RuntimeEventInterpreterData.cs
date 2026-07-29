using UnityEngine;

public class RuntimeEventInterpreterData
{
    public RuntimeEvent Event { get; set; } = null;
    public IRuntimeEventOperation[] Program { get; set; } = null;
    public RuntimeEventInterpreterState State { get; set; } = RuntimeEventInterpreterState.Free;
    public int PC { get; set; } = 0;

    public void Reset()
    {
        Event   = null;
        Program = null;
        State   = RuntimeEventInterpreterState.Free;
        PC      = 0;
    }
}
