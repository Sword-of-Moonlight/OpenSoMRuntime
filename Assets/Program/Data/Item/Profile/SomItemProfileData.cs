using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SomItemProfileData
{
    [FieldOffset(0x00)] public SomItemProfileDataUsable usable;
    [FieldOffset(0x00)] public SomItemProfileDataWeapon weapon;
    [FieldOffset(0x00)] public SomItemProfileDataArmour armour;
    [FieldOffset(0x00)] public fixed byte raw[16];
}