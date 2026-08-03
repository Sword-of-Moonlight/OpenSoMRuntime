using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.IO;

public class ModelResource : BaseResource<Mesh>
{
    /// <summary>
    /// Static mesh vertex format (32 Bytes)
    /// </summary>
    public readonly static VertexAttributeDescriptor[] StaticVertexFormat = new VertexAttributeDescriptor[]
    {
        new VertexAttributeDescriptor { attribute = VertexAttribute.Position,  format = VertexAttributeFormat.Float32, dimension = 3, stream = 0 },
        new VertexAttributeDescriptor { attribute = VertexAttribute.Normal,    format = VertexAttributeFormat.UInt32,  dimension = 1, stream = 0 },
        new VertexAttributeDescriptor { attribute = VertexAttribute.Color,     format = VertexAttributeFormat.UInt8,   dimension = 4, stream = 0 },
        new VertexAttributeDescriptor { attribute = VertexAttribute.TexCoord0, format = VertexAttributeFormat.Float32, dimension = 2, stream = 0 },
    };

    /// <summary>Default materials. Not avaliable if "CreateDefaultMaterials" is set to false when loading.</summary>
    public Material[] Materials { get; private set; }

    /// <summary>
    /// Default mapping of meshes to materials.
    /// </summary>
    public int[] MeshMaterialMapping { get; private set; }

    // Data
    TextureResource[] MaterialTextures;             // Used to store material textures - HOWEVER - Only when CreateDefaultMaterials is set to true.
    ModelStaticVertex[] staticVertexData;           // Used to store static mesh vertex data. Generalized as byte.
    ushort[] indexData;                             // Used to store index data for both static and animated meshes

    ModelMeshDefinition[] meshDefinitions;          // Mesh Definition Data
    ModelMaterialDefinition[] materialDefinitions;  // Material Definition Data

    /// <summary>
    /// Call to load vertex data
    /// </summary>
    public void LoadStaticVertexData(ModelStaticVertex[] staticVertexData)
    {
        if (ResourceState == ResourceState.WaitingForTransfer)
            return;

        this.staticVertexData = staticVertexData;
    }

    /// <summary>
    /// Call to load index data
    /// </summary>
    public void LoadIndexData(ushort[] indexData)
    {
        if (ResourceState == ResourceState.WaitingForTransfer)
            return;

        this.indexData = indexData;
    }

    /// <summary>
    /// Call to load mesh definitions
    /// </summary>
    public void LoadMeshDefinitions(ModelMeshDefinition[] meshDefinitions)
    {
        if (ResourceState == ResourceState.WaitingForTransfer)
            return;

        this.meshDefinitions = meshDefinitions;
    }

    /// <summary>
    /// Call to load material definitions
    /// </summary>
    public void LoadMaterialDefinitions(ModelMaterialDefinition[] materialDefinitions)
    {
        if (ResourceState == ResourceState.WaitingForTransfer)
            return;

        this.materialDefinitions = materialDefinitions;
    }

    /// <summary>
    /// Call to complete loading model data.
    /// </summary>
    public void LoadComplete() =>
        ResourceState = ResourceState.WaitingForTransfer;

    /// <summary>
    /// Returns the number of material definitions inside the model resource.
    /// </summary>
    public int GetMaterialDefinitionCount() => materialDefinitions.Length;

    /// <summary>
    /// Returns a material definition from the model
    /// </summary>
    public ModelMaterialDefinition GetMaterialDefinition(int index) => materialDefinitions[index];

    /// <summary>
    /// Grab the internal resource and return it.<br/>
    /// If the resource is waiting for transfer, it will be transferred first so expect some delay.
    /// </summary>
    /// <returns>The resource.</returns>
    public override Mesh Get()
    {
        ReferenceCount++;

        // If the resource is ready, return it immediately
        if (ResourceState == ResourceState.Ready)
            return resource;

        // If the resource is not ready, and is waiting for transfer 
        if (ResourceState == ResourceState.WaitingForTransfer)
        {
            // Create the unity resource
            try
            {
                //
                // Mesh creation
                //
                if (Parameters == null || ((ModelParameters)Parameters).ModelType == ModelParameterType.Static)
                {
                    // Start creating the resource...
                    resource = new Mesh();

                    // Load vertex buffer
                    resource.SetVertexBufferParams(staticVertexData.Length, StaticVertexFormat);
                    resource.SetVertexBufferData(staticVertexData, 0, 0, staticVertexData.Length, 0);

                    // Load index buffer
                    resource.SetIndexBufferParams(indexData.Length, IndexFormat.UInt16);
                    resource.SetIndexBufferData(indexData, 0, 0, indexData.Length);

                    // Load meshes...
                    MeshMaterialMapping = new int[meshDefinitions.Length];

                    resource.subMeshCount = meshDefinitions.Length;
                    for (int i = 0; i < meshDefinitions.Length; ++i)
                    {
                        resource.SetSubMesh(i, new SubMeshDescriptor
                        {
                            vertexCount = staticVertexData.Length,
                            baseVertex = 0,
                            firstVertex = 0,
                            indexCount = meshDefinitions[i].indexCount,
                            indexStart = meshDefinitions[i].indexStart,
                            topology = MeshTopology.Triangles
                        });

                        MeshMaterialMapping[i] = meshDefinitions[i].materialID;
                    }
                }
                else
                    throw new ArgumentException("Non static model types are not currently supported.");

                //
                // Material creation
                //
                if (Parameters == null || ((ModelParameters)Parameters).CreateDefaultMaterials)
                {
                    List<TextureResource> textureResources = new List<TextureResource>();

                    Material[] defaultMaterials = new Material[materialDefinitions.Length];
                   
                    for (int i = 0; i < materialDefinitions.Length; ++i)
                    {
                        // Get the material definition
                        ModelMaterialDefinition materialDefinition = materialDefinitions[i];

                        // Create material
                        Material material = new Material(Shader.Find(GameManager.Instance.RenderStyle.ObjectStatic));

                        // Albedo Texture
                        if (materialDefinition.textureMode != ModelMaterialTextureMode.None)
                        {
                            TextureResource textureResource = null;

                            string resourceOrigin;
                            ulong resourceName;

                            // Textures can be internal to the model (so they're a blob) or a file
                            switch (materialDefinition.textureMode)
                            {
                                case ModelMaterialTextureMode.File:
                                    resourceOrigin = $"{((ModelParameters)Parameters).TextureRootPath}\\{materialDefinition.textureFileName}";

                                    // We won't error - because it doesn't matter really.
                                    if (!ResourceManager.Find(resourceOrigin, out string foundOrigin))
                                        break;

                                    // Load 'n' grab.
                                    resourceName    = ResourceManager.Load<TextureResource>(foundOrigin);
                                    textureResource = ResourceManager.Get<TextureResource>(resourceName); 
                                    break;

                                case ModelMaterialTextureMode.Blob:
                                    // Load 'n' grab.
                                    resourceName    = ResourceManager.Load<TextureResource>(materialDefinition.textureBlob);
                                    textureResource = ResourceManager.Get<TextureResource>(resourceName);
                                    break;
                            }

                            // Assign the texture to the material
                            if (textureResource != null)
                            {
                                // Set the texture for the material
                                material.mainTexture = textureResource.Get();

                                // Store the resource in a list so we can keep reference and free it later.
                                textureResources.Add(textureResource);
                            }
                        }

                        // Albedo Colour Tint
                        material.SetColor("_BaseColor", materialDefinition.colourAlbedo);

                        // TO-DO: Emissive Colour  
                        material.SetColor("_EmissionColor", materialDefinition.colourEmissive);

                        // Blend Mode
                        switch (materialDefinition.blendMode)
                        {
                            case ModelMaterialBlendMode.Additive:
                                material.SetInt("_SrcBlend", (int)BlendMode.One);
                                material.SetInt("_DstBlend", (int)BlendMode.One);
                                material.SetInt("_SrcBlendAlpha", (int)BlendMode.Zero);
                                material.SetInt("_DstBlendAlpha", (int)BlendMode.One);
                                material.SetFloat("_FogMultiplier", 0F);

                                material.renderQueue = (int)RenderQueue.Transparent;
                                break;

                            case ModelMaterialBlendMode.Default:
                                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                                material.SetInt("_SrcBlendAlpha", (int)BlendMode.One);
                                material.SetInt("_DstBlendAlpha", (int)BlendMode.OneMinusSrcAlpha);
                                material.SetFloat("_FogMultiplier", 1F);

                                if (materialDefinition.colourAlbedo.a >= 1)
                                    material.renderQueue = (int)RenderQueue.GeometryLast;
                                else
                                    material.renderQueue = (int)RenderQueue.Transparent;
                                break;
                        }

                        // Enable Instancing
                        material.enableInstancing = true;

                        // Store new material
                        defaultMaterials[i] = material;
                    }

                    // Store any loaded textures so we can free them later.
                    MaterialTextures = textureResources.ToArray();

                    // Store the default materials
                    Materials = defaultMaterials;
                }
            }
            catch
            {
                // Handle our error by forcing the resource state to unloaded
                ResourceState = ResourceState.Unloaded;

                // rethrow the exception without changing the stack location
                throw;
            }

            // Set ready flag now.
            ResourceState = ResourceState.Ready;

            return resource;
        }

        // Return null or a default in any other case
        return null;
    }

    /// <summary>
    /// Free a model resource
    /// </summary>
    public override void Free()
    {
        ReferenceCount--;

        if (ReferenceCount <= 0)
        {
            switch (ResourceState)
            {
                case ResourceState.WaitingForTransfer:
                    ResourceState = ResourceState.Unloaded;
                    break;

                case ResourceState.Ready:

                    // Destroy default materials (if they exist)
                    if (Materials != null)
                    {
                        for (int i = 0; i < Materials.Length; ++i)
                            UnityEngine.Object.Destroy(Materials[i]);
                        Materials = null;

                        // Also need to free and destroy textures...
                        for (int i = 0; i < MaterialTextures.Length; ++i)
                            MaterialTextures[i].Free();
                        MaterialTextures = null;
                    }

                    // Destroy mesh
                    UnityEngine.Object.Destroy(resource);

                    // Set resource to null...
                    resource = null;

                    ResourceState = ResourceState.Unloaded;
                    break;
            }
        }
    }

    #region Utility
    /// <summary>
    /// Packs a normal into 1010102 format
    /// </summary>
    public static uint PackNormal1010102(Vector3 normal)
    {
        int NX = Mathf.RoundToInt(Mathf.Clamp(normal.x, -1f, 1f) * 511f);
        int NY = Mathf.RoundToInt(Mathf.Clamp(normal.y, -1f, 1f) * 511f);
        int NZ = Mathf.RoundToInt(Mathf.Clamp(normal.z, -1f, 1f) * 511f);
        int NW = 0;

        return (uint)((NX & 0x3FF) | ((NY & 0x3FF) << 10) | ((NZ & 0x3FF) << 20) | ((NW & 0x3) << 30));
    }
    #endregion
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ModelStaticVertex
{
    public Vector3 position;
    public uint    normal;
    public Color32 colour;
    public Vector2 texcoord;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ModelMeshDefinition
{
    public int indexCount;  // Number of indices in the mesh
    public int indexStart;  // Offset in the index buffer where mesh indices begin
    public int materialID;  // ID of the material to use
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ModelMaterialDefinition
{
    public ModelMaterialBlendMode blendMode;
    public Color32 colourAlbedo;
    public Color32 colourEmissive;

    public ModelMaterialTextureMode textureMode;
    public string textureFileName;
    public ResourceBlob textureBlob;
}

public enum ModelMaterialBlendMode
{
    Default  = 0,
    Additive = 1
}

public enum ModelMaterialTextureMode
{
    None    = 0,
    File    = 1,
    Blob    = 2
}
