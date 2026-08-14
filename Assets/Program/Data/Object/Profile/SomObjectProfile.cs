using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SomObjectProfile
{
    // Data
    [FieldOffset(0x00)] fixed byte name[31];                     // Profile name. S-JIS, 15 usable 2 byte characters + null terminator.
    [FieldOffset(0x1F)] fixed byte modelFile[31];                // Model name.   ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^
    [FieldOffset(0x3E)] public byte billboard;                          // If object is billboard
    [FieldOffset(0x3F)] public byte openable;                           // If object is openable... broken.
    [FieldOffset(0x40)] public float colliderHeight;                    // Collider height
    [FieldOffset(0x44)] public float colliderRW;                        // Collider radius or height
    [FieldOffset(0x48)] public float colliderRD;                        // Collider radius or depth
    [FieldOffset(0x4C)] public float f32x4C;                            // Unknown. Usually 1.0F.
    [FieldOffset(0x50)] public SomObjectColliderType colliderType;      // type of collider
    [FieldOffset(0x51)] SomObjectTextureScrollType scrollMode;   // texture scroll mode
    [FieldOffset(0x52)] public SomObjectType type;                      // The type of object.
    [FieldOffset(0x54)] public short effectId;                          // ID of an effect to use on the object. -1 = None.
    [FieldOffset(0x56)] public byte effectControlPointAnchor;           // Anchor control point.
    [FieldOffset(0x57)] public byte effectAnimationRate;                // Rate of animation.
    [FieldOffset(0x58)] public short loopingSoundId;                    // SFX to play during looping animation
    [FieldOffset(0x5A)] public short openingSoundId;                    // SFX to play during opening animation
    [FieldOffset(0x5C)] public short closingSoundId;                    // SFX to play during closing animation
    [FieldOffset(0x5E)] public byte loopingSoundDelay;                  // Looping SFX delay
    [FieldOffset(0x5F)] public byte openingSoundDelay;                  // Opening SFX delay
    [FieldOffset(0x60)] public byte closingSoundDelay;                  // Closing SFX delay
    [FieldOffset(0x61)] public sbyte loopingSoundPitch;                 // Looping SFX playback pitch (semi tones)
    [FieldOffset(0x62)] public sbyte openingSoundPitch;                 // Opening SFX playback pitch (semi tones)
    [FieldOffset(0x63)] public sbyte closingSoundPitch;                 // Closing SFX playback pitch (semi tones)
    [FieldOffset(0x64)] public short trapEffectId;                      // ID of an effect to use for a trap projectile.
    [FieldOffset(0x66)] public byte trapEffectOrientate;                // If the effect should orientate itself in the same direction as the model?
    [FieldOffset(0x67)] public byte trapEffectVisible;                  // If the effect is visible.
    [FieldOffset(0x68)] public byte loopAnimation;                      // If animation should loop
    [FieldOffset(0x69)] public byte invisible;                          // If the object is invisible.
    [FieldOffset(0x6A)] public byte slotKeyID;                          // Special id which must match with an item's to be able to slot the item into the object
    [FieldOffset(0x6B)] public byte allowXZRotation;                    // If rotation is allowed on the X and Z axis (editor only)

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
    /// Texture scroll mode of the object
    /// </summary>
    public SomObjectTextureScrollType ScrollMode
    {
        get
        {
            return scrollMode;
        }
    }
}