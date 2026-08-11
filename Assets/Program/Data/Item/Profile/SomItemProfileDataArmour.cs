using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public struct SomItemProfileDataArmour
{
    [FieldOffset(0x00)] public SomItemEquipType equipType;  // The type of this piece of armour. 0 = Helm, 1 = Body, 2 = Arms, 3 = Boots, 4 = Suit, 5 = Shield, 6 = Accessory
    [FieldOffset(0x01)] public byte unkx01;                 // Unknown. These first few values are mixes of either 00 or FF (probably signed), They probably have _some_ purpose...
    [FieldOffset(0x02)] public ushort unkx02;               // Unknown. ^  ^  ^  ^  ^  ^  ^  ^  ^  ^
    [FieldOffset(0x04)] public uint unkx04;                 // Unknown. ^  ^  ^  ^  ^  ^  ^  ^  ^  ^
    [FieldOffset(0x08)] public uint unkx08;                 // Unknown.
    [FieldOffset(0x0C)] public uint unkx0C;                 // Unknown.
}