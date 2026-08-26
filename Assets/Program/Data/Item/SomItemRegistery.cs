using System;
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

    // const, readonly
    public readonly static string ItemDataPath = Path.Combine("DATA", "ITEM");

    // data
    ModelResource[] modelResource;

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

        modelResource = new ModelResource[ProfileCount];

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
    /// Gets data for a specific item, using it's parameter ID
    /// </summary>
    public bool GetData(int itemId, out SomItemProfile profile, out SomItemParameter parameter, out ModelResource model)
    {
        // Safty range check...
        if (itemId < 0 || itemId >= ParameterCount)
            throw new ArgumentOutOfRangeException(nameof(itemId), "Item id was out of range.");

        // Get parameter
        parameter = Parameter[itemId];

        // Get profile
        profile = Profile[parameter.ProfileId];

        // Check if the model assossiated with this profile has been loaded yet...
        if (modelResource[parameter.ProfileId] == null)
        {
            // Find the file path for the model...
            if (!ResourceManager.Find(Path.Combine(ItemDataPath, "MODEL", profile.ModelFile), out string foundModel))
                throw new Exception("Failed to find object model!");

            // Load the resource
            ulong resourceName = ResourceManager.Load<ModelResource>(foundModel, new ModelParameters
            {
                CreateDefaultMaterials = true,
                ModelType = ModelParameterType.Static,
                TextureRootPath = Path.Combine(ItemDataPath, "MODEL")
            });

            // We can now get and store a reference to the model here...
            modelResource[parameter.ProfileId] = ResourceManager.Get<ModelResource>(resourceName);
        }

        // Get model
        model = modelResource[parameter.ProfileId];

        return true;
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
        Parameter = fis.ReadStructArray<SomItemParameter>(ParameterCount);
    }
}