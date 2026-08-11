using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public struct SomItemProfileDataUsable
{
    [FieldOffset(0x00)] public uint unkx00;     // Unknown.
    [FieldOffset(0x04)] public byte slotKeyId;  // Special ID which must match with an objects to be able to slot the item into the object.
    [FieldOffset(0x05)] public byte unkx05;     // Unknown.
    [FieldOffset(0x06)] public ushort unkx06;   // Unknown.
    [FieldOffset(0x08)] public uint unkx08;     // Unknown.
    [FieldOffset(0x0C)] public uint unkx0C;     // Unknown.
}