using System;
using UnityEngine;

public class RuntimeEventOperationChangeEventPage : IRuntimeEventOperation
{
    public short OpCode => 145;

    /// <summary>
    /// The event target.
    /// </summary>
    public short targetEvent;
    public byte moveType;
    public byte pageToMoveTo;

    /// <summary>
    /// Change an events active page.<br/>
    /// </summary>
    public RuntimeEventInterpreterState Do(RuntimeEventInterpreterData interpreterData)
    {
        // Get the event to modify the page of
        RuntimeEvent Target;

        if (targetEvent == -1)
            Target = interpreterData.Event;
        else
            Target = EventManager.Instance.Events[targetEvent];

        // Set the new event page
        Target.CurrentPageIndex = moveType switch
        {
            0 => (Target.CurrentPageIndex + 1),
            1 => (Target.CurrentPageIndex - 1),
            2 => (pageToMoveTo),

            _ => throw new NotImplementedException()
        };

        return RuntimeEventInterpreterState.Executing;
    }
}