using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "SoMMapData", menuName = "Sword of Moonlight/Map Data")]
public class SoMMapData : ScriptableObject
{
    [field: Header("Meta Data")]
    [field: SerializeField, ReadOnly] public bool IsOptimized { get; private set; } = false;
    [field: SerializeField, ReadOnly] public bool IsLightmapped { get; private set; } = false;
    [field: SerializeField, ReadOnly] public string MapName { get; private set; } = string.Empty;
    [field: SerializeField, ReadOnly] public string MusicFileName { get; private set; } = string.Empty;

    [field: Header("Map Item Images")]
    [field: SerializeField, ReadOnly] public string[] MapImageFilenames { get; private set; }

    [field: Header("Camera Data")]
    [field: SerializeField, ReadOnly] public float CameraFoV;
    [field: SerializeField, ReadOnly] public float CameraZNear;
    [field: SerializeField, ReadOnly] public float CameraZFar;

    [field: Header("Enviroment Data")]
    [field: SerializeField, ReadOnly] public float EnviromentFogDistance;
    [field: SerializeField, ReadOnly] public Color32 EnviromentFogColour;
    [field: SerializeField, ReadOnly] public Color32 EnviromentAmbientColour;
    [field: SerializeField, ReadOnly] public Vector3 EnviromentDirLightADirection;
    [field: SerializeField, ReadOnly] public Color32 EnviromentDirLightAColour;
    [field: SerializeField, ReadOnly] public Vector3 EnviromentDirLightBDirection;
    [field: SerializeField, ReadOnly] public Color32 EnviromentDirLightBColour;
    [field: SerializeField, ReadOnly] public Vector3 EnviromentDirLightCDirection;
    [field: SerializeField, ReadOnly] public Color32 EnviromentDirLightCColour;

    [field: Header("Player Data")]
    [field: SerializeField, ReadOnly] public Vector3 PlayerDefaultStartPosition;
    [field: SerializeField, ReadOnly] public float PlayerDefaultStartDirection;

    [field: Header("World Data")]
    [field: SerializeField, ReadOnly] public uint WorldSkyType;
    [field: SerializeField, ReadOnly] public uint WorldLayers;
    [field: SerializeField, ReadOnly] public uint WorldWidth;
    [field: SerializeField, ReadOnly] public uint WorldHeight;
    [field: SerializeField, ReadOnly] public MapTile[] WorldTiles;
    [field: SerializeField, ReadOnly] public MPXObject[] WorldObjects;

    // Non Inspector Properties
    [field: SerializeField] public UnityEngine.Material[] RenderMaterials { get; private set; }
    [field: SerializeField] public Mesh[] RenderMeshes { get; private set; }
    [field: SerializeField] public BlobAssetReference<Unity.Physics.Collider>[] CollisionMeshes { get; private set; }

    // Tile Vertex Format (32 Bytes!!!)
    readonly static VertexAttributeDescriptor[] TileVertexFormat =
    {
        new VertexAttributeDescriptor(VertexAttribute.Position,     VertexAttributeFormat.Float32, 3, 0),   // Position : 12 Bytes
        new VertexAttributeDescriptor(VertexAttribute.Normal,       VertexAttributeFormat.UInt32,  1, 0),   // Normal   : 4  Bytes
        new VertexAttributeDescriptor(VertexAttribute.Tangent,      VertexAttributeFormat.UInt32,  1, 0),   // Tangent  : 4  Bytes
        new VertexAttributeDescriptor(VertexAttribute.Color,        VertexAttributeFormat.UInt8,   4, 0),   // Colour   : 4  Bytes
        new VertexAttributeDescriptor(VertexAttribute.TexCoord0,    VertexAttributeFormat.Float32, 2, 0)    // Texcoord : 8  Bytes
    };

    TextureResource[] textureResources;

    // Data Definitions
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct MPXObjectFlagsLight
    {
        [FieldOffset(0x00)] public uint colour;
        [FieldOffset(0x04)] public float range;
        [FieldOffset(0x08)] public byte affectObjects;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct MPXObjectFlags
    {
        [FieldOffset(0x00)] public MPXObjectFlagsLight lightFlags;  // 7 of 32 bytes used
        [FieldOffset(0x00)] public fixed byte raw[32];              // Raw Access
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
    public struct MPXObject
    {
        [FieldOffset(0x00)] public short declarationID;     // PR2-PRO ID
        [FieldOffset(0x02)] public byte unkx02;             // Unknown.
        [FieldOffset(0x03)] public byte animating;          // Object is animating by default (traps only. animating is a crap name)
        [FieldOffset(0x04)] public byte visible;            // Object is visible by default
        [FieldOffset(0x05)] public byte unkx05;             // Unknown.
        [FieldOffset(0x06)] public byte unkx06;             // Unknown.
        [FieldOffset(0x07)] public byte unkx07;             // Unknown.
        [FieldOffset(0x08)] public Vector3 position;        // Position of the object
        [FieldOffset(0x14)] public Vector3 rotation;        // Rotation of the object (degrees)
        [FieldOffset(0x20)] public float scale;
        [FieldOffset(0x24)] public MPXObjectFlags flags;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
    struct MPXTile
    {
        [FieldOffset(0x00)] public short collisionMeshID;  // ID of the collision mesh (mhm) to use
        [FieldOffset(0x02)] public short renderMeshID;     // ID of the render mesh (msm) to use. UNUSED! MSM is baked into an MPX graph, so this is a legacy element.
                                                           // ^ If both are -1, the tile is an empty slot.
        [FieldOffset(0x04)] public float elevation;
        [FieldOffset(0x08)] public uint flags;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MPXVertex
    {
        [FieldOffset(0x00)] public float PX;
        [FieldOffset(0x04)] public float PY;
        [FieldOffset(0x08)] public float PZ;
        [FieldOffset(0x0C)] public float TU;
        [FieldOffset(0x10)] public float TV;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MSXTriangle
    {
        [FieldOffset(0x00)] public ushort A;
        [FieldOffset(0x02)] public ushort B;
        [FieldOffset(0x04)] public ushort C;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MSXPacket
    {
        [FieldOffset(0x00)] public uint blockVertexIndex;
        [FieldOffset(0x04)] public uint colour;
    }

    struct MSXMesh
    {
        public MSXPacket[] packets;
        public ushort[] indices;

        public short textureID;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MSXUnityVertex
    {
        public Vector3 position;
        public uint normal;
        public uint tangent;
        public Color32 colour;
        public Vector2 texcoord;
    }

    /// <summary>
    /// Loads map data for a given ID
    /// </summary>
    public void Load(int mapID)
    {
        LoadMPX($"{ResourceManager.ResourceRoot}\\DATA\\MAP\\{mapID:D2}.mpx");
    }

    /// <summary>
    /// Gets the elevation at a position
    /// </summary>
    public float GetElevationFromPosition(float X, float Z)
    {
        // X and Z must be turned into tile positions
        // X and Z span from -1F to 199F, so we must offset before doing the conversion.
        int tileX = Mathf.FloorToInt((1F + X) / 2F);
        int tileZ = Mathf.FloorToInt((1F + Z) / 2F);

        return WorldTiles[(100 * tileX) + tileZ].elevation;
    }

    /// <summary>
    /// Internal.<br/>
    /// Loads an MPX file.
    /// </summary>
    void LoadMPX(string mpxFileName)
    {
        //
        // Read
        //
        using FileInputStream fis = new (mpxFileName);

        ReadMPXHeader(fis);
        ReadMPXCamera(fis);
        ReadMPXEnviroment(fis);
        ReadMPXPlayer(fis);
        WorldObjects = ReadMPXObjects(fis);
        ReadMPXEnemies(fis);
        ReadMPXNPCs(fis);
        ReadMPXItems(fis);

        MPXTile[] tileData     = ReadMPXWorld(fis);
        string[] textureData   = ReadMPXTextures(fis);
        MPXVertex[] vertexData = ReadMPXVertices(fis);
        MSXMesh[][] meshData   = ReadMPXRenderMeshes(fis, tileData);

        // Reading MHM collision data from the MPX.
        CollisionMeshes = ReadMPXCollisionMeshes(fis);

        //
        // Process
        //
        ProcessMPXTileData(tileData, vertexData, meshData, textureData);
    }

    #region MPX/MSX Read and Conversion Helpers
    /// <summary>
    /// Internal.<br/>
    /// Reads MPX Header from a stream.
    /// </summary>
    void ReadMPXHeader(FileInputStream fis)
    {
        uint mpxFlags = fis.ReadU32();

        IsOptimized   = ((mpxFlags >> 0) & 0x1) > 0;
        IsLightmapped = ((mpxFlags >> 1) & 0x1) > 0;

        MapName = fis.ReadFixedString(32, EncodingExtensions.SJIS).Sanitise().TrimEnd();
        MusicFileName = fis.ReadFixedString(32, EncodingExtensions.SJIS).Sanitise();

        MapImageFilenames = new string[3];
        MapImageFilenames[0] = fis.ReadFixedString(32, EncodingExtensions.SJIS).Sanitise();
        MapImageFilenames[1] = fis.ReadFixedString(32, EncodingExtensions.SJIS).Sanitise();
        MapImageFilenames[2] = fis.ReadFixedString(32, EncodingExtensions.SJIS).Sanitise();
    }

    /// <summary>
    /// Internal.<br/>
    /// Reads MPX Camera from a stream.
    /// </summary>
    void ReadMPXCamera(FileInputStream fis)
    {
        CameraFoV   = fis.ReadF32();
        CameraZNear = fis.ReadF32();
        CameraZFar  = fis.ReadF32();
    }

    /// <summary>
    /// Internal.<br/>
    /// Reads MPX Enviroment from a stream.
    /// </summary>
    void ReadMPXEnviroment(FileInputStream fis)
    {
        EnviromentFogDistance   = fis.ReadF32();
        EnviromentFogColour     = fis.ReadColor32_BGRX32();
        EnviromentAmbientColour = fis.ReadColor32_BGRX32();

        EnviromentDirLightAColour    = fis.ReadColor32_BGRX32();
        EnviromentDirLightADirection = fis.ReadVector3();
        EnviromentDirLightBColour    = fis.ReadColor32_BGRX32();
        EnviromentDirLightBDirection = fis.ReadVector3();
        EnviromentDirLightCColour    = fis.ReadColor32_BGRX32();
        EnviromentDirLightCDirection = fis.ReadVector3();

        // Some padding...
        fis.SeekRelative(4);
    }

    /// <summary>
    /// Internal.<br/>
    /// Reads MPX Default Player Start from a stream.
    /// </summary>
    void ReadMPXPlayer(FileInputStream fis)
    {
        PlayerDefaultStartPosition  =  fis.ReadVector3();
        PlayerDefaultStartDirection = -fis.ReadF32();
    }

    /// <summary>
    /// Internal.<br/>
    /// Reads MPX Object List from a stream.
    /// </summary>
    MPXObject[] ReadMPXObjects(FileInputStream fis)
    {
        // Object Count
        int objectCount = fis.ReadS32();

        // Object Data
        int objectUsedNum = 0;

        MPXObject[] objectData = new MPXObject[objectCount];
        for (int i = 0; i < objectCount; ++i)
        {
            objectData[i] = fis.ReadStruct<MPXObject>();

            if (objectData[i].declarationID != -1)
                objectUsedNum++;
        }

        // Object Logging
        Logger.Info($"MPX Objects = {{ Used: {objectUsedNum:D3}/{objectCount:D3} }}");
        
        return objectData;
    }
    
    /// <summary>
    /// Read MPX Enemy list from a stream
    /// </summary>
    void ReadMPXEnemies(FileInputStream fis)
    {
        // Enemy Count
        int enemyCount = fis.ReadS32();

        // Enemy Data
        fis.SeekRelative(0x34 * enemyCount);
    }

    /// <summary>
    /// Read MPX NPC list from a stream
    /// </summary>
    void ReadMPXNPCs(FileInputStream fis)
    {
        // NPC Count
        int npcCount = fis.ReadS32();

        // NPC Data
        fis.SeekRelative(0x34 * npcCount);
    }

    /// <summary>
    /// Read MPX item list from a stream
    /// </summary>
    void ReadMPXItems(FileInputStream fis)
    {
        // Item Count
        int itemCount = fis.ReadS32();

        // Item Data
        fis.SeekRelative(0x28 * itemCount);
    }

    /// <summary>
    /// Read MPX world data from a stream
    /// </summary>
    MPXTile[] ReadMPXWorld(FileInputStream fis)
    {
        // Small Header
        WorldSkyType = fis.ReadU32();
        WorldLayers  = fis.ReadU32();   // Unused by default. We only know it's layers because of context clues.
        WorldWidth   = fis.ReadU32();
        WorldHeight  = fis.ReadU32();

        // Read tiles into an array
        MPXTile[] tileData = new MPXTile[WorldWidth * WorldHeight];

        for (int i = 0; i < tileData.Length; ++i)
            tileData[i] = fis.ReadStruct<MPXTile>();

        // Read world optimization data if required
        if (IsOptimized)
            ReadMPXBSPTree(fis);

        // Return tile data for processing later.
        return tileData;
    }

    /// <summary>
    /// Read MPX BSP data from a stream
    /// </summary>
    void ReadMPXBSPTree(FileInputStream fis)
    {
        // We're just skipping all of this for now, we don't understand it
        // and don't need the data in unity any how.
        //
        // You can find specific information: https://doc.swordofmoonlight.com/editor/ff/map-mpx-file-format/ ...
        // if for some reason you want to try and crack it.
        //

        Logger.Info("Skipping MPX BSP segment...");

        // struct1
        uint countA = fis.ReadU32();
        for (int i = 0; i < countA; ++i)
        {
            fis.SeekRelative(20);

            uint countB = fis.ReadU32();

            for (int j = 0; j < countB; ++j)
                fis.SeekRelative(8);
        }

        // struct2
        uint countC = fis.ReadU32();
        fis.SeekRelative(44 * countC);

        // struct3
        uint countD = fis.ReadU32();
        fis.SeekRelative(24 * countD);

        // weird list (bits?)
        fis.SeekRelative((countA + 7) / 8 * countA);
    }

    /// <summary>
    /// Read MPX Texture file names from a stream
    /// </summary>
    /// <param name="fis"></param>
    string[] ReadMPXTextures(FileInputStream fis)
    {
        // texture count
        int textureCount = fis.ReadS32();

        // texture filenames (fixed here, too)
        string[] textureFilenames = new string[textureCount];
        for (int i = 0; i < textureCount; ++i)
            textureFilenames[i] = Path.ChangeExtension(fis.ReadTerminatedString(EncodingExtensions.SJIS).Sanitise(), "txr");

        return textureFilenames;
    }

    /// <summary>
    /// Read MPX Vertices from a stream
    /// </summary>
    MPXVertex[] ReadMPXVertices(FileInputStream fis)
    {
        // vertex count
        int vertexCount = fis.ReadS32();

        // vertex data
        MPXVertex[] vertexData = new MPXVertex[vertexCount];
        for (int i = 0; i < vertexCount; ++i)
            vertexData[i] = fis.ReadStruct<MPXVertex>();

        return vertexData;
    }

    /// <summary>
    /// Read MSX Render Meshes
    /// </summary>
    MSXMesh[][] ReadMPXRenderMeshes(FileInputStream fis, MPXTile[] tileData)
    {
        //
        // MSX Mesh Reading...
        //
        List<MSXMesh[]> meshData = new List<MSXMesh[]>();

        for (int i = 0; i < tileData.Length; ++i)
        {
            // Get an MPX tile definition
            MPXTile mpxTile = tileData[i];

            // Is this a valid tile?
            if (mpxTile.renderMeshID == -1 && mpxTile.collisionMeshID == -1)
                continue;   // If not, skip it.

            // Read mesh count
            uint msxMeshNum = fis.ReadU32();

            // Read meshes
            MSXMesh[] msxMeshes = new MSXMesh[msxMeshNum];

            for (int j = 0; j < msxMeshNum; ++j)
            {
                // MSX Header
                short msxTextureID  = fis.ReadS16();
                ushort msxIndexNum  = fis.ReadU16();
                ushort msxPacketNum = fis.ReadU16();

                // Read index data
                ushort[] msxIndices = fis.ReadU16Array(msxIndexNum);

                // Read packet data
                MSXPacket[] msxPackets = fis.ReadStructArray<MSXPacket>(msxPacketNum);

                // Create mesh data
                msxMeshes[j] = new MSXMesh
                {
                    textureID = msxTextureID,
                    packets   = msxPackets,
                    indices   = msxIndices,
                };
            }

            meshData.Add(msxMeshes);
        }

        return meshData.ToArray();
    }

    /// <summary>
    /// Responsible for converting MPX tile data to render meshes
    /// </summary>
    void ProcessMPXTileData(MPXTile[] tileData, MPXVertex[] vertexData, MSXMesh[][] meshData, string[] textureData)
    {
        //
        // Material Conversion
        //
        UnityEngine.Material[] mpxMaterials = new UnityEngine.Material[textureData.Length];

        // We must load each texture, and create a material from it...
        textureResources = new TextureResource[mpxMaterials.Length];

        // Get the tile shader. TO-DO: Should come from an external source at some point...
        Shader tileShader = Shader.Find("OpenSoM/Tile (Texture, Lit, Simple)");

        for (int i = 0; i < mpxMaterials.Length; ++i)
        {
            // Load the texture resource...
            ulong resourceName  = ResourceManager.Load<TextureResource>($"{ResourceManager.ResourceRoot}\\DATA\\MAP\\TEXTURE\\{textureData[i]}");
            textureResources[i] = ResourceManager.Get<TextureResource>(resourceName);

            // Create a material using the texture (TO-DO: Optimize this later with a stripped down shader)
            UnityEngine.Material material = new(tileShader);

            // Apply default material properties    
            material.SetTexture("_BaseMap", textureResources[i].Get());
            material.enableInstancing = true;

            mpxMaterials[i] = material;
        }

        RenderMaterials = mpxMaterials;

        //
        // Mesh Conversion
        //
        MapTile[] mpxTiles = new MapTile[tileData.Length];

        // MPX only stores valid tiles = so we need a different counter to get the correct mesh data.
        int validTileID = 0;

        // We use a dictionary and list to store mesh data.
        // The list stores unique meshes, and the dictionary just stores indices into the list by a specific key.
        Dictionary<ulong, int> uniqueMeshKeys = new Dictionary<ulong, int>();
        List<Mesh> uniqueMeshData             = new List<Mesh>();

        // We need to go through MPX default tiles and convert them to our format, at the same time as spitting out only unique meshes.
        for (int i = 0; i < tileData.Length; ++i)
        {
            // Get an MPX tile definition
            MPXTile mpxTile = tileData[i];

            // Is this a valid tile?
            if (mpxTile.renderMeshID == -1 && mpxTile.collisionMeshID == -1)
            {
                mpxTiles[i] = new MapTile { used = false };
                continue;   // Skip any further work.
            }

            // Get the mesh set this tile uses...
            MSXMesh[] msxMeshes = meshData[validTileID];

            // We must now hash the mesh data to see if it is unique.
            ulong meshHash = HashThis.FNV1A_64_OFFSET;
            for (int j = 0; j < msxMeshes.Length; ++j)
            {
                // Get a single mesh...
                MSXMesh msxMesh = msxMeshes[j];

                // Accumulate indices into the hash
                for (int k = 0; k < msxMesh.indices.Length; ++k)
                    meshHash = HashThis.BytesTo64(BitConverter.GetBytes(msxMesh.indices[k]), meshHash);

                // Accumulate vertices into the hash
                for (int k = 0; k < msxMesh.packets.Length; ++k)
                    meshHash = HashThis.BytesTo64(BitConverter.GetBytes(msxMesh.packets[k].blockVertexIndex), meshHash);

                // Accumulate texture ID into the hash
                meshHash = HashThis.BytesTo64(BitConverter.GetBytes(msxMesh.textureID), meshHash);
            }

            // Get a unique index for this mesh...
            if (!uniqueMeshKeys.ContainsKey(meshHash))
            {
                // Mesh not found. Construct new mesh

                //
                // Pass #1: Count up total vertex and index count, create sub mesh descriptors
                //
                SubMeshDescriptor[] unitySubmeshes = new SubMeshDescriptor[msxMeshes.Length];
                int unityMeshVertexCount = 0;
                int unityMeshIndexCount  = 0;

                for (int j = 0; j < msxMeshes.Length; ++j)
                {
                    // Get a single mesh...
                    MSXMesh msxMesh = msxMeshes[j];

                    // We can create the sub mesh descriptor now...
                    unitySubmeshes[j] = new SubMeshDescriptor
                    {
                        vertexCount = msxMesh.packets.Length,
                        baseVertex  = 0,
                        firstVertex = 0,
                        indexCount  = msxMesh.indices.Length,
                        indexStart  = unityMeshIndexCount,
                        topology    = MeshTopology.Triangles
                    };

                    unityMeshVertexCount += msxMesh.packets.Length;
                    unityMeshIndexCount += msxMesh.indices.Length;
                }

                //
                // Pass #2: Construct Mesh Data (Vertex, Texcoord)
                //
                NativeArray<MSXUnityVertex> unityMeshVertices = new NativeArray<MSXUnityVertex>(unityMeshVertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                NativeArray<ushort> unityMeshIndices          = new NativeArray<ushort>(unityMeshIndexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

                int vertexOffset = 0, indexOffset = 0;
                for (int j = 0; j < msxMeshes.Length; ++j)
                {
                    MSXMesh msxMesh = msxMeshes[j];

                    // MPX-MSX Vertex -> MSXUnityVertex
                    for (int k = 0; k < msxMesh.packets.Length; ++k)
                    {
                        // Get vertex from the global block
                        MPXVertex mpxVertex = vertexData[msxMesh.packets[k].blockVertexIndex];

                        // Move it local
                        unityMeshVertices[vertexOffset + k] = new MSXUnityVertex
                        {
                            position = new Vector3(mpxVertex.PX, mpxVertex.PY, mpxVertex.PZ),
                            texcoord = new Vector2(mpxVertex.TU, mpxVertex.TV),
                            normal   = 0,
                            tangent  = 0,
                            colour   = new Color32(255, 255, 255, 255)
                        };
                    }

                    // MPX-MSX Index -> ushort
                    for (int k = 0; k < msxMesh.indices.Length; ++k)
                    {
                        unityMeshIndices[indexOffset + k] = (ushort)(vertexOffset + msxMesh.indices[k]);
                    }

                    // Increment vertex-index offsets
                    vertexOffset += msxMesh.packets.Length;
                    indexOffset  += msxMesh.indices.Length;
                }

                //
                // Pass #3: Normal Generation
                //
                Vector3[] unityMeshNormals = new Vector3[unityMeshVertices.Length];
                for (int j = 0; j < unityMeshIndexCount; j += 3)
                {
                    int I0 = unityMeshIndices[j + 0];
                    int I1 = unityMeshIndices[j + 1];
                    int I2 = unityMeshIndices[j + 2];

                    Vector3 P0 = unityMeshVertices[I0].position;
                    Vector3 P1 = unityMeshVertices[I1].position;
                    Vector3 P2 = unityMeshVertices[I2].position;

                    Vector3 E1 = P1 - P0;
                    Vector3 E2 = P2 - P0;

                    Vector3 FN = Vector3.Cross(E1, E2);

                    unityMeshNormals[I0] += FN;
                    unityMeshNormals[I1] += FN;
                    unityMeshNormals[I2] += FN;
                }

                for (int j = 0; j < unityMeshVertexCount; ++j)
                {
                    // Read vertex
                    MSXUnityVertex vertex = unityMeshVertices[j];

                    // Packing...
                    Vector3 normal = unityMeshNormals[j].normalized;

                    int NX = Mathf.RoundToInt(Mathf.Clamp(normal.x, -1f, 1f) * 511f);
                    int NY = Mathf.RoundToInt(Mathf.Clamp(normal.y, -1f, 1f) * 511f);
                    int NZ = Mathf.RoundToInt(Mathf.Clamp(normal.z, -1f, 1f) * 511f);
                    int NW = 0;

                    vertex.normal = (uint)((NX & 0x3FF) | ((NY & 0x3FF) << 10) | ((NZ & 0x3FF) << 20) | ((NW & 0x3) << 30));

                    // Write vertex
                    unityMeshVertices[j] = vertex;
                }

                //
                // Pass #4: Tangent Generation
                //

                //
                // Mesh Construction
                //
                Mesh unityMesh = new ();

                // Vertex Buffer
                unityMesh.SetVertexBufferParams(unityMeshVertexCount, TileVertexFormat);
                unityMesh.SetVertexBufferData(unityMeshVertices, 0, 0, unityMeshVertexCount, 0, MeshUpdateFlags.Default);

                // Index Buffer
                unityMesh.SetIndexBufferParams(unityMeshIndexCount, IndexFormat.UInt16);
                unityMesh.SetIndexBufferData(unityMeshIndices, 0, 0, unityMeshIndexCount, MeshUpdateFlags.Default);

                // Meshes
                unityMesh.subMeshCount = unitySubmeshes.Length;
                unityMesh.SetSubMeshes(unitySubmeshes);

                // Add unique mesh to list...
                uniqueMeshData.Add(unityMesh);
                uniqueMeshKeys.Add(meshHash, uniqueMeshData.Count - 1);
            }

            // Create the tile data...
            mpxTiles[i] = new MapTile
            {
                meshID      = uniqueMeshKeys[meshHash],
                materialIDs = Array.ConvertAll(msxMeshes.Select(x => x.textureID).ToArray(), y => (int)y),

                colliderID  = mpxTile.collisionMeshID,

                elevation   = mpxTile.elevation,
                rotation    = (-90F * Mathf.Deg2Rad) * (mpxTile.flags & 0x3),

                used        = true
            };

            validTileID++;
        }

        RenderMeshes = uniqueMeshData.ToArray();
        WorldTiles   = mpxTiles;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MHMHeader
    {
        [FieldOffset(0x00)] public uint numVertex;  // Total number of vertices stored in the MHM
        [FieldOffset(0x04)] public uint numNormal;  // Total number of normals stored in the MHM
        [FieldOffset(0x08)] public uint numPacket;  // Total number of packets stored in the MHM
        [FieldOffset(0x0C)] public uint numAAXZ;    // Axis Aligned Walls	(Check for normals facing forward,backward,left or right)
        [FieldOffset(0x10)] public uint numAAY;     // Axis Aligned Floors	(Check for normals facing up, down)
        [FieldOffset(0x14)] public uint numNAAXYZ;  // Non Axis Aligned		(Any that fail above checks end up in here)
    };

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MHMPacket
    {
        [FieldOffset(0x00)] public uint resolutionMode;   // 0 for NAAXYZ type packets, 1 for AAXZ and AAY type packets
        [FieldOffset(0x04)] public Vector3 aabbMin;
        [FieldOffset(0x10)] public Vector3 aabbMax;
        [FieldOffset(0x1C)] public uint normalIndex;
        [FieldOffset(0x20)] public uint numIndices;
    };

    BlobAssetReference<Unity.Physics.Collider>[] ReadMPXCollisionMeshes(FileInputStream fis)
    {
        List<BlobAssetReference<Unity.Physics.Collider>> collisionMeshes = new List<BlobAssetReference<Unity.Physics.Collider>>();

        // We define our MHM collider filter here
        CollisionFilter tileFilter = new CollisionFilter
        {
            BelongsTo = 1u << 0, // Environment
            CollidesWith = ~0u   // Collides with everything
        };

        // MHM Count
        int mhmCount = fis.ReadS32();

        // MHM Meshes
        for (int i = 0; i < mhmCount; ++i)
        {
            // Each MHM in an MPX is prefixed with a size... We don't need it, though
            int mhmSize = fis.ReadS32();

            // Read MHM Header
            MHMHeader mhmHeader = fis.ReadStruct<MHMHeader>();

            // Read MHM Vertices
            Vector3[] mhmVertex = fis.ReadStructArray<Vector3>((int)mhmHeader.numVertex);

            // Read MHM Normals
            Vector3[] mhmNormal = fis.ReadStructArray<Vector3>((int)mhmHeader.numNormal);

            // Read MHM Packets
            MHMPacket[] mhmPackets = fis.ReadStructArray<MHMPacket>((int)mhmHeader.numPacket);

            // Read Indices
            int numIndices = 0;
            for (int j = 0; j < mhmPackets.Length; ++j)
                numIndices += (int)mhmPackets[j].numIndices;

            int[] mhmIndices = fis.ReadS32Array(numIndices);

            // We must now convert the MHM to a Unity mesh...
            List<int> triangleIndices = new List<int>();

            int indicesUsed = 0;
            foreach (MHMPacket packet in mhmPackets)
            {
                switch (packet.numIndices)
                {
                    case 3:
                        triangleIndices.AddRange(new int[] { mhmIndices[indicesUsed + 0], mhmIndices[indicesUsed + 1], mhmIndices[indicesUsed + 2] });
                        break;

                    case 4:
                        triangleIndices.AddRange(new int[] { mhmIndices[indicesUsed + 0], mhmIndices[indicesUsed + 1], mhmIndices[indicesUsed + 2] });
                        triangleIndices.AddRange(new int[] { mhmIndices[indicesUsed + 2], mhmIndices[indicesUsed + 3], mhmIndices[indicesUsed + 0] });
                        break;

                    default:
                        throw new Exception("Invalid MHM packet index count!");
                }

                indicesUsed += (int)packet.numIndices;
            }

            // Now create the the collider mesh
            collisionMeshes.Add(CreateMeshCollider(mhmVertex, triangleIndices.ToArray(), tileFilter));
        }

        return collisionMeshes.ToArray();
    }

    BlobAssetReference<Unity.Physics.Collider> CreateMeshCollider(Vector3[] vertices, int[] triangleIndices, CollisionFilter filter)
    {
        int vertexCount   = vertices.Length;
        int triangleCount = triangleIndices.Length / 3;

        // 1. Convert Vector3[] to NativeArray<float3>
        NativeArray<float3> physicsVertices = new NativeArray<float3>(vertexCount, Allocator.Temp);
        for (int i = 0; i < vertexCount; i++)
            physicsVertices[i] = vertices[i];

        // 2. Pack index stream into int3 triangles
        NativeArray<int3> physicsTriangles = new NativeArray<int3>(triangleCount, Allocator.Temp);
        for (int i = 0; i < triangleCount; i++)
            physicsTriangles[i] = new int3(triangleIndices[i * 3 + 0], triangleIndices[i * 3 + 1], triangleIndices[i * 3 + 2]);

        // 3. Generate unmanaged mesh collider blob
        return Unity.Physics.MeshCollider.Create(physicsVertices, physicsTriangles, filter, Unity.Physics.Material.Default);
    }
    #endregion
}

/// <summary>
/// Structure stores information about a given tile
/// </summary>
public struct MapTile
{
    public int meshID;          // Visible mesh to use for this tile
    public int[] materialIDs;   // Materials to use for this tile per submesh of the tile

    public int colliderID;      // Collision mesh to use for this tile

    public float elevation;     // Elevation of the tile
    public float rotation;      // Rotation of the tile

    public bool used;           // If the tile is used
}