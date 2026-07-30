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
        MenuEventMessage messageBox = GameManager.Instance.MenuManager.OpenMenu("EventMessage") as MenuEventMessage;
        messageBox.SetText(text);

        return RuntimeEventInterpreterState.Halt;
    }    
}
