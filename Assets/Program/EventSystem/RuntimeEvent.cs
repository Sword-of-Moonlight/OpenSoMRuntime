/// <summary>
/// Friendly representation of a SoM event, including all SoM editor data and the pages themselves.
/// </summary>
public class RuntimeEvent
{
    /// <summary>
    /// The name of the event as displayed in the Sword of Moonlight Event Editor
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The target type of the event, also acting as a parent of sorts.
    /// </summary>
    public RuntimeEventTarget Target { get; set; } = RuntimeEventTarget.Default;

    /// <summary>
    /// Trigger control data for the event, declaring what will cause the event to run.
    /// </summary>
    public RuntimeEventTrigger Trigger { get; set; } = RuntimeEventTrigger.Default;

    /// <summary>
    /// Condition data for the event.
    /// </summary>
    public RuntimeEventCondition Condition { get; set; } = RuntimeEventCondition.Default;

    /// <summary>
    /// Page list for the event
    /// </summary>
    public RuntimeEventPage[] Pages { get; set; } = null;

    /// <summary>
    /// The index of the current active page
    /// </summary>
    public int CurrentPageIndex { get; set; } = 0;

    /// <summary>
    /// The current event page itself
    /// </summary>
    public RuntimeEventPage CurrentPage => Pages[CurrentPageIndex];

}