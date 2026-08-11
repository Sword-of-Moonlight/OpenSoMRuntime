using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SomItemUsableData
{
    [FieldOffset(0x00)] public SomItemUsableDataMap map;
    [FieldOffset(0x00)] public SomItemUsableDataRecovery recovery;
    [FieldOffset(0x00)] public fixed byte raw[32];
}