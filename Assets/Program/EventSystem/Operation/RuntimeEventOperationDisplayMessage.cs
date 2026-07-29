public class RuntimeEventOperationDisplayMessage : IRuntimeEventOperation
{
    public short OpCode => 0;

    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Displays a message.<br/>
    /// Returns halt, as event processing must pause while this one finishes
    /// </summary>
    public RuntimeEventInterpreterState Do()
    {
        GameManager.Instance.MenuManager.ShowSystemMessage(Text, EventManager.Instance.ResumeEvent);

        return RuntimeEventInterpreterState.Halt;
    }
        
}
