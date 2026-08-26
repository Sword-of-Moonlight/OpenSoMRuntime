using System;
using System.IO;

using UnityEngine;

[CreateAssetMenu(fileName = "SomObjectRegistry", menuName = "Sword of Moonlight/Object Registry")]
public class SomObjectRegistry : ScriptableObject
{
    [field: Header("Profile")]
    [field: SerializeField, ReadOnly] public SomObjectProfile[] Profile { get; private set; }
    [field: SerializeField, ReadOnly] public int ProfileCount { get; private set; } = 0;

    [field: Header("Parameter")]
    [field: SerializeField, ReadOnly] public SomObjectParameter[] Parameter { get; private set; }
    [field: SerializeField, ReadOnly] public int ParameterCount { get; private set; } = 0;

    // Data
    ModelResource[] modelResource;

    /// <summary>
    /// Load data for the object registry
    /// </summary>
    public void Load()
    {
        // Load object profiles
        if (ResourceManager.Find(Path.Combine("PARAM", "OBJ.PR2"), out string foundPR2File))
            LoadProfileData(foundPR2File);
        else
            Logger.Critical("Couldn't find OBJ.PR2 file!");

        modelResource = new ModelResource[ProfileCount];

        // Load object parameters
        if (ResourceManager.Find(Path.Combine("PARAM", "OBJ.PRM"), out string foundPRMFile))
            LoadParameterData(foundPRMFile);
        else
            Logger.Critical("Couldn't find OBJ.PRM file!");
    }

    /// <summary>
    /// Free data from the object registry
    /// </summary>
    public void Free()
    {
        // Clear out parameter data
        ParameterCount = 0;
        Parameter = null;

        // Clear out model data
        for (int i = 0; i < ProfileCount; ++i)
        {
            if (modelResource[i] != null)
                modelResource[i].Free();
        }

        // Clear out profile data
        ProfileCount = 0;
        Profile = null;
    }

    /// <summary>
    /// Gets data for a specific object, from its parameter id.
    /// </summary>
    public bool GetData(int objectId, out SomObjectProfile profile, out SomObjectParameter parameter, out ModelResource model)
    {
        // Safty range check...
        if (objectId >= ParameterCount || objectId < 0)
            throw new ArgumentOutOfRangeException(nameof(objectId));

        // Get parameter
        parameter = Parameter[objectId];

        // Get profile
        profile = Profile[parameter.ProfileId];

        // Check if the model assossiated with this profile has been loaded yet...
        if (modelResource[parameter.ProfileId] == null)
        {
            // Find the file path for the model...
            if (!ResourceManager.Find(Path.Combine("DATA", "OBJ", "MODEL", profile.ModelFile), out string foundModel))
                throw new Exception($"Failed to find object model!\n Path = {Path.Combine("DATA", "OBJ", "MODEL", profile.ModelFile)}\n ID = {objectId}\n Prof ID = {parameter.ProfileId}");

            // Load the resource
            ulong resourceName = ResourceManager.Load<ModelResource>(foundModel, new ModelParameters
            {
                CreateDefaultMaterials = true,
                ModelType = ModelParameterType.Static,
                TextureRootPath = Path.Combine("DATA", "OBJ", "MODEL")
            });

            // We can now get and store a reference to the model here...
            modelResource[parameter.ProfileId] = ResourceManager.Get<ModelResource>(resourceName);

            // Here we check if the object should have scrolling textures...
            if (modelResource[parameter.ProfileId].Materials != null && modelResource[parameter.ProfileId].Materials.Length > 1)
            {
                modelResource[parameter.ProfileId].Materials[1].SetVector("_ScrollParams", profile.ScrollMode switch
                {
                    SomObjectTextureScrollType.Horizontal => new Vector4(0F, -1F, 0F, 25F),
                    SomObjectTextureScrollType.Vertical => new Vector4(-1F, 0F, 25F, 0F),
                    _ => Vector4.zero
                });
            }
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
        Profile = fis.ReadStructArray<SomObjectProfile>(ProfileCount);
    }

    /// <summary>
    /// Load parameter data into memory
    /// </summary>
    void LoadParameterData(string prmFile)
    {
        // Open PRM for reading...
        using FileInputStream fis = new FileInputStream(prmFile);

        ParameterCount = 1024;

        // PRM Items
        Parameter = fis.ReadStructArray<SomObjectParameter>(ParameterCount);
    }
}