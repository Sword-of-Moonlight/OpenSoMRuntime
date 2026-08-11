using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public struct SomItemParameterDataUsable
{
    [FieldOffset(0x00)] public SomItemUsableType type;
    [FieldOffset(0x01)] public byte unusable;
    [FieldOffset(0x02)] public byte dontConsume;
    [FieldOffset(0x03)] public byte unkx03;
    [FieldOffset(0x04)] public uint unkx04;
    [FieldOffset(0x08)] public SomItemUsableData data;
}
