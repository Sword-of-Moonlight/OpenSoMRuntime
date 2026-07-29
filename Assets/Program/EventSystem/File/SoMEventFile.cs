using System;
using System.Collections.Generic;

public partial class SoMEventFile
{
    public unsafe static RuntimeEvent[] LoadFromFile(string filepath)
    {
        // open evt file for reading
        using FileInputStream fis = new FileInputStream(filepath);

        // 
        // EVT Header
        //
        uint evtEventNum = fis.ReadU32();

        // Storage for event set
        RuntimeEvent[] result = new RuntimeEvent[evtEventNum];

        //
        // EVT Event Definitions + Page Definitions + Page Payloads
        //
        for (int i = 0; i < evtEventNum; ++i)
        {
            EVTHeader evtHeader = fis.ReadStruct<EVTHeader>();    // Read Header
            EVTTrigger evtTrigger = fis.ReadStruct<EVTTrigger>();   // Read Trigger
            EVTCondition evtCondition = fis.ReadStruct<EVTCondition>(); // Read Condition

            // We must now read each event page from the file
            RuntimeEventPage[] evtPages = new RuntimeEventPage[16];

            for (int j = 0; j < evtPages.Length; ++j)
            {
                // First comes the payload offset of the page...
                uint payloadOffset = fis.ReadU32();

                // Then we must read the page condition
                EVTCondition pageCondition = fis.ReadStruct<EVTCondition>();

                // When the payload offset is 0, the page is not used.
                if (payloadOffset == 0)
                    continue;

                // Jump and then return to read the next page...
                fis.Jump(payloadOffset);
                IRuntimeEventOperation[] pageOperations = DecodeBytecode(fis);
                fis.Return();

                evtPages[j] = new RuntimeEventPage
                {
                    Condition = new RuntimeEventCondition(
                        pageCondition.type,
                        pageCondition.comparision,
                        pageCondition.conditionConstant,
                        pageCondition.conditionId
                    ),

                    // Page operations will be stored here...
                    Payload = pageOperations
                };
            }

            // We can now begin the process of creating the event defintion by copying in our converted data
            result[i] = new RuntimeEvent
            {
                // The name must be converted from SJIS byte format
                Name = EncodingExtensions.SJIS.GetString(evtHeader.name, 31).Sanitise(),

                // Target
                Target = new RuntimeEventTarget(
                    evtHeader.targetType,
                    evtHeader.targetId
                ),

                // Trigger
                Trigger = new RuntimeEventTrigger(
                    evtTrigger.type,
                    evtTrigger.item,
                    evtTrigger.cone,
                    evtTrigger.rectWE,
                    evtTrigger.rectNS,
                    evtTrigger.radius
                ),

                // Condition
                Condition = new RuntimeEventCondition(
                    evtCondition.type,
                    evtCondition.comparision,
                    evtCondition.conditionConstant,
                    evtCondition.conditionId
                ),

                // Pages and page counter
                Pages = evtPages,
                CurrentPageIndex = 0
            };
        }

        return result;
    }

    /// <summary>
    /// Decodes event page byte code into an array of RuntimeEventOperations
    /// </summary>
    unsafe static IRuntimeEventOperation[] DecodeBytecode(FileInputStream fis)
    {
        // This list is used to store decoded operations to return
        List<IRuntimeEventOperation> decodedOps = new List<IRuntimeEventOperation>();

        // Decoding loop... We keep going until we reach the return operation.
        short OpCode, PayloadSize;
        do
        {
            // Read OpCode ID and payload size.
            OpCode      = fis.ReadS16();
            PayloadSize = fis.ReadS16();
            PayloadSize -= 4;   // we subtract 4, as the previous 4 bytes are included in this...

            // Now the actual decoding...
            switch (OpCode)
            {
                // Display Message (Operation 0x0000)
                // Displays a message on the screen
                case 0:
                    string dispMsgText = fis.ReadFixedString(PayloadSize, EncodingExtensions.SJIS).Sanitise();
                    decodedOps.Add(new RuntimeEventOperationDisplayMessage { Text = dispMsgText });
                    break;

                // Return          (Operation 0xFFFF)
                // Always placed at the end of an event.
                case -1:
                    decodedOps.Add(new RuntimeEventOperationReturn { });
                    break;
                
                // When encountering a operation we don't understand yet, log it.
                default:
                    fis.SeekRelative(PayloadSize);
                    Logger.Critical($"Unknown EVT operation: 0x{OpCode:X4}, at 0x{fis.Position:X8}");
                    break;
            }

        } while (OpCode != -1);

        // Return the decoded operations
        return decodedOps.ToArray();
    }
}