using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SomObjectParameter
{
    // Data
    [FieldOffset(0x00)] fixed byte name[31];
    [FieldOffset(0x1F)] byte revealed;
    [FieldOffset(0x20)] float scale;
    [FieldOffset(0x24)] short profileId;
    [FieldOffset(0x26)] short unkx26;
    [FieldOffset(0x28)] SomObjectParameterData data;

    /// <summary>
    /// Name of the object as the game defines it.
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
    /// If the object is revealed by special items.
    /// </summary>
    public bool Revealed
    {
        get
        {
            return (revealed == 1);
        }
    }

    /// <summary>
    /// The base scale of the object
    /// </summary>
    public float Scale
    {
        get
        {
            return scale;
        }
    }

    /// <summary>
    /// The id of the profile of which this object is based
    /// </summary>
    public short ProfileId
    {
        get
        {
            return profileId;
        }
    }

    /// <summary>
    /// Unknown data stored at offset 0x26 in each parameter (two bytes)
    /// </summary>
    public short UnknownX26
    {
        get
        {
            return unkx26;
        }
    }

    /// <summary>
    /// Extended union type data stored depending on the type of object defined by the parent profile.
    /// </summary>
    public SomObjectParameterData Data
    {
        get
        {
            return data;
        }
    }
}