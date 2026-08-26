using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SomItemParameter
{
    // Data
    [FieldOffset(0x000), SerializeField] short profileId;                  // Index into the PR2 file for base PRF data.
    [FieldOffset(0x002), SerializeField] fixed byte name[31];              // Name
    [FieldOffset(0x021), SerializeField] fixed byte description[241];      // Description
    [FieldOffset(0x112), SerializeField] uint unkx112;              // Unknown Bytes. Always 0?.. Could be more description.
    [FieldOffset(0x116), SerializeField] uint unkx116;              // ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^
    [FieldOffset(0x11A), SerializeField] uint unkx11A;              // ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^
    [FieldOffset(0x11E), SerializeField] uint unkx11E;              // ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^
    [FieldOffset(0x122), SerializeField] byte priority;             // 0 = Default, 1 = Crucial (does not despawn)
    [FieldOffset(0x123), SerializeField] byte unkx123;              // Unknown Bytes. Always 0?
    [FieldOffset(0x124), SerializeField] uint unkx124;              // ^   ^   ^   ^   ^   ^   ^
    [FieldOffset(0x128), SerializeField] SomItemParameterData data; // Data depending on the item type (defined in the profile)

    /// <summary>
    /// The id of the profile of which this item is based
    /// </summary>
    public short ProfileId
    {
        get
        {
            return profileId;
        }
    }

    /// <summary>
    /// Name of the item as the game defines it.
    /// </summary>
    public unsafe string Name
    {
        get
        {
            fixed (byte* ptr = name)
            {
                return EncodingExtensions.SJIS.GetString(ptr, 31).Sanitise();
            }
        }
    }

    /// <summary>
    /// Name of the item as the game defines it.
    /// </summary>
    public unsafe string Description
    {
        get
        {
            fixed (byte* ptr = description)
            {
                return EncodingExtensions.SJIS.GetString(ptr, 241).Sanitise();
            }
        }
    }

    /// <summary>
    /// The priority of the item.
    /// </summary>
    public byte Priority
    {
        get
        {
            return priority;
        }
    }

    /// <summary>
    /// Extended union type data stored depending on the type of item defined by the parent profile.
    /// </summary>
    public SomItemParameterData Data
    {
        get
        {
            return data;
        }
    }
}