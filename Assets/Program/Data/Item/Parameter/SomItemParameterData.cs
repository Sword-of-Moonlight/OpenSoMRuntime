using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SomItemParameterData
{
    [FieldOffset(0x00)] public SomItemParameterDataArmour armour;
    [FieldOffset(0x00)] public SomItemParameterDataWeapon weapon;
    [FieldOffset(0x00)] public SomItemParameterDataUsable usable;
    [FieldOffset(0x00)] public fixed byte raw[40];
}