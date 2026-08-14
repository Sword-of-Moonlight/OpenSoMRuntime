using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SomObjectParameterData
{
    // Data
    [FieldOffset(0x00), SerializeField] SomObjectParameterDataTrap trap;
    [FieldOffset(0x00), SerializeField] fixed byte raw[16];

    /// <summary>
    /// Data for traps.
    /// </summary>
    public SomObjectParameterDataTrap Trap
    {
        get
        {
            return trap;
        }
    }
}