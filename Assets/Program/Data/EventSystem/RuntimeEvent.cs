using UnityEngine;

public class RuntimeEvent
{
    /// <summary>Name of the event</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Start condition for the event</summary>
    public RuntimeEventStartCondition StartCondition { get; private set; } = default;

    /// <summary>Current active page ID of the event</summary>
    public int ActivePageID { get; private set; } = 0;

    // Data

    /// <summary>Pages contained in the event.</summary>
    RuntimeEventPage[] eventPages;
}