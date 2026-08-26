using UnityEngine;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

public partial class MDLFormatHandler : FormatHandler<ModelResource>
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
        name        = "Sword of Moonlight [M]o[D]e[L] (*.MDL)",
        description = "Proprietary model file format created for Sword of Moonlight: King's Field Making Tool",
        version     = "1.0",
        authors     = new string[] { "FromSoftware" },
        extensions  = new string[] { ".MDL" }
    };

    /// <summary>
    /// Validates the content of a stream as an MDL file
    /// </summary>
    /// <param name="finStream">A stream containing the data to check</param>
    /// <returns><b>True</b> if it is, <b>False</b> if it is not</returns>
    public override bool Validate(FileInputStream fis)
    {
        bool streamIsMDL = true;

        // Validation Checks    - TO-DO: Improve this fucking garbage
        MDLHeader headerData = fis.ReadStruct<MDLHeader>();

        streamIsMDL &= ((byte)headerData.flags & 0xF0) == 0;        // Only supported flags are set...
        streamIsMDL &= (headerData.meshDataSize > 0);               // Must be some mesh data...

        return streamIsMDL;
    }

    /// <summary>
    /// Loads MDL data from a stream
    /// </summary>
    public override bool Load(FileInputStream fis, in ModelResource resource, ResourceParameters parameters = null)
    {
        // Stream is reused
        fis.SeekBegin(0);

        //
        // MDL Header
        //
        MDLHeader mdlHeader = fis.ReadStruct<MDLHeader>();

        //
        // MDL Static Data
        //

        // MDL Objects
        MDLObject[] mdlObjects = new MDLObject[mdlHeader.numTmdObject];
        for (int i = 0; i < mdlHeader.numTmdObject; ++i)
        {
            // Read
            MDLObject mdlObject = fis.ReadStruct<MDLObject>();

            // Modify - MDL Object *bases are reduced to units of 4, and don't include the size of the header!
            mdlObject.vertexBase    = 0x10 + (mdlObject.vertexBase * 4);
            mdlObject.normalBase    = 0x10 + (mdlObject.normalBase * 4);
            mdlObject.primitiveBase = 0x10 + (mdlObject.primitiveBase * 4);

            // Store
            mdlObjects[i] = mdlObject;
        }

        // MDL UV Block(s)
        MDLBlockUVS[][] mdlUVBlocks = new MDLBlockUVS[mdlHeader.numUVBlocks][];
        for (int i = 0; i < mdlHeader.numUVBlocks; ++i)
        {
            // Number of Entries in block
            int mdlUVEntries = fis.ReadS32();

            // Now read each uv entry
            mdlUVBlocks[i] = fis.ReadStructArray<MDLBlockUVS>(mdlUVEntries);
        }

        // MDL Texture Data
        MDLTextureContext mdlTextureContext = null;

        if (mdlHeader.numInternalTexture > 0)
        {
            mdlTextureContext = new MDLTextureContext();

            // Seek to start of texture data...
            fis.Jump(0x10 + (4 * (mdlHeader.meshDataSize + mdlHeader.vertexAnimDataSize + mdlHeader.skeletonAnimDataSize)));

            // All MDL textures are stored as TIM.
            for (int i = 0; i < mdlHeader.numInternalTexture; ++i)
                mdlTextureContext.LoadImage(fis);

            fis.Return();
        }

        // Parse each MDL Object
        MDLObjectContext[] mdlObjectContexts = new MDLObjectContext[mdlObjects.Length];

        for (int i = 0; i < mdlObjects.Length; ++i)
        {
            // Get Object
            MDLObject mdlObject = mdlObjects[i];

            // Read Object Vertices
            fis.Jump(mdlObject.vertexBase);
            MDLVector[] mdlObjectVertices = fis.ReadStructArray<MDLVector>(mdlObject.vertexNum);
            fis.Return();

            // Read Object Normals
            fis.Jump(mdlObject.normalBase);
            MDLVector[] mdlObjectNormals  = fis.ReadStructArray<MDLVector>(mdlObject.normalNum);
            fis.Return();

            // We now create a context object to aid in parsing MDL data
            MDLObjectContext mdlObjectContext = new MDLObjectContext(mdlObjectVertices, mdlObjectNormals, mdlUVBlocks);

            // Read MDL Primitives...
            fis.Jump(mdlObject.primitiveBase);

            int totalPrimitiveCount = mdlObject.primitiveNum;

            do
            {
                // MDL Stores a total primitive count in the header, but then individual
                // blocks of seperate primitive types which have their own count.
                //
                // We must decode and accumulate all primitive data.

                // Read the block tag.
                // All primitives under this tag will be identical in type.
                MDLPrimitiveTag primitiveBlockTag = fis.ReadStruct<MDLPrimitiveTag>();

                try
                {
                    if (primitiveBlockTag.type > PrimitiveReadFunc.Length)
                        throw new Exception();

                    for (int j = 0; j < primitiveBlockTag.count; ++j)
                        PrimitiveReadFunc[primitiveBlockTag.type](fis, mdlObjectContext);
                } 
                catch
                {
                    Logger.Critical($"Unsupported Primitive Type '0x{primitiveBlockTag.type:X4}' at offset = 0x{fis.Position:X8}");
                    return false;
                }

                // Decrement the total primitive count with each primitive from a sub block we read
                totalPrimitiveCount -= primitiveBlockTag.count;

            } while (totalPrimitiveCount > 0);

            fis.Return();

            // Store read context
            mdlObjectContexts[i] = mdlObjectContext;
        }

        /**
         * Parse mesh data
        **/
        if (mdlHeader.numVertexAnim == 0 && mdlHeader.numSkeletalAnim == 0)
            ParseStaticMeshes(resource, mdlObjectContexts, mdlTextureContext);
        else
            ParseAnimatedMeshes(resource, mdlObjectContexts, mdlTextureContext);

        resource.LoadComplete();

        return true;
    }

    /// <summary>
    /// Gets a Texture-Material Id from a MDL Triangle, an MDL Object and the MDL Texture Data
    /// </summary>
    void GetTriangleMaterialId(MDLTriangle triangle, MDLObjectContext obj, MDLTextureContext tex, out int materialId, out int textureId)
    {
        materialId = 0;
        textureId = -1;

        // We use this bit to tell that it will use no texture
        if ((triangle.textureData & 0x10000) != 0)
            return;

        // First get the texture page offsets requested by the triangle
        int tpageX = 64  * (int)((triangle.textureData & 0x0F) >> 0);
        int tpageY = 256 * (int)((triangle.textureData & 0x10) >> 4);
        int tbpp   = (int)(triangle.textureData >> 7) & 0x3;

        // We can now get the first UV, which is the only one used in the texture Id calculation in SoM.
        // These are offset with the base tpage position
        int pu = tbpp switch
        {
            0 => obj.Texcoords[(int)(triangle.texcoordIndices & 0xFFFFUL)].U >> 2,
            1 => obj.Texcoords[(int)(triangle.texcoordIndices & 0xFFFFUL)].U >> 1,
            _ => obj.Texcoords[(int)(triangle.texcoordIndices & 0xFFFFUL)].U
        };
        pu += tpageX;

        int pv   = obj.Texcoords[(int)(triangle.texcoordIndices & 0xFFFFUL)].V;
        pv += tpageY;

        // Okay... Now we need to do a rectangle test on our textures.
        // SoM starts with the last texture in the list, so we will too...
        for (int i = tex.images.Count - 1; i >= 0; --i)
        {
            materialId = 1 + i; 
            textureId  = i;

            // Getting texture data...
            MDLTextureContext.ImageData imageData = tex.images[textureId];

            int loadX = imageData.dataSurface.loadX;
            int loadY = imageData.dataSurface.loadY;
            int loadW = imageData.dataSurface.loadW;
            int loadH = imageData.dataSurface.loadH;

            if ((loadX <= pu) && (pu < loadW + loadX) && (loadY <= pv) && (pv < loadH + loadY))
                return;
        }

        materialId = 1;
        textureId  = 0;
    }
   
    /// <summary>
    /// Modifies a raw MDL UV to fit the texture applied to it.
    /// </summary>
    Vector2 GetVertexUV(MDLTriangle triangle, MDLTexcoord texcoord, MDLTextureContext tex, int textureId)
    {
        // We use this bit to tell that it will use no texture
        if ((triangle.textureData & 0x10000) != 0)
            return Vector2.zero;

        // First get the texture page offsets requested by the triangle
        int tpageX = 64  * (int)((triangle.textureData & 0x0F) >> 0);
        int tpageY = 256 * (int)((triangle.textureData & 0x10) >> 4);
        int tbpp   = (int)(triangle.textureData >> 7) & 0x3;

        // Now run the actual calculations...
        Vector2 uv = new Vector2(tpageX, tpageY);

        uv.x += tbpp switch
        {
            0 => texcoord.U * 0.25F,
            1 => texcoord.U * 0.50F,
            _ => texcoord.U
        };

        uv.y += texcoord.V;

        // Now we run the scaling by the texture...
        uv.x = (uv.x - tex.images[textureId].dataSurface.loadX) / (float)tex.images[textureId].dataSurface.loadW;
        uv.y = (uv.y - tex.images[textureId].dataSurface.loadY) / (float)tex.images[textureId].dataSurface.loadH;

        return uv;
    }

    void ParseStaticMeshes(ModelResource resource, MDLObjectContext[] objects, MDLTextureContext texture)
    {
        // Create material list
        // This part should actually be done inside the meshing loop, and account for the blend mode
        // info provided in each MDL Prim.
        List<ModelMaterialDefinition> materials       = new List<ModelMaterialDefinition>();

        // Default "No Texture" material
        materials.Add(new ModelMaterialDefinition
        {
            textureMode     = ModelMaterialTextureMode.None,
            textureBlob     = null,
            textureFileName = string.Empty,
            blendMode       = ModelMaterialBlendMode.Default,
            colourAlbedo    = Color.white,
            colourEmissive  = new Color32(0, 0, 0, 0)
        });

        // Per texture materials
        for (int i = 0; i < texture.images.Count; ++i)
        {
            MDLTextureContext.ImageData imageData = texture.images[i];

            materials.Add(new ModelMaterialDefinition
            {
                textureMode = ModelMaterialTextureMode.Blob,
                textureBlob = new ResourceBlob 
                {
                    Buffer        = imageData.raw, 
                    VirtualOrigin = imageData.virtualName
                },
                textureFileName = ".tim",
                blendMode       = ModelMaterialBlendMode.Default,
                colourAlbedo    = Color.white,
                colourEmissive  = new Color32(0, 0, 0, 0)
            });
        }

        resource.LoadMaterialDefinitions(materials.ToArray());

        // Parsing mesh data
        // Total bloody mess
        Dictionary<int, (int, List<ModelStaticVertex>, List<ushort>)> meshes = new Dictionary<int, (int, List<ModelStaticVertex>, List<ushort>)>();

        // Parsing of actual mesh data
        foreach (MDLObjectContext context in objects)
        {
            // We must loop over each triangle in the MDL...
            for (int i = 0; i < context.Triangles.Count; ++i)
            {
                // Get MDL Triangle data...
                MDLTriangle triangle = context.Triangles[i];

                // Figure out the material ID for the triangle...
                GetTriangleMaterialId(triangle, context, texture, out int materialId, out int textureId);

                // We'll get mesh data for
                if (!meshes.TryGetValue(materialId, out (int, List<ModelStaticVertex>, List<ushort>) mesh))
                {
                    mesh = (materialId, new List<ModelStaticVertex>(), new List<ushort>());
                    mesh.Item1 = materialId;
                }

                // Get each index, convert it into the actual data...
                for (int j = 0; j < 3; ++j)
                {
                    // Getting the vertex data for the current index (j)
                    MDLVector position = context.Vertices[(int)((triangle.vertexIndices >> (16 * j)) & 0xFFFFUL)];
                    MDLVector normal = context.Normals[(int)((triangle.normalIndices >> (16 * j)) & 0xFFFFUL)];
                    MDLColour colour = context.Colours[(int)((triangle.colourIndices >> (16 * j)) & 0xFFFFUL)];
                    MDLTexcoord texcoord = context.Texcoords[(int)((triangle.texcoordIndices >> (16 * j)) & 0xFFFFUL)];

                    // We now handle the conversion of this vertices data...
                    Vector3 unityPosition = new Vector3(position.VX / 1024F, -position.VY / 1024F, position.VZ / 1024F);
                    Vector3 unityNormal   = new Vector3(normal.VX, -normal.VY, normal.VZ).normalized; // SoM also does this instead of dividing each component by 4096...
                    Color32 unityColour   = new Color32(colour.R, colour.G, colour.B, colour.A);
                    Vector2 unityTexcoord = GetVertexUV(triangle, texcoord, texture, textureId);

                    // Processing the texture coordinate...
                    ModelStaticVertex unityVertex =
                        new ModelStaticVertex
                        {
                            position = unityPosition,
                            normal   = ModelResource.PackNormal1010102(unityNormal),
                            colour   = unityColour,
                            texcoord = unityTexcoord
                        };

                    mesh.Item2.Add(unityVertex);
                    mesh.Item3.Add((ushort)(mesh.Item2.Count - 1));
                }

                // Store in meshes list
                meshes[materialId] = mesh;
            }
        }

        // Now the meshes have been pharsed, we can place them into our ModelResource...
        List<ModelMeshDefinition> meshDefs = new List<ModelMeshDefinition>();
        List<ModelStaticVertex> vertices   = new List<ModelStaticVertex>();
        List<ushort> indices               = new List<ushort>();

        foreach (KeyValuePair<int, (int, List<ModelStaticVertex>, List<ushort>)> kvp in meshes)
        {
            meshDefs.Add(new ModelMeshDefinition
            {
                indexStart = indices.Count,
                indexCount = kvp.Value.Item3.Count,
                materialID = kvp.Value.Item1
            });

            for (int i = 0; i < kvp.Value.Item3.Count; ++i)
                indices.Add((ushort)(vertices.Count + kvp.Value.Item3[i]));

            vertices.AddRange(kvp.Value.Item2);
        }

        resource.LoadStaticVertexData(vertices.ToArray());
        resource.LoadIndexData(indices.ToArray());
        resource.LoadMeshDefinitions(meshDefs.ToArray());
    }

    void ParseAnimatedMeshes(ModelResource resource, MDLObjectContext[] objects, MDLTextureContext texture) =>
        ParseStaticMeshes(resource, objects, texture);


    #region MDL Primitive Read Helper
    Action<FileInputStream, MDLObjectContext>[] PrimitiveReadFunc = new Action<FileInputStream, MDLObjectContext>[]
    {
        // Named primitive types    - These are ones that names could be found for (from MapComp)
        ReadPrimitiveFC30,   // FC30             Flat, Colour, Tri
        ReadPrimitiveFT30,   // FT30             Flat, Texture, Tri
        ReadPrimitiveStub,   // FG30             Flat, Gradiant, Tri                Unused in SoM (though it does support skipping over them...)

        ReadPrimitiveStub,   // GC30             Smooth, Colour, Tri
        ReadPrimitiveGT30,   // GT30             Smooth, Texture, Tri   
        ReadPrimitiveStub,   // GG30             Smooth, Gradiant, Tri              Unused in SoM (though it does support skipping over them...)

        ReadPrimitiveStub,   // FC40             Flat, Colour, Quad
        ReadPrimitiveFT40,   // FT40             Flat, Texture, Quad
        ReadPrimitiveStub,   // FG40             Flat, Gradiant, Quad               Unused in SoM (though it does support skipping over them...)

        ReadPrimitiveStub,   // GC40             Smooth, Colour, Quad
        ReadPrimitiveGT40,   // GT40             Smooth, Texture, Quad
        ReadPrimitiveStub,   // GG40             Smooth, Gradiant, Quad

        ReadPrimitiveStub,   // FT31             Flat, Texture, Tri, Unlit           Not supported by som_rt? (it's null)
        ReadPrimitiveFT31,   // GT31             Smooth, Texture, Tri, Unlit
        ReadPrimitiveStub,   // FT41             Flat, Texture, Quad, Unlit          Not supported by som_rt? (it's null)
        ReadPrimitiveStub,   // GT41             Smooth, Texture, Quad, Unlit 

        // Unnamed primitive types
        ReadPrimitiveStub,   // ----UV           ? ? ? ? ? ?, External UV            Not supported by som_rt? (it's null)
        ReadPrimitiveFT41,   // FT41             Smooth, Texture, Quad, Unlit 
        ReadPrimitiveStub,   // ----UV           ? ? ? ? ? ?, External UV            Not supported by som_rt? (it's null)
        ReadPrimitiveStub    // ----UV           ? ? ? ? ? ?, External UV
    };

    static void ReadPrimitiveFC30(FileInputStream fis, MDLObjectContext context)
    {
        // PSX Equivalent = FC3 (0x20 0x00 0x03 0x04)
        MDLPrimitiveFC30 fc3 = fis.ReadStruct<MDLPrimitiveFC30>();

        ushort ci0 = context.AddUniqueColour(new MDLColour { R = fc3.red, G = fc3.green, B = fc3.blue, A = 255 });
        ushort ti0 = context.AddUniqueTexcoord(new MDLTexcoord { U = 0, V = 0 });

        context.AddTriangles(
            new MDLTriangle {
                vertexIndices   = ((ulong)fc3.vertex2 << 32) | ((ulong)fc3.vertex1 << 16) | ((ulong)fc3.vertex0 << 00),
                normalIndices   = ((ulong)fc3.normal0 << 32) | ((ulong)fc3.normal0 << 16) | ((ulong)fc3.normal0 << 00),
                colourIndices   = ((ulong)ci0 << 32) | ((ulong)ci0 << 16) | ((ulong)ci0 << 00),
                texcoordIndices = ((ulong)ti0 << 32) | ((ulong)ti0 << 16) | ((ulong)ti0 << 00),
                textureData     = 0x10000
            }
        );;
    }

    static void ReadPrimitiveFT30(FileInputStream fis, MDLObjectContext context)
    {
        // PSX Equivalent = FT3 (0x24 0x00 0x05 0x07)
        MDLPrimitiveFT30 ft3 = fis.ReadStruct<MDLPrimitiveFT30>();

        ushort ci0 = context.AddUniqueColour(new MDLColour { R = 255, G = 255, B = 255, A = 255 });
        ushort ti0 = context.AddUniqueTexcoord(new MDLTexcoord { U = ft3.u0, V = ft3.v0 });
        ushort ti1 = context.AddUniqueTexcoord(new MDLTexcoord { U = ft3.u1, V = ft3.v1 });
        ushort ti2 = context.AddUniqueTexcoord(new MDLTexcoord { U = ft3.u2, V = ft3.v2 });

        context.AddTriangles(
            new MDLTriangle {
                vertexIndices   = ((ulong)ft3.vertex2 << 32) | ((ulong)ft3.vertex1 << 16) | ((ulong)ft3.vertex0 << 00),
                normalIndices   = ((ulong)ft3.normal0 << 32) | ((ulong)ft3.normal0 << 16) | ((ulong)ft3.normal0 << 00),
                colourIndices   = ((ulong)ci0 << 32) | ((ulong)ci0 << 16) | ((ulong)ci0 << 00),
                texcoordIndices = ((ulong)ti2 << 32) | ((ulong)ti1 << 16) | ((ulong)ti0 << 00),
                textureData     = ft3.tsb
            }
        );
    }

    static void ReadPrimitiveGT30(FileInputStream fis, MDLObjectContext context)
    {
        // PSX Equivalent = GT3 (0x34 0x00 0x06 0x09)
        MDLPrimitiveGT30 gt3 = fis.ReadStruct<MDLPrimitiveGT30>();

        ushort ci0 = context.AddUniqueColour(new MDLColour { R = 255, G = 255, B = 255, A = 255 });
        ushort ti0 = context.AddUniqueTexcoord(new MDLTexcoord { U = gt3.u0, V = gt3.v0 });
        ushort ti1 = context.AddUniqueTexcoord(new MDLTexcoord { U = gt3.u1, V = gt3.v1 });
        ushort ti2 = context.AddUniqueTexcoord(new MDLTexcoord { U = gt3.u2, V = gt3.v2 });

        context.AddTriangles(
            new MDLTriangle {
                vertexIndices   = ((ulong)gt3.vertex2 << 32) | ((ulong)gt3.vertex1 << 16) | ((ulong)gt3.vertex0 << 00),
                normalIndices   = ((ulong)gt3.normal2 << 32) | ((ulong)gt3.normal1 << 16) | ((ulong)gt3.normal0 << 00),
                colourIndices   = ((ulong)ci0 << 32) | ((ulong)ci0 << 16) | ((ulong)ci0 << 00),
                texcoordIndices = ((ulong)ti2 << 32) | ((ulong)ti1 << 16) | ((ulong)ti0 << 00),
                textureData     = gt3.tsb
            }
        );
    }

    static void ReadPrimitiveFT40(FileInputStream fis, MDLObjectContext context)
    {
        // PSX Equivalent = FT4 (0x2c 0x00 0x07 0x09)   !! USES INDEXED UVS !!
        MDLPrimitiveFT40 ft4 = fis.ReadStruct<MDLPrimitiveFT40>();
        MDLBlockUVS uvData   = context.UVBlocks[0][ft4.uvIndex];

        ushort ci0 = context.AddUniqueColour(new MDLColour { R = 255, G = 255, B = 255, A = 255 });
        ushort ti0 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u0, V = uvData.v0 });
        ushort ti1 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u1, V = uvData.v1 });
        ushort ti2 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u2, V = uvData.v2 });
        ushort ti3 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u3, V = uvData.v3 });

        context.AddTriangles(
            new MDLTriangle {
                vertexIndices   = ((ulong)ft4.vertex2 << 32) | ((ulong)ft4.vertex1 << 16) | ((ulong)ft4.vertex0 << 00),
                normalIndices   = ((ulong)ft4.normal0 << 32) | ((ulong)ft4.normal0 << 16) | ((ulong)ft4.normal0 << 00),
                colourIndices   = ((ulong)ci0 << 32) | ((ulong)ci0 << 16) | ((ulong)ci0 << 00),
                texcoordIndices = ((ulong)ti2 << 32) | ((ulong)ti1 << 16) | ((ulong)ti0 << 00),
                textureData     = uvData.tsb
            },
            new MDLTriangle {
                vertexIndices   = ((ulong)ft4.vertex1 << 32) | ((ulong)ft4.vertex2 << 16) | ((ulong)ft4.vertex3 << 00),
                normalIndices   = ((ulong)ft4.normal0 << 32) | ((ulong)ft4.normal0 << 16) | ((ulong)ft4.normal0 << 00),
                colourIndices   = ((ulong)ci0 << 32) | ((ulong)ci0 << 16) | ((ulong)ci0 << 00),
                texcoordIndices = ((ulong)ti1 << 32) | ((ulong)ti2 << 16) | ((ulong)ti3 << 00),
                textureData     = uvData.tsb
            }
        );
    }

    static void ReadPrimitiveGT40(FileInputStream fis, MDLObjectContext context)
    {
        Logger.Critical("Update GT40");

        // PSX Equivalent = GT4 (0x3c 0x00 0x08 0x0c)   !! USES INDEXED UVS !!
        MDLPrimitiveGT40 gt4 = fis.ReadStruct<MDLPrimitiveGT40>();
        MDLBlockUVS uvData  = context.UVBlocks[0][gt4.uvIndex];

        ushort ci0 = context.AddUniqueColour(new MDLColour { R = 255, G = 255, B = 255, A = 255 });
        ushort ti0 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u0, V = uvData.v0 });
        ushort ti1 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u1, V = uvData.v1 });
        ushort ti2 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u2, V = uvData.v2 });
        ushort ti3 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u3, V = uvData.v3 });

        context.AddTriangles(
            new MDLTriangle
            {
                vertexIndices   = (ulong)((gt4.vertex2 << 32) | (gt4.vertex1 << 16) | (gt4.vertex0 << 00)),
                normalIndices   = (ulong)((gt4.normal2 << 32) | (gt4.normal1 << 16) | (gt4.normal0 << 00)),
                colourIndices   = (ulong)((ci0 << 32) | (ci0 << 16) | (ci0 << 00)),
                texcoordIndices = (ulong)((ti2 << 32) | (ti1 << 16) | (ti0 << 00)),
                textureData     = uvData.tsb
            },
            new MDLTriangle
            {
                vertexIndices   = (ulong)((gt4.vertex1 << 32) | (gt4.vertex2 << 16) | (gt4.vertex3 << 00)),
                normalIndices   = (ulong)((gt4.normal1 << 32) | (gt4.normal2 << 16) | (gt4.normal3 << 00)),
                colourIndices   = (ulong)((ci0 << 32) | (ci0 << 16) | (ci0 << 00)),
                texcoordIndices = (ulong)((ti1 << 32) | (ti2 << 16) | (ti3 << 00)),
                textureData     = uvData.tsb
            }
        );
    }

    static void ReadPrimitiveFT31(FileInputStream fis, MDLObjectContext context)
    {
        // PSX Equivalent = FT3 Unlit (0x25 0x01 0x06 0x07)
        MDLPrimitiveFT31 ft3 = fis.ReadStruct<MDLPrimitiveFT31>();

        ushort ci0 = context.AddUniqueColour(new MDLColour { R = ft3.red, G = ft3.green, B = ft3.blue, A = 255 });
        ushort ni0 = context.AddUniqueNormal(GenerateNormal(context, ft3.vertex0, ft3.vertex1, ft3.vertex2));
        ushort ti0 = context.AddUniqueTexcoord(new MDLTexcoord { U = ft3.u0, V = ft3.v0 });
        ushort ti1 = context.AddUniqueTexcoord(new MDLTexcoord { U = ft3.u1, V = ft3.v1 });
        ushort ti2 = context.AddUniqueTexcoord(new MDLTexcoord { U = ft3.u2, V = ft3.v2 });

        context.AddTriangles(
            new MDLTriangle
            {
                vertexIndices   = ((ulong)ft3.vertex2 << 32) | ((ulong)ft3.vertex1 << 16) | ((ulong)ft3.vertex0 << 00),
                normalIndices   = ((ulong)ni0 << 32) | ((ulong)ni0 << 16) | ((ulong)ni0 << 00),
                colourIndices   = ((ulong)ci0 << 32) | ((ulong)ci0 << 16) | ((ulong)ci0 << 00),
                texcoordIndices = ((ulong)ti2 << 32) | ((ulong)ti1 << 16) | ((ulong)ti0 << 00),
                textureData     = ft3.tsb
            }
        );
    }

    static void ReadPrimitiveFT41(FileInputStream fis, MDLObjectContext context)
    {
        Logger.Critical("Update FT41");

        // PSX Equivalent = FT4 Unlit (0x2d 0x01 0x07 0x09)   !! USES INDEXED UVS !!
        MDLPrimitiveFT41 ft4 = fis.ReadStruct<MDLPrimitiveFT41>();
        MDLBlockUVS uvData   = context.UVBlocks[0][ft4.uvIndex];

        ushort ci0 = context.AddUniqueColour(new MDLColour { R = ft4.red, G = ft4.green, B = ft4.blue, A = 255 });
        ushort ni0 = context.AddUniqueNormal(GenerateNormal(context, ft4.vertex0, ft4.vertex1, ft4.vertex2));
        ushort ni1 = context.AddUniqueNormal(GenerateNormal(context, ft4.vertex3, ft4.vertex2, ft4.vertex1));
        ushort ti0 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u0, V = uvData.v0 });
        ushort ti1 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u1, V = uvData.v1 });
        ushort ti2 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u2, V = uvData.v2 });
        ushort ti3 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u3, V = uvData.v3 });

        context.AddTriangles(
            new MDLTriangle
            {
                vertexIndices   = ((ulong)ft4.vertex2 << 32) | ((ulong)ft4.vertex1 << 16) | ((ulong)ft4.vertex0 << 00),
                normalIndices   = ((ulong)ni0 << 32) | ((ulong)ni0 << 16) | ((ulong)ni0 << 00),
                colourIndices   = ((ulong)ci0 << 32) | ((ulong)ci0 << 16) | ((ulong)ci0 << 00),
                texcoordIndices = ((ulong)ti2 << 32) | ((ulong)ti1 << 16) | ((ulong)ti0 << 00),
                textureData     = uvData.tsb
            },
            new MDLTriangle
            {
                vertexIndices   = ((ulong)ft4.vertex1 << 32) | ((ulong)ft4.vertex2 << 16) | ((ulong)ft4.vertex3 << 00),
                normalIndices   = ((ulong)ni1 << 32) | ((ulong)ni1 << 16) | ((ulong)ni1 << 00),
                colourIndices   = ((ulong)ci0 << 32) | ((ulong)ci0 << 16) | ((ulong)ci0 << 00),
                texcoordIndices = ((ulong)ti1 << 32) | ((ulong)ti2 << 16) | ((ulong)ti3 << 00),
                textureData     = uvData.tsb
            }
        );
    }

    static void ReadPrimitiveStub(FileInputStream fis, MDLObjectContext context)
    {
        throw new NotImplementedException();
    }

    static MDLVector GenerateNormal(MDLObjectContext context, ushort I0, ushort I1, ushort I2)
    {
        // Get basic vertices...
        MDLVector V0 = context.Vertices[I0];
        MDLVector V1 = context.Vertices[I1];
        MDLVector V2 = context.Vertices[I2];

        // Get edges
        Vector3 E1 = new Vector3((V1.VX - V0.VX), -(V1.VY - V0.VY), (V1.VZ - V0.VZ));
        Vector3 E2 = new Vector3((V2.VX - V0.VX), -(V2.VY - V0.VY), (V2.VZ - V0.VZ));

        // Now create the normal...
        Vector3 N = Vector3.Cross(E1, E2).normalized;

        // Convert to fixed point and return
        return new MDLVector { VX = (short)(4096F * N.x), VY = (short)(4096F * -N.y), VZ = (short)(4096F * N.z) };
    }
    #endregion

    #region Data Definition
    /**
     * Below structs are helpers and not actually contained.
    **/
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLTexcoord
    {
        [FieldOffset(0x00)] public byte U;
        [FieldOffset(0x01)] public byte V;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLColour
    {
        [FieldOffset(0x00)] public byte R;
        [FieldOffset(0x01)] public byte G;
        [FieldOffset(0x02)] public byte B;
        [FieldOffset(0x03)] public byte A;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLTriangle
    {
        [FieldOffset(0x00)] public ulong vertexIndices;
        [FieldOffset(0x08)] public ulong normalIndices;
        [FieldOffset(0x10)] public ulong colourIndices;
        [FieldOffset(0x18)] public ulong texcoordIndices;
        [FieldOffset(0x20)] public ulong textureData;
    }

    /// <summary>
    /// Class is used to ease loading MDL data
    /// </summary>
    class MDLObjectContext
    {
        public readonly List<MDLVector>  Vertices;
        public readonly List<MDLVector>  Normals;
        public readonly List<MDLColour>   Colours;
        public readonly List<MDLTexcoord> Texcoords;
        public readonly List<MDLTriangle> Triangles;

        public readonly List<MDLBlockUVS[]> UVBlocks;

        public MDLObjectContext(MDLVector[] vertices, MDLVector[] normals, MDLBlockUVS[][] uvBlocks)
        {
            Vertices  = new List<MDLVector>(vertices);
            Normals   = new List<MDLVector>(normals);
            Colours   = new List<MDLColour>();
            Texcoords = new List<MDLTexcoord>();
            Triangles = new List<MDLTriangle>();

            UVBlocks = new List<MDLBlockUVS[]>(uvBlocks);
        }

        public ushort AddUniqueNormal(MDLVector normal)
        {
            for (int i = 0; i < Normals.Count; ++i)
                if (Normals[i].Equals(normal))
                    return (ushort)i;

            Normals.Add(normal);
            return (ushort)(Normals.Count - 1);
        }

        /// <summary>
        /// Adds a unique colour to the colour data list, if an identical one is not found.<br/>
        /// The index of the pre-existing duplicate is returned if it is matched.
        /// </summary>
        public ushort AddUniqueColour(MDLColour colour)
        {
            for (int i = 0; i < Colours.Count; ++i)
                if (Colours[i].Equals(colour))
                    return (ushort)i;

            Colours.Add(colour);
            return (ushort)(Colours.Count - 1);
        }

        /// <summary>
        /// Adds a unique texcoord to the texcoord data list, if an identical one is not found.<br/>
        /// The index of the pre-existing duplicate is returned if it is matched.
        /// </summary>
        public ushort AddUniqueTexcoord(MDLTexcoord texcoord)
        {
            for (int i = 0; i < Texcoords.Count; ++i)
                if (Texcoords[i].Equals(texcoord))
                    return (ushort)i;

            Texcoords.Add(texcoord);
            return (ushort)(Texcoords.Count - 1);
        }

        /// <summary>
        /// Adds a number of triangles to the triangle data list
        /// </summary>
        public void AddTriangles(params MDLTriangle[] triangles) =>
            Triangles.AddRange(triangles);
    }
    
    /// <summary>
    /// Class is used to ease loading TIM data
    /// </summary>
    class MDLTextureContext
    {
        public class ImageData
        {
            public TIMSurface clutSurface;
            public TIMSurface dataSurface;
            public bool hasClut;
            public byte bpp;
            public byte[] raw;
            public string virtualName;
        }

        public List<ImageData> images = new List<ImageData>();

        /// <summary>
        /// Load tim data into the texture context.
        /// </summary>
        public void LoadImage(FileInputStream fis)
        {
            ImageData imageData = new ImageData();

            int byteSize = 8;

            // We want to return after actually reading TIM data...
            fis.Jump(fis.Position);

            TIMHeader header = fis.ReadStruct<TIMHeader>();

            imageData.bpp     = (byte)(header.mode & 0x3);
            imageData.hasClut = (header.mode & 0x8) != 0;

            // Read clut...
            if ((header.mode & 0x8) != 0)
            {
                imageData.clutSurface = new TIMSurface
                {
                    byteSize = fis.ReadU32(),
                    loadX    = fis.ReadU16(),
                    loadY    = fis.ReadU16(),
                    loadW    = fis.ReadU16(),
                    loadH    = fis.ReadU16()
                };

                imageData.clutSurface.data = fis.ReadU8Array((int)(imageData.clutSurface.byteSize - 0xC));

                byteSize += (int)imageData.clutSurface.byteSize;
            }

            // Read data...
            imageData.dataSurface = new TIMSurface
            {
                byteSize = fis.ReadU32(),
                loadX    = fis.ReadU16(),
                loadY    = fis.ReadU16(),
                loadW    = fis.ReadU16(),
                loadH    = fis.ReadU16()
            };

            imageData.dataSurface.data = fis.ReadU8Array((int)(imageData.dataSurface.byteSize - 0xC));

            byteSize += (int)imageData.dataSurface.byteSize;

            fis.Return();

            // Now we've read the image parts individually, we will read the entire image as raw bytes.
            imageData.virtualName = $"{Path.GetDirectoryName(fis.Source)}\\{Path.GetFileNameWithoutExtension(fis.Source)}_{imageData.dataSurface.loadX:D4}_{imageData.dataSurface.loadY:D4}.tim";
            imageData.raw         = fis.ReadU8Array(byteSize);

            // Store the image data...
            images.Add(imageData);
        }
    }

    #endregion
}