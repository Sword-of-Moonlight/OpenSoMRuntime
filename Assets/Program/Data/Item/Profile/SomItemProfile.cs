using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SomItemProfile
{
    // Data
    [FieldOffset(0x00), SerializeField] fixed byte name[31];
    [FieldOffset(0x1F), SerializeField] fixed byte modelFile[31];
    [FieldOffset(0x3E), SerializeField] SomItemType type;
    [FieldOffset(0x40), SerializeField] float menuElevationOffset;
    [FieldOffset(0x44), SerializeField] short menuTilt;
    [FieldOffset(0x46), SerializeField] short worldTilt;
    [FieldOffset(0x48), SerializeField] SomItemProfileData data;

    /// <summary>
    /// Name of the object as the creator defines it
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
    /// Name of the model file for the object
    /// </summary>
    public unsafe string ModelFile
    {
        get
        {
            fixed (byte* ptr = modelFile)
            {
                return EncodingExtensions.SJIS.GetString(ptr, 31).Sanitise();
            }
        }
    }

    /// <summary>
    /// Type of the item
    /// </summary>
    public SomItemType Type
    {
        get
        {
            return type;
        }
    }

    /// <summary>
    /// Elevation of the item model in the menu
    /// </summary>
    public float MenuElevation
    {
        get
        {
            return menuElevationOffset;
        }
    }

    /// <summary>
    /// Tilt of the item model in the menu
    /// </summary>
    public float MenuTilt
    {
        get
        {
            return (menuTilt * Mathf.Deg2Rad);
        }
    }

    /// <summary>
    /// Tilt of the item model in the world (?)
    /// </summary>
    public float WorldTilt
    {
        get
        {
            return (worldTilt * Mathf.Deg2Rad);
        }
    }

    /// <summary>
    /// Extended union type data stored depending on the type of item defined by the parent profile.
    /// </summary>
    public SomItemProfileData Data
    {
        get
        {
            return data;
        }
    }
}
