using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    /// <summary>
    /// Raw event storage
    /// </summary>
    RuntimeEvent[] eventStack;

    /// <summary>
    /// Execution state holds detail of the current executing event...
    /// </summary>
    RuntimeEventInterpreterData interpreterData = new RuntimeEventInterpreterData();

    public RuntimeEvent[] Events => eventStack;

    /// <summary>
    /// Singleton Instance
    /// </summary>
    public static EventManager Instance { get; private set; } = null;

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    public void Awake()
    {
        // Singleton Implementation
        if (Instance != null)
            throw new DuplicateSingletonException();

        Instance = this;
    }

    /// <summary>
    /// Loads new event data from a file<br/>
    /// </summary>
    public void LoadEventsFromFile(string filepath)
    {
        if (!File.Exists(filepath))
            throw new FileNotFoundException();

        // If an even is in progress, it must be terminated.
        StopEvent();

        // Remove all current events
        eventStack = null;

        // Load new events
        eventStack = SoMEventFile.LoadFromFile(filepath);
    }

    /// <summary>
    /// Gets the state of every event
    /// </summary>
    public byte[] GetEventStateBuffer()
    {
        // We return null if the state is not valid yet.
        if (eventStack == null)
            return null;

        // There are only 16 pages per event, store the state of two events per byte...
        byte[] stateBuffer = new byte[512];

        for (int i = 0; i < 512; ++i)
        {
            // start with clear state...
            byte stateByte = 0x00;

            // Take the current state of two events and put them into a single byte
            stateByte |= (byte)((eventStack[(2 * i) + 0].CurrentPageIndex & 0xF) << 0);
            stateByte |= (byte)((eventStack[(2 * i) + 1].CurrentPageIndex & 0xF) << 4);

            // Store that byte in the buffer
            stateBuffer[i] = stateByte;
        }

        return stateBuffer;
    }

    /// <summary>
    /// Sets the state of every event
    /// </summary>
    public void SetEventStateBuffer(byte[] stateBuffer)
    {
        // We return null if the state is not valid yet.
        if (eventStack == null)
            return;

        for (int i = 0; i < 512; ++i)
        {
            eventStack[(2 * i) + 0].CurrentPageIndex = (stateBuffer[i] >> 0) & 0xF;
            eventStack[(2 * i) + 1].CurrentPageIndex = (stateBuffer[i] >> 4) & 0xF;
        }
    }

    /// <summary>
    /// Executes an event by ID
    /// </summary>
    public void ExecuteEvent(int eventId)
    {
        // Event Stack must not be null...
        if (eventStack == null)
            return;

        // The interpreter must not already be trying to run an event
        if (interpreterData.State != RuntimeEventInterpreterState.Free)
        {
            Logger.Critical("Tried to start an event while already processing one...");
            return;
        }
            
        // Get the event
        RuntimeEvent ev = eventStack[eventId];

        // TO-DO: Evaluate the event trigger...

        // Is the current page valid ?
        if (ev.CurrentPage == null)
            return;

        // Evalulate the main event condition
        if (!ev.Condition.Evaluate())
            return;

        // Evalulate the event page condition
        if (!ev.CurrentPage.Condition.Evaluate())
            return;

        // When all of the above checks have passed... We can finally begin executing the event.
        interpreterData.Reset();
        interpreterData.Event   = ev;
        interpreterData.Program = ev.CurrentPage.Payload;

        // Resume event is called to begin processing, as we are resuming from the first operation.
        ResumeEvent();
    }

    /// <summary>
    /// Resumes event processing from the last position
    /// </summary>
    public void ResumeEvent()
    {
        // Must mark as executing
        interpreterData.State = RuntimeEventInterpreterState.Executing;

        do
        {
            interpreterData.State = interpreterData.Program[interpreterData.PC++].Do(interpreterData);
        } 
        while (interpreterData.State == RuntimeEventInterpreterState.Executing);

        // Debug Logging...
        Logger.Custom("EVNT", 0x80F080, $"Resume State = {interpreterData.State}");
    }

    /// <summary>
    /// Force stop the current event, and resets state
    /// </summary>
    public void StopEvent()
    {
        interpreterData.Reset();
        interpreterData.State = RuntimeEventInterpreterState.Free;
    }
}
