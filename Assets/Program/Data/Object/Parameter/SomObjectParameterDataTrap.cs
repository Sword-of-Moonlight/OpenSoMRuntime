using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public struct SomObjectParameterDataTrap
{
    // Data
    [FieldOffset(0x00)] float range;
    [FieldOffset(0x04)] byte slashDamage;
    [FieldOffset(0x05)] byte smashDamage;
    [FieldOffset(0x06)] byte stabDamage;
    [FieldOffset(0x07)] byte fireDamage;
    [FieldOffset(0x08)] byte earthDamage;
    [FieldOffset(0x09)] byte windDamage;
    [FieldOffset(0x0A)] byte waterDamage;
    [FieldOffset(0x0B)] byte holyDamage;
    [FieldOffset(0x0C)] SomObjectTrapStatus statusEffect;
    [FieldOffset(0X0D)] byte statusChance;
    [FieldOffset(0x0E)] byte unkx0E;
    [FieldOffset(0x0F)] byte unkx0F;

    /// <summary>
    /// The range of the trap.
    /// </summary>
    public float Range
    {
        get
        {
            return range;
        }
    }

    /// <summary>
    /// Amount of "slash" damage
    /// </summary>
    public byte SlashDamage
    {
        get
        {
            return slashDamage;
        }
    }

    /// <summary>
    /// Amount of "smash" damage
    /// </summary>
    public byte SmashDamage
    {
        get
        {
            return smashDamage;
        }
    }

    /// <summary>
    /// Amount of "stab" damage
    /// </summary>
    public byte StabDamage
    {
        get
        {
            return stabDamage;
        }
    }

    /// <summary>
    /// Amount of "fire" damage
    /// </summary>
    public byte FireDamage
    {
        get
        {
            return fireDamage;
        }
    }

    /// <summary>
    /// Amount of "earth" damage
    /// </summary>
    public byte EarthDamage
    {
        get
        {
            return earthDamage;
        }
    }

    /// <summary>
    /// Amount of "wind" damage
    /// </summary>
    public byte WindDamage
    {
        get
        {
            return windDamage;
        }
    }

    /// <summary>
    /// Amount of "water" damage
    /// </summary>
    public byte WaterDamage
    {
        get
        {
            return waterDamage;
        }
    }

    /// <summary>
    /// Amount of "holy" damage
    /// </summary>
    public byte HolyDamage
    {
        get
        {
            return holyDamage;
        }
    }

    /// <summary>
    /// A status effect that the trap can apply
    /// </summary>
    public SomObjectTrapStatus StatusEffect
    { 
        get
        {
            return statusEffect;
        }
    }

    /// <summary>
    /// The percent chance of applying a status effect
    /// </summary>
    public byte StatusChance
    {
        get
        {
            return statusChance;
        }
    }

    /// <summary>
    /// Unknown data stored at offset 0x0E in the trap data (one byte)
    /// </summary>
    public byte UnknownX0E
    {
        get
        {
            return unkx0E;
        }
    }

    /// <summary>
    /// Unknown data stored at offset 0x0F in the trap data (one byte)
    /// </summary>
    public byte UnknownX0F
    {
        get
        {
            return unkx0F;
        }
    }
}