using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit, Pack=1)]
public struct ReferenceID
{
    [FieldOffset(0x00)] public byte type;   // Reference Type. 0 = Constant (Defined in map data), 1 = Dynamic (Spawned)
    [FieldOffset(0x01)] public byte entity; // Entity Type. 0 = Object, 1 = Item, 2 = NPC, 3 = Enemy
    [FieldOffset(0x02)] public ushort id;   // Entity ID.

    public uint RefID
    {
        get
        {
            return (uint)((type << 24) | (entity << 16) | id);
        }

        set
        {
            type   = (byte)((value >> 24) & 0xFF);
            entity = (byte)((value >> 16) & 0xFF);
            id     = (byte)((value >> 00) & 0xFFFF);
        }
    }
}
