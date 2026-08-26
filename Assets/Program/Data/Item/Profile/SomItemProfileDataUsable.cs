using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public struct SomItemProfileDataUsable
{
    // Data
    [FieldOffset(0x00), SerializeField] uint unkx00;     // Unknown.
    [FieldOffset(0x04), SerializeField] byte slotKeyId;  // Special ID which must match with an objects to be able to slot the item into the object.
    [FieldOffset(0x05), SerializeField] byte unkx05;     // Unknown.
    [FieldOffset(0x06), SerializeField] ushort unkx06;   // Unknown.
    [FieldOffset(0x08), SerializeField] uint unkx08;     // Unknown.
    [FieldOffset(0x0C), SerializeField] uint unkx0C;     // Unknown.

    /// <summary>
    /// Slot-Key id for linking pedestals
    /// </summary>
    public byte SlotKeyID
    {
        get
        {
            return slotKeyId;
        }
    }
}