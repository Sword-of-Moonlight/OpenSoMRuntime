using UnityEngine;

using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class MDOFormatHandler : FormatHandler<ModelResource>
{
    public override FormatCapabilities Capabilities => new()
    {
        allowExport  = false,
        allowImport  = true,
        deprecated   = false,   // Sadly...
        experimental = false    
    };

    public override FormatMetadata Metadata => new()
    {
        name        = "Sword of Moonlight [M]o[D]el [O]bject (*.MDO)",
        description = "Proprietary model file format created for Sword of Moonlight: King's Field Making Tool",
        version     = "1.0",
        authors     = new string[] { "FromSoftware" },
        extensions  = new string[] { ".MDO" }
    };

    /// <summary>
    /// Validates the content of a stream as an MDO file.
    /// </summary>
    /// <param name="finStream">A stream containing the data to check</param>
    /// <returns>True if it is, false if it is not</returns>
    public override bool Validate(FileInputStream finStream) => true;   // TO-DO

    /// <summary>
    /// Parses an MDO file
    /// </summary>
    public override bool Load(FileInputStream finStream, in ModelResource resource, ResourceParameters parameters = null)
    {
        // The stream is reused from the validation pass, so it's good practice to seek to the start
        finStream.SeekBegin(0);

        //
        // Texture Block
        //
        int mdoTextureNum = finStream.ReadS32();

        string[] mdoTextureFiles = new string[mdoTextureNum];
        for (int i = 0; i < mdoTextureFiles.Length; ++i)
            mdoTextureFiles[i] = Path.ChangeExtension(finStream.ReadTerminatedString(EncodingExtensions.SJIS).Sanitise(), "txr");

        // Validate if this is needed... I'm not sure it is.
        finStream.Align(4);

        //
        // Material Block
        //
        int mdoMaterialNum = finStream.ReadS32();
        
        MDOMaterial[] mdoMaterials = finStream.ReadStructArray<MDOMaterial>(mdoMaterialNum);

        //
        // Control Point Block
        //
        Vector3[] mdoControlPoints = new Vector3[4];
        for (int i = 0; i < 4; ++i)
            mdoControlPoints[i] = finStream.ReadVector3();

        //
        // Mesh Block
        //
        int mdoMeshCount = finStream.ReadS32();

        MDOMesh[] mdoMeshes = new MDOMesh[mdoMeshCount];
        for (int i = 0; i < mdoMeshes.Length; ++i)
        {
            // Create mesh storage
            MDOMesh mdoMesh = new MDOMesh { };

            // Mesh Header
            mdoMesh.renderFlags    = finStream.ReadU32();
            mdoMesh.textureID      = finStream.ReadS16();
            mdoMesh.materialID     = finStream.ReadS16();
            mdoMesh.indexCount     = finStream.ReadU16();
            mdoMesh.vertexCount    = finStream.ReadU16();
            mdoMesh.indicesOffset  = finStream.ReadU32();
            mdoMesh.verticesOffset = finStream.ReadU32();

            // Mesh Indices
            finStream.Jump(mdoMesh.indicesOffset);
            mdoMesh.indices = finStream.ReadU16Array(mdoMesh.indexCount);
            finStream.Return();

            // Mesh Vertices
            finStream.Jump(mdoMesh.verticesOffset);
            mdoMesh.vertices = finStream.ReadStructArray<MDOVertex>(mdoMesh.vertexCount);
            finStream.Return();

            // Store mesh
            mdoMeshes[i] = mdoMesh;
        }


        //
        // Pass #1 - Combine permutations
        // Any meshes which share the same material, texture and render mode will be merged to their own lists.
        //
        Dictionary<(uint, short, short), List<int>> mergedMeshes = new Dictionary<(uint, short, short), List<int>>();

        for (int i = 0; i < mdoMeshes.Length; ++i)
        {
            // Get mesh and test if it already exists in the mergedMeshes list...
            MDOMesh mdoMesh = mdoMeshes[i];

            if (!mergedMeshes.TryGetValue((mdoMesh.renderFlags, mdoMesh.textureID, mdoMesh.materialID), out List<int> mergedMeshIDs))
            {
                // Create new list because one does not exist...
                mergedMeshIDs = new List<int>();

                // Store list in the dictionary for future lookups
                mergedMeshes[(mdoMesh.renderFlags, mdoMesh.textureID, mdoMesh.materialID)] = mergedMeshIDs;
            }

            mergedMeshIDs.Add(i);
        }

        //
        // Pass #2 - Build Mesh Data
        //

        List<ModelMaterialDefinition> unityMaterialData = new List<ModelMaterialDefinition>();
        List<ModelMeshDefinition> unityMeshData         = new List<ModelMeshDefinition>();
        List<ModelStaticVertex> unityVertexData         = new List<ModelStaticVertex>();
        List<ushort> unityIndexData                     = new List<ushort>();

        foreach((uint, short, short) mergedMeshKey in mergedMeshes.Keys)
        {
            //
            // MDO Material & Texture -> Model Material.
            //
            ModelMaterialDefinition materialDefinition = new ModelMaterialDefinition { };
            materialDefinition.blendMode = (mergedMeshKey.Item1 & 0xFF) != 0 ? ModelMaterialBlendMode.Additive : ModelMaterialBlendMode.Default;

            if (mergedMeshKey.Item2 != -1)
            {
                materialDefinition.textureMode     = ModelMaterialTextureMode.File;
                materialDefinition.textureFileName = mdoTextureFiles[mergedMeshKey.Item2];
            }           
            else
            {
                materialDefinition.textureMode     = ModelMaterialTextureMode.None;
                materialDefinition.textureFileName = string.Empty;
            }
                
            if (mergedMeshKey.Item3 != -1)
            {
                // We must do conversion from F32 format to Color32...
                MDOMaterial mdoMaterial = mdoMaterials[mergedMeshKey.Item3];

                materialDefinition.colourAlbedo = 
                    new Color(
                        mdoMaterial.diffuseR, 
                        mdoMaterial.diffuseG, 
                        mdoMaterial.diffuseB, 
                        mdoMaterial.diffuseA
                        );

                materialDefinition.colourEmissive = 
                    new Color(
                        mdoMaterial.emissiveR * mdoMaterial.emissiveX,
                        mdoMaterial.emissiveG * mdoMaterial.emissiveX,
                        mdoMaterial.emissiveB * mdoMaterial.emissiveX,
                        255
                        );
            }
            else
            {
                materialDefinition.colourAlbedo   = Color.white;
                materialDefinition.colourEmissive = Color.black;
            }

            unityMaterialData.Add(materialDefinition);

            //
            // MDO Mesh Data
            //
            ModelMeshDefinition meshDefinition = new ModelMeshDefinition
            {
                indexStart = unityIndexData.Count,
                materialID = unityMaterialData.Count - 1,
            };

            // Now we must merge each mesh with this key...
            List<int> mergedMeshIndices = mergedMeshes[mergedMeshKey];

            foreach (int meshIndex in mergedMeshIndices)
            {
                MDOMesh mdoMesh = mdoMeshes[meshIndex];

                // First we add the indices, offset bytthe current vertex count...
                for (int i = 0; i < mdoMesh.indexCount; ++i)
                    unityIndexData.Add((ushort)(unityVertexData.Count + mdoMesh.indices[i]));

                // Now we add the vertices as is
                for (int i = 0; i < mdoMesh.vertexCount; ++i)
                {
                    MDOVertex vertex = mdoMesh.vertices[i];

                    unityVertexData.Add(new ModelStaticVertex
                    {
                        position = vertex.position,
                        normal   = ModelResource.PackNormal1010102(vertex.normal.normalized),
                        colour   = Color.white,
                        texcoord = vertex.texcoord
                    });
                }
            }

            meshDefinition.indexCount = unityIndexData.Count - meshDefinition.indexStart;

            unityMeshData.Add(meshDefinition);
        }

        //
        // Loading data into the resource
        //
        resource.LoadStaticVertexData(unityVertexData.ToArray());
        resource.LoadIndexData(unityIndexData.ToArray());
        resource.LoadMaterialDefinitions(unityMaterialData.ToArray());
        resource.LoadMeshDefinitions(unityMeshData.ToArray());
        resource.LoadComplete();

        return true;
    }

    /// <summary>
    /// Material layout for MDO files
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDOMaterial
    {
        [FieldOffset(0x00)] public float diffuseR;
        [FieldOffset(0x04)] public float diffuseG;
        [FieldOffset(0x08)] public float diffuseB;
        [FieldOffset(0x0C)] public float diffuseA;
        [FieldOffset(0x10)] public float emissiveR;
        [FieldOffset(0x14)] public float emissiveG;
        [FieldOffset(0x18)] public float emissiveB;
        [FieldOffset(0x1C)] public float emissiveX;
    }

    /// <summary>
    /// Vertex layout for MDO files
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MDOVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 texcoord;
    }

    /// <summary>
    /// Mesh layout for MDO files
    /// </summary>
    struct MDOMesh
    {
        public uint renderFlags;
        public short textureID;
        public short materialID;
        public ushort indexCount;
        public ushort vertexCount;
        public uint indicesOffset;
        public uint verticesOffset;

        public ushort[] indices;
        public MDOVertex[] vertices;
    }
}