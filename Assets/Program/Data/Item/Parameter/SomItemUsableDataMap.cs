using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SomItemUsableDataMap
{
    [FieldOffset(0x00)] public SomItemMapType displayType;
    [FieldOffset(0x01)] public fixed byte pictureFile[31];  
}