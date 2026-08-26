using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SomItemProfileData
{
    // Data
    [FieldOffset(0x00), SerializeField] SomItemProfileDataUsable usable;
    [FieldOffset(0x00), SerializeField] SomItemProfileDataWeapon weapon;
    [FieldOffset(0x00), SerializeField] public SomItemProfileDataArmour armour;
    [FieldOffset(0x00), SerializeField] public fixed byte raw[16];

    /// <summary>
    /// Data for usable type items
    /// </summary>
    public SomItemProfileDataUsable Usable
    {
        get
        {
            return usable;
        }
    }

    /// <summary>
    /// Data for weapon type items
    /// </summary>
    public SomItemProfileDataWeapon Weapon
    {
        get
        {
            return weapon;
        }
    }

    /// <summary>
    /// Data for armour type items
    /// </summary>
    public SomItemProfileDataArmour Armour
    {
        get
        {
            return armour;
        }
    }
}