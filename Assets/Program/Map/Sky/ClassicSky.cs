using System.Collections.Generic;
using UnityEngine;

public class ClassicSky : BaseSky
{
    [Header("References (Internal)")]
    [SerializeField, ReadOnly] MeshRenderer meshRenderer;
    [SerializeField, ReadOnly] MeshFilter   meshFilter;

    // Data
    ModelResource skyModelResource;
    TextureResource[] skyTextureResources;

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    void Update()
    {
        if (Camera.main != null)
            transform.position = Camera.main.transform.position;
    }

    public void Initialize(MeshRenderer meshRenderer, MeshFilter meshFilter)
    {
        this.meshRenderer = meshRenderer;
        this.meshFilter   = meshFilter;

        // Configure Mesh Renderer
    }

    /// <summary>
    /// Loads a classic SoM sky.
    /// </summary>
    public void LoadClassicSky(int skyID)
    {
        // Load the sky resource...
        ulong resourceName = ResourceManager.Load<ModelResource>($"{ResourceManager.ResourceRoot}\\DATA\\MAP\\MODEL\\SKY{skyID:D2}.mdo", new ModelParameters { ModelType = ModelParameterType.Static, CreateDefaultMaterials = false});

        // Then get it immediately.
        skyModelResource = ResourceManager.Get<ModelResource>(resourceName);
        meshFilter.mesh = skyModelResource.Get();

        // We must now create materials for our sky...
        Material[] materialList = new Material[skyModelResource.GetMaterialDefinitionCount()];

        List<TextureResource> materialTextures = new List<TextureResource>();

        for (int i = 0; i < materialList.Length; ++i)
        {
            // Get material definition...
            ModelMaterialDefinition materialDefinition = skyModelResource.GetMaterialDefinition(i);

            // We want a sky material type
            Material newMaterial;
            if (materialDefinition.blendMode == ModelMaterialBlendMode.Additive)
            {
                newMaterial = new(Shader.Find("OpenSoM/Sky (Texture, Colour, UVScroll, Additive)"));
                newMaterial.renderQueue = 1001; // Always the most forward
            }           
            else
                newMaterial = new(Shader.Find("OpenSoM/Sky (Texture, Colour, UVScroll)"));

            newMaterial.color = materialDefinition.colourAlbedo;

            // We must load the texture and assign it to the material
            if (materialDefinition.textureFileName != string.Empty)
            {
                resourceName = ResourceManager.Load<TextureResource>($"{ResourceManager.ResourceRoot}\\DATA\\MAP\\MODEL\\{materialDefinition.textureFileName}");
                TextureResource skyTextureResource = ResourceManager.Get<TextureResource>(resourceName);
                newMaterial.mainTexture = skyTextureResource.Get();
            }

            materialList[i] = newMaterial;
        }

        // The first material must be set up to scroll...
        materialList[0].SetVector("_ScrollParams", new Vector4(0F, 1F, 0F, 1F)); // DIR.X, DIR.Y, SPEED.X, SPEED.Y

        skyTextureResources = materialTextures.ToArray();

        meshRenderer.materials = materialList;
    }
}
