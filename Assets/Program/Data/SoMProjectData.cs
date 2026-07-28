using UnityEngine;

using System;
using System.Runtime.InteropServices;

[CreateAssetMenu(fileName = "SoMProjectData", menuName = "Sword of Moonlight/SoMProjectData")]
public class SoMProjectData : ScriptableObject
{
    [field: Header("Project Information")]
    [field: SerializeField] public string rootDirectory { get; private set; }


    [field: Header("Sequence Data")]
    [field: SerializeField] public SoMSequence[] sequences { get; private set; }


    [field: Header("Image Data")]
    [field: SerializeField] public string titleBackgroundFile { get; private set; }
    [field: SerializeField] public string closeBackgroundFile { get; private set; }
    [field: SerializeField] public string menuBackgroundFile { get; private set; }


    [field: Header("Game Configuration")]
    [field: SerializeField] public bool enableDash { get; private set; }
    [field: SerializeField] public bool enableEncumbrance { get; private set; }
    [field: SerializeField] public byte initialMap { get; set; }
    [field: SerializeField] public string[] counterName { get; private set; }


    [field: Header("Menu Configuration")]
    [field: SerializeField] public bool allowSaveInMenu { get; private set; }
    [field: SerializeField] public byte defaultCompassType { get; private set; }
    [field: SerializeField] public byte defaultGaugeType { get; private set; }
    [field: SerializeField] public byte defaultMenuStyle { get; private set; }
    [field: SerializeField] public string coinSymbol { get; private set; }
    [field: SerializeField] public byte menuSoundType { get; private set; }


    [field: Header("Player Configuration")]
    [field: SerializeField] public float playerWalkSpeed { get; private set; }
    [field: SerializeField] public float playerDashSpeed { get; private set; }
    [field: SerializeField] public short playerTurnSpeed { get; private set; }
    [field: SerializeField] public byte playerLevelCurveType { get; private set; }
    [field: SerializeField] public SoMPlayerConfig playerConfigNormal { get; private set; }
    [field: SerializeField] public SoMPlayerConfig playerConfigDebug { get; private set; }


    [field: Header("Class Data")]
    [field: SerializeField] public SoMClassData classData { get; private set; }
    

    [field: Header("Spell Data")]
    [field: SerializeField] public SoMSpellData spellData { get; private set; }


    [field: Header("Messages")]
    [field: SerializeField] public string[] messagesA { get; private set; }
    [field: SerializeField] public string[] messagesB { get; private set; }


    [field: Header("Sound Data")]
    [field: SerializeField] public ushort[] systemSoundIDs { get; private set; }
    

    /// <summary>
    /// Loads a SoM Project
    /// </summary>
    public void LoadLegacyProject(string directory)
    {
        // Set root folder...
        rootDirectory = directory;

        LoadLegacySystem($"{directory}\\PARAM\\SYS.DAT");
    }

    /// <summary>
    /// Loads a legacy SoM sys.dat file
    /// </summary>
    /// <param name="filepath"></param>
    void LoadLegacySystem(string filepath)
    {
        // Open file in file stream.
        using FileInputStream fis = new FileInputStream(filepath);

        //
        // Read sequence-image block...
        //
        sequences  = new SoMSequence[(int)SoMSequenceType.Max];
        sequences[(int)SoMSequenceType.Title] = LoadLegacySequence(fis, "TITLE.DAT");
        titleBackgroundFile = fis.ReadFixedString(31, EncodingExtensions.SJIS).Sanitise();
        sequences[(int)SoMSequenceType.Opening] = LoadLegacySequence(fis, "OPENNING.DAT");
        sequences[(int)SoMSequenceType.GameEndA] = LoadLegacySequence(fis, "ENDING1.DAT");
        sequences[(int)SoMSequenceType.GameEndB] = LoadLegacySequence(fis, "ENDING2.DAT");
        sequences[(int)SoMSequenceType.GameEndC] = LoadLegacySequence(fis, "ENDING3.DAT");
        sequences[(int)SoMSequenceType.StaffRoll] = LoadLegacySequence(fis, "STAFF.DAT");
        closeBackgroundFile = fis.ReadFixedString(31, EncodingExtensions.SJIS).Sanitise();

        //
        // Misc #1
        //
        enableDash = (fis.ReadU8() == 1);
        fis.ReadU8();                       // Padding 0x00FF

        //
        // Player Speed
        //
        playerWalkSpeed = fis.ReadF32();
        playerDashSpeed = fis.ReadF32();
        playerTurnSpeed = fis.ReadS16();
        playerLevelCurveType = fis.ReadU8();

        //
        // Class Data
        //
        SoMClassData somClassData = new SoMClassData { names = new string[25] };
        for (int i = 0; i < 25; ++i)
            somClassData.names[i] = fis.ReadFixedString(15, EncodingExtensions.SJIS);
        somClassData.strengthThresholds = fis.ReadU16Array(4);
        somClassData.magicThresholds = fis.ReadU16Array(4);
        classData = somClassData;

        //
        // Spell Data
        //
        SoMSpellData somSpellData = new SoMSpellData { };
        somSpellData.IDs = fis.ReadU8Array(32);
        somSpellData.levelRequired = fis.ReadU8Array(32);
        spellData = somSpellData;

        //
        // 'Menu' configuration
        //
        allowSaveInMenu   = fis.ReadU8() != 0;
        enableEncumbrance = fis.ReadU8() != 0;
        defaultCompassType = fis.ReadU8();
        defaultGaugeType = fis.ReadU8();
        fis.ReadU8();
        defaultMenuStyle = fis.ReadU8();

        //
        // Messages #1
        //
        messagesA = new string[13];
        for (int i = 0; i < 13; ++i)
            messagesA[i] = fis.ReadFixedString(41, EncodingExtensions.SJIS);

        //
        // Coin symbol
        //
        coinSymbol = fis.ReadFixedString(3, EncodingExtensions.SJIS);

        //
        // Player config
        //
        playerConfigNormal = fis.ReadStruct<SoMPlayerConfig>();
        playerConfigDebug  = fis.ReadStruct<SoMPlayerConfig>();

        //
        // Initial map
        //
        initialMap = fis.ReadU8();

        //
        // Counter Names
        //
        counterName = new string[1024];
        for (int i = 0; i < 1024; ++i)
            counterName[i] = fis.ReadFixedString(31, EncodingExtensions.SJIS);

        // Padding...
        fis.ReadU8();

        //
        // System Sounds
        //
        systemSoundIDs = fis.ReadU16Array(16);

        //
        // Menu Background
        //
        menuBackgroundFile = fis.ReadFixedString(38, EncodingExtensions.SJIS);

        //
        // Messages #2
        //
        messagesB = new string[3];
        for (int i = 0; i < 3; ++i)
            messagesB[i] = fis.ReadFixedString(41, EncodingExtensions.SJIS);

        // Menu Sounds
        menuSoundType = fis.ReadU8();

        // Padding...
        fis.ReadU8();
    }

/// <summary>
/// Loads a legacy SoM sequence from a sys.dat file
/// </summary>
SoMSequence LoadLegacySequence(FileInputStream fis, string slideshowFileName)
    {
        SoMSequenceMode sequenceMode = fis.ReadEnum<SoMSequenceMode>();
        string sequenceFile = fis.ReadFixedString(31, EncodingExtensions.SJIS).Sanitise();

        switch (sequenceMode)
        {
            case SoMSequenceMode.None:
                sequenceFile = string.Empty;
                break;

            case SoMSequenceMode.Video:
                sequenceFile = $"\\DATA\\MOVIE\\{sequenceFile}";
                break;

            case SoMSequenceMode.SlideShow:
                sequenceFile = $"\\PARAM\\{slideshowFileName}";
                break;
        }

        return new SoMSequence
        {
            mode = sequenceMode,
            file = sequenceFile
        };
    }
}

[Serializable]
public struct SoMSequence
{
    /// <summary>
    /// Sequence playback mode
    /// </summary>
    public SoMSequenceMode mode;

    /// <summary>
    /// Sequence data file
    /// </summary>
    public string file;
}

public enum SoMSequenceMode : byte
{
    None      = 0,
    Video     = 1,
    SlideShow = 2
}

public enum SoMSequenceType : int
{
    // Original
    Title     = 0x0,
    Opening   = 0x1,
    GameEndA  = 0x2,
    GameEndB  = 0x3,
    GameEndC  = 0x4,
    StaffRoll = 0x5,

    // Additional
    AttractA  = 0x6,
    AttractB  = 0x7,
    AttractC  = 0x8,

    // Max Elements
    Max       = 0x8
}

[Serializable]
public struct SoMClassData
{
    public string[] names;
    public ushort[] strengthThresholds;
    public ushort[] magicThresholds;
}

[Serializable]
public struct SoMSpellData
{
    public byte[] IDs;
    public byte[] levelRequired;
}

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SoMPlayerConfig
{
    [FieldOffset(0x00)] public ushort startStrength;
    [FieldOffset(0x02)] public ushort startMagic;
    [FieldOffset(0x04)] public ushort startHP;
    [FieldOffset(0x06)] public ushort startMP;
    [FieldOffset(0x08)] public uint startCoin;
    [FieldOffset(0x0C)] public uint startExperience;
    [FieldOffset(0x10)] public byte startLevel;
    [FieldOffset(0x11)] public fixed byte startEquipment[8];
    [FieldOffset(0x19)] public fixed byte startInventory[251];
}