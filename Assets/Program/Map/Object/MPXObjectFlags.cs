// Data Definitions
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public unsafe struct MPXObjectFlags
{
    [FieldOffset(0x00)] public MPXObjectFlagsLight lightFlags;  // 9 of 32 bytes used
    [FieldOffset(0x00)] public fixed byte raw[32];
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct MPXObjectFlagsLight
{
    [FieldOffset(0x00)] public float range;
    [FieldOffset(0x04)] public uint colour;
    [FieldOffset(0x08)] public byte affectObjects;
}