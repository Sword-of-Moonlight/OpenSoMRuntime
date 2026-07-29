public class RuntimeEventPage
{
    /// <summary>
    /// Condition data for the page.
    /// </summary>
    public RuntimeEventCondition Condition { get; set; } = RuntimeEventCondition.Default;
    
    /// <summary>
    /// Payload of the event.
    /// </summary>
    public IRuntimeEventOperation[] Payload { get; set; } = null;
}
