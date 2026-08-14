using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SomItemProfile
{
    [FieldOffset(0x00)] public fixed byte name[31];
    [FieldOffset(0x1F)] public fixed byte modelFile[31];
    [FieldOffset(0x3E)] public SomItemType type;
    [FieldOffset(0x40)] public float menuElevationOffset;
    [FieldOffset(0x44)] public ushort menuTilt;
    [FieldOffset(0x46)] public ushort worldTilt;
    [FieldOffset(0x48)] public SomItemProfileData data;
}
