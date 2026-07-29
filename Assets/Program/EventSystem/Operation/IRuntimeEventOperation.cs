public interface IRuntimeEventOperation
{
    public short OpCode { get; }
    RuntimeEventInterpreterState Do(RuntimeEventInterpreterData interpreterData);
}
