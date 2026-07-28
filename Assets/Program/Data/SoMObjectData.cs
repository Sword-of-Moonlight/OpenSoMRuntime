using System;
using System.Runtime.InteropServices;

using UnityEngine;

[CreateAssetMenu(fileName = "SoMObjectData", menuName = "Sword of Moonlight/SoM Object Data")]
public class SoMObjectData : ScriptableObject
{
    [field: Header("Profile Data")]
    [field: SerializeField, ReadOnly] public SoMObjectProfile[] ObjectProfiles { get; private set; }
    [field: SerializeField, ReadOnly] public int ObjectProfileCount { get; private set; }

    [field: Header("Parameter Data")]
    [field: SerializeField, ReadOnly] public SoMObjectParameter[] ObjectParameters { get; private set; }
    [field: SerializeField, ReadOnly] public int ObjectParameterCount { get; private set; }

    // Data
    ModelResource[] modelResources;

    public bool GetObjectData(int objectID, out SoMObjectProfile profile, out SoMObjectParameter parameters, out ModelResource model)
    {
        if (objectID > ObjectParameterCount)
            throw new ArgumentOutOfRangeException(nameof(objectID));

        // Basic Data for now...
        parameters = ObjectParameters[objectID];
        profile    = ObjectProfiles[parameters.profileID];

        if (modelResources[parameters.profileID] == null)
        {
            // Here we should be loading model data, and _also_ returning that... It should all be stored in here so none goes missing anywhere!!!
            ulong resourceName = ResourceManager.Load<ModelResource>($"{ResourceManager.ResourceRoot}\\DATA\\OBJ\\MODEL\\{profile.modelFile}",
                new ModelParameters
                {
                    CreateDefaultMaterials = true,
                    ModelType = ModelParameterType.Static,
                    TextureRootPath = $"{ResourceManager.ResourceRoot}\\DATA\\OBJ\\MODEL"
                }
            );

            modelResources[parameters.profileID] = ResourceManager.Get<ModelResource>(resourceName);

            // Set up scroll params (if needed)
            if (modelResources[parameters.profileID].Materials != null && modelResources[parameters.profileID].Materials.Length >= 1)
            {
                switch (profile.scrollUVMode)
                {
                    case SoMObjectScrollUVMode.Horizontal:
                        Debug.Log($"Scroll Params Set {profile.name}: Horizontal");
                        modelResources[parameters.profileID].Materials[1].SetVector("_ScrollParams", new Vector4(0F, -1F, 0F, 25F));
                        break;

                    case SoMObjectScrollUVMode.Vertical:
                        Debug.Log($"Scroll Params Set {profile.name}: Vertical");
                        modelResources[parameters.profileID].Materials[1].SetVector("_ScrollParams", new Vector4(-1F, 0F, 25F, 0F));
                        break;
                }
            }
        }

        model = modelResources[parameters.profileID];

        return true;
    }

    /// <summary>
    /// Loads object data
    /// </summary>
    public void Load()
    {
        LoadPR2($"{ResourceManager.ResourceRoot}\\PARAM\\OBJ.PR2");
        LoadPRM($"{ResourceManager.ResourceRoot}\\PARAM\\OBJ.PRM");

        // We will load models here, so create an array which can hold one model for each profile.
        modelResources = new ModelResource[ObjectProfiles.Length];
    }

    public void Free()
    {
        ObjectProfiles   = null;
        ObjectParameters = null;
    }

    /// <summary>
    /// Loads object profiles
    /// </summary>
    void LoadPR2(string filename)
    {
        using FileInputStream fis = new FileInputStream(filename);

        // PR2 Header
        uint prfCount = fis.ReadU32();

        // PRF Data
        ObjectProfiles = new SoMObjectProfile[prfCount];

        for (int i = 0; i < prfCount; ++i)
        {
            SoMObjectProfile prfData = new SoMObjectProfile();

            prfData.name      = fis.ReadFixedString(31, EncodingExtensions.SJIS);
            prfData.modelFile = fis.ReadFixedString(31, EncodingExtensions.SJIS);
            prfData.billboard = fis.ReadU8() == 1;
            prfData.openable  = fis.ReadU8() == 1;
            prfData.colliderHeight = fis.ReadF32();
            prfData.colliderRW     = fis.ReadF32();
            prfData.colliderRD     = fis.ReadF32();
            prfData.f32x4C = fis.ReadF32();
            prfData.colliderMode = fis.ReadEnum<SoMObjectColliderMode>();
            prfData.scrollUVMode = fis.ReadEnum<SoMObjectScrollUVMode>();
            prfData.objectClass = fis.ReadEnum<SoMObjectClass>();
            prfData.effectID                 = fis.ReadS16();
            prfData.effectControlPointAnchor = fis.ReadU8();
            prfData.effectAnimationRate      = fis.ReadU8();
            prfData.loopingSoundFxID = fis.ReadS16();
            prfData.openingSoundFxID = fis.ReadS16();
            prfData.closingSoundFxID = fis.ReadS16();
            prfData.loopingSoundFxDelay = fis.ReadU8();
            prfData.openingSoundFxDelay = fis.ReadU8();
            prfData.closingSoundFxDelay = fis.ReadU8();
            prfData.loopingSoundFxPitch = fis.ReadS8();
            prfData.openingSoundFxPitch = fis.ReadS8();
            prfData.closingSoundFxPitch = fis.ReadS8();
            prfData.trapEffectID = fis.ReadS16();
            prfData.trapEffectOrientate = fis.ReadU8() == 1;
            prfData.trapEffectVisible = fis.ReadU8() == 1;
            prfData.loopAnimation = fis.ReadU8() == 1;
            prfData.invisible = fis.ReadU8() == 1;
            prfData.slotKeyID = fis.ReadU8();
            prfData.allowXZRotation = fis.ReadU8() == 1;

            ObjectProfiles[i] = prfData;
        }

        ObjectProfileCount = (int)prfCount;
    }

    /// <summary>
    /// Load object parameters
    /// </summary>
    void LoadPRM(string filename)
    {
        using FileInputStream fis = new FileInputStream(filename);

        // PRM Data
        ObjectParameters = new SoMObjectParameter[1024];

        for (int i = 0; i < 1024; ++i)
        {
            SoMObjectParameter prmData = new SoMObjectParameter();
            prmData.name = fis.ReadFixedString(31, EncodingExtensions.SJIS);
            prmData.revealed = fis.ReadU8() == 1;
            prmData.scale = fis.ReadF32();
            prmData.profileID = fis.ReadU16();
            prmData.unkx26 = fis.ReadU16();
            prmData.data = fis.ReadStruct<SoMObjectParameterData>();

            ObjectParameters[i] = prmData;
        }

        ObjectParameterCount = 1024;
    }
}

// Data Definitions - Profile
public enum SoMObjectColliderMode : byte
{
    Cylinder = 0,
    Box = 1,
    Unknown = 2
}

public enum SoMObjectScrollUVMode : byte
{
    None = 0,
    Vertical = 1,
    Horizontal = 2
}

public enum SoMObjectClass : short
{
    Static = 0,
    Light = 10,
    DoorSlideUp = 11,
    DoorSwingSingle = 13,
    DoorSwingDouble = 14,
    Container = 20,
    Chest = 21,
    Corpse = 22,
    TrapNormal = 30,
    TrapRanged = 31,
    Switch = 40,
    Pedestal = 41
}

[StructLayout(LayoutKind.Auto, Pack = 1), Serializable]
public struct SoMObjectProfile
{
    public string name;                     // Profile name. S-JIS, 15 usable 2 byte characters + null terminator.
    public string modelFile;                // Model name.   ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^  ^
    public bool billboard;                  // If object is billboard (Ex only?)
    public bool openable;                   // If object is openable (door, chest... Ex only?)

    public float colliderHeight;            // Collider height
    public float colliderRW;                // Collider radius or height
    public float colliderRD;                // Collider radius or depth

    public float f32x4C;                    // Unknown. Usually 1.0F.
    public SoMObjectColliderMode colliderMode; // Mode of the collider.
    public SoMObjectScrollUVMode scrollUVMode; // Mode of scrolling UVs.
    public SoMObjectClass objectClass;         // Object class type.

    public short effectID;                  // ID of an effect to use on the object. -1 = None.
    public byte effectControlPointAnchor;   // Anchor control point.
    public byte effectAnimationRate;        // Rate of animation.

    public short loopingSoundFxID;          // SFX to play during looping animation
    public short openingSoundFxID;          // SFX to play during opening animation
    public short closingSoundFxID;          // SFX to play during closing animation

    public byte loopingSoundFxDelay;        // Looping SFX delay
    public byte openingSoundFxDelay;        // Opening SFX delay
    public byte closingSoundFxDelay;        // Closing SFX delay

    public sbyte loopingSoundFxPitch;       // Looping SFX playback pitch (semi tones)
    public sbyte openingSoundFxPitch;       // Opening SFX playback pitch (semi tones)
    public sbyte closingSoundFxPitch;       // Closing SFX playback pitch (semi tones)

    public short trapEffectID;              // ID of an effect to use for a trap projectile.
    public bool trapEffectOrientate;        // If the effect should orientate itself in the same direction as the model?.. (needs test)
    public bool trapEffectVisible;          // If the effect is visible.

    public bool loopAnimation;              // If animation should loop
    public bool invisible;                  // If the object is invisible.
    public byte slotKeyID;                  // Special ID which must match with an item's to be able to slot the item into the object
    public bool allowXZRotation;            // If rotation is allowed on the X and Z axis (editor only)
}

// Data Definitions - Parameters
public enum SoMObjectTrapStatus : byte
{
    None = 0,
    Poison = 1,
    Paralyse = 2,
    Dark = 3,
    Curse = 4,
    Slow = 5,

    Unknown = 9
}

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public struct SoMObjectParameterDataTrap
{
    [FieldOffset(0x00)] public float range;
    [FieldOffset(0x04)] public byte slashDamage;
    [FieldOffset(0x05)] public byte smashDamage;
    [FieldOffset(0x06)] public byte stabDamage;
    [FieldOffset(0x07)] public byte fireDamage;
    [FieldOffset(0x08)] public byte earthDamage;
    [FieldOffset(0x09)] public byte windDamage;
    [FieldOffset(0x0A)] public byte waterDamage;
    [FieldOffset(0x0B)] public byte holyDamage;
    [FieldOffset(0x0C)] public SoMObjectTrapStatus statusEffect;
    [FieldOffset(0X0D)] public byte statusChance;
    [FieldOffset(0x0E)] public byte unkx0E;
    [FieldOffset(0x0F)] public byte unkx0F;
}

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SoMObjectParameterData
{
    [FieldOffset(0x00)] public SoMObjectParameterDataTrap trapData;
    [FieldOffset(0x00)] public fixed byte raw[16];
}

[StructLayout(LayoutKind.Auto, Pack = 1), Serializable]
public struct SoMObjectParameter
{
    public string name;
    public bool revealed;
    public float scale;
    public ushort profileID;
    public ushort unkx26;
    public SoMObjectParameterData data;
}