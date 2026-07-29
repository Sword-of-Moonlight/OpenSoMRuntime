using System.Runtime.InteropServices;

public partial class SoMEventFile
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct EVTTrigger
    {
        [FieldOffset(0x00)] public RuntimeEventTriggerType type;
        [FieldOffset(0x01)] public byte item;
        [FieldOffset(0x02)] public short cone;
        [FieldOffset(0x04)] public short u16x04;
        [FieldOffset(0x06)] public float rectWE;
        [FieldOffset(0x0A)] public float rectNS;
        [FieldOffset(0X0E)] public float radius;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct EVTCondition
    {
        [FieldOffset(0x00)] public RuntimeEventConditionType type;
        [FieldOffset(0x02)] public ushort conditionId;
        [FieldOffset(0x04)] public ushort conditionConstant;
        [FieldOffset(0x06)] public RuntimeEventComparisonType comparision;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    unsafe struct EVTHeader
    {
        [FieldOffset(0x00)] public fixed byte name[31];
        [FieldOffset(0x1F)] public RuntimeEventTargetType targetType;
        [FieldOffset(0x20)] public short targetId;
    }
};