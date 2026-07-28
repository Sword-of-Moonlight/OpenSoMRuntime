using System;
using UnityEngine;

public class SoMEventFile
{
    public static RuntimeEvent[] LoadFromFile(string filepath)
    {
        RuntimeEvent[] result = null;

        // open evt file for reading
        using FileInputStream fis = new FileInputStream(filepath);

        // 
        // EVT Header
        //
        uint evtEventNum = fis.ReadU32();

        result = new RuntimeEvent[evtEventNum];

        //
        // EVT Event Definitions + Page Definitions + Page Payloads
        //
        for (int i = 0; i < evtEventNum; ++i)
        {
            // Event Defintion - meta
            string evtName      = fis.ReadFixedString(31, EncodingExtensions.SJIS).Sanitise();
            sbyte evtTargetType = fis.ReadS8();
            short evtTargetID   = fis.ReadS16();

            // Event Definition - Trigger
            RuntimeEventTriggerType evtTriggerType = fis.ReadEnum<RuntimeEventTriggerType>();
            byte evtTriggerItem    = fis.ReadU8();
            ushort evtTriggerCone  = fis.ReadU16();
            ushort evtTriggerUnkn  = fis.ReadU16();
            float evtTriggerRectWE = fis.ReadF32();
            float evtTriggerRectNS = fis.ReadF32();
            float evtTriggerRadius = fis.ReadF32();

            RuntimeEventTrigger resultTrigger = new RuntimeEventTrigger { TriggerType = evtTriggerType };

            // Event Definition - Condition
            RuntimeEventConditionType evtConditionType = fis.ReadEnum<RuntimeEventConditionType>();
            ushort evtConditionID   = fis.ReadU16();
            ushort evtConditionVal  = fis.ReadU16();
            RuntimeEventComparisonType evtConditionComp = fis.ReadEnum<RuntimeEventComparisonType>();

            RuntimeEventCondition resultCondition = evtConditionType switch
            {
                RuntimeEventConditionType.None         => CreateSimpleCondition<RuntimeEventCondition>(evtConditionComp, evtConditionVal),
                RuntimeEventConditionType.ItemQuantity => throw new NotImplementedException(),
                RuntimeEventConditionType.CoinQuantity => CreateSimpleCondition<RuntimeEventPlayerCoinCondition>(evtConditionComp, evtConditionVal),
                RuntimeEventConditionType.Strength     => CreateSimpleCondition<RuntimeEventPlayerStrengthCondition>(evtConditionComp, evtConditionVal),
                RuntimeEventConditionType.Magic        => CreateSimpleCondition<RuntimeEventPlayerMagicCondition>(evtConditionComp, evtConditionVal),
                RuntimeEventConditionType.Level        => CreateSimpleCondition<RuntimeEventPlayerLevelCondition>(evtConditionComp, evtConditionVal),
                RuntimeEventConditionType.Counter =>
                    new RuntimeEventCounterCondition
                    {
                        ComparisonMode    = evtConditionComp,
                        ConditionConstant = evtConditionVal,

                        CounterID = evtConditionID
                    },

                // Default
                _ => throw new ArgumentOutOfRangeException(nameof(evtConditionType))
            };


            // Event Definition - Pages
            RuntimeEventPage[] eventPages = new RuntimeEventPage[16];

            for (int j = 0; j < 16; ++j)
            {
                // Page definition

                // Page payload (where valid)
            }

            // When the event is invalid, set the slot to null - otherwise read the page operations
            // and compile them.
            result[i] = null;
        }

        return result;
    }

    static RuntimeEventCondition CreateSimpleCondition<T>(RuntimeEventComparisonType comparisionMode, ushort constant) where T : RuntimeEventCondition, new()
        => new T
        {
            ComparisonMode    = comparisionMode,
            ConditionConstant = constant
        };
}