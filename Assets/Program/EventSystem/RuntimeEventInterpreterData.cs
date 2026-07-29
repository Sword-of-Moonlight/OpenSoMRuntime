using UnityEngine;

public class RuntimeEventInterpreterData
{
    public IRuntimeEventOperation[] Program { get; set; } = null;
    public RuntimeEventInterpreterState State { get; set; } = RuntimeEventInterpreterState.Free;
    public int PC { get; set; } = 0;

    public void Reset()
    {
        Program = null;
        State   = RuntimeEventInterpreterState.Free;
        PC      = 0;
    }
}
