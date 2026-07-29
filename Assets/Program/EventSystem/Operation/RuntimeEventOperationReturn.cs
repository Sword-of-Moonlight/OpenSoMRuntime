public class RuntimeEventOperationReturn : IRuntimeEventOperation
{
    public short OpCode => -1;

    /// <summary>
    /// Simply returns the free state, as the event has finished.
    /// </summary>
    public RuntimeEventInterpreterState Do() =>
        RuntimeEventInterpreterState.Free;
}
