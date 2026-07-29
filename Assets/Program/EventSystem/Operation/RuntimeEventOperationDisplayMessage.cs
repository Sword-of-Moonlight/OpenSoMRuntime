public class RuntimeEventOperationDisplayMessage : IRuntimeEventOperation
{
    public short OpCode => 0;

    public string text;

    /// <summary>
    /// Displays a message.<br/>
    /// Returns halt, as event processing must pause while this one finishes
    /// </summary>
    public RuntimeEventInterpreterState Do(RuntimeEventInterpreterData interpreterData)
    {
        GameManager.Instance.MenuManager.ShowSystemMessage(text, EventManager.Instance.ResumeEvent);

        return RuntimeEventInterpreterState.Halt;
    }    
}
