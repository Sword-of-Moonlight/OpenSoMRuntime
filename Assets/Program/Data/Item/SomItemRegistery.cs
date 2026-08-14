using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "SomItemRegistry", menuName = "Sword of Moonlight/Item Registry")]
public class SomItemRegistery : ScriptableObject
{
    [field: Header("Profile")]
    [field: SerializeField, ReadOnly] public SomItemProfile[] Profile { get; private set; }
    [field: SerializeField, ReadOnly] public int ProfileCount { get; private set; } = 0;

    [field: Header("Parameter")]
    [field: SerializeField, ReadOnly] public SomItemParameter[] Parameter { get; private set; }
    [field: SerializeField, ReadOnly] public int ParameterCount { get; private set; } = 0;

    /// <summary>
    /// Load data for the item registry
    /// </summary>
    public void Load()
    {
        // Load item profiles
        if (ResourceManager.Find(Path.Combine("PARAM", "ITEM.PR2"), out string foundPR2File))
            LoadProfileData(foundPR2File);
        else
            Logger.Critical("Couldn't find ITEM.PR2 file!");

        // Load item parameters
        if (ResourceManager.Find(Path.Combine("PARAM", "ITEM.PRM"), out string foundPRMFile))
            LoadParameterData(foundPRMFile);
        else
            Logger.Critical("Couldn't find ITEM.PRM file!");
    }

    /// <summary>
    /// Free data from the item registry
    /// </summary>
    public void Free()
    {
        // Clear out parameter data
        ParameterCount = 0;
        Parameter = null;

        // Clear out profile data
        ProfileCount = 0;
        Profile = null;
    }

    /// <summary>
    /// Load profile data into memory
    /// </summary>
    void LoadProfileData(string pr2File)
    {
        // Open PR2 for reading...
        using FileInputStream fis = new FileInputStream(pr2File);

        // PR2 Header...
        ProfileCount = (int)fis.ReadU32();

        // PRF Items...
        Profile = fis.ReadStructArray<SomItemProfile>(ProfileCount);
    }

    /// <summary>
    /// Load parameter data into memory
    /// </summary>
    void LoadParameterData(string prmFile)
    {
        // Open PRM for reading...
        using FileInputStream fis = new FileInputStream(prmFile);

        ParameterCount = 250;

        // PRM Items...
        Parameter = new SomItemParameter[ParameterCount];

        for (int i = 0; i < ParameterCount; ++i)
        {
            SomItemParameter prm = new SomItemParameter();

            prm.profileId   = fis.ReadS16();
            prm.name        = fis.ReadFixedString(031, EncodingExtensions.SJIS).Sanitise();
            prm.description = fis.ReadFixedString(241, EncodingExtensions.SJIS).Sanitise();
            prm.unkx112     = fis.ReadU32();
            prm.unkx116     = fis.ReadU32();
            prm.unkx11A     = fis.ReadU32();
            prm.unkx11E     = fis.ReadU32();
            prm.priority    = fis.ReadU8();
            prm.unkx123     = fis.ReadU8();
            prm.unkx124     = fis.ReadU32();
            prm.data        = fis.ReadStruct<SomItemParameterData>();

            Parameter[i] = prm;
        }
    }
}