using UnityEngine;

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class MDLFormatHandler : FormatHandler<ModelResource>
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

        // Parse each MDL Object
        MDLObjectContext[] mdlObjectContexts = new MDLObjectContext[mdlObjects.Length];

        for (int i = 0; i < mdlObjects.Length; ++i)
        {
            // Get Object
            MDLObject mdlObject = mdlObjects[i];

            // Read Object Vertices
            fis.Jump(mdlObject.vertexBase);
            MDLSVector[] mdlObjectVertices = fis.ReadStructArray<MDLSVector>(mdlObject.vertexNum);
            fis.Return();

            // Read Object Normals
            fis.Jump(mdlObject.normalBase);
            MDLSVector[] mdlObjectNormals  = fis.ReadStructArray<MDLSVector>(mdlObject.normalNum);
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
                MDLBlockPrimitiveTag primitiveBlockTag = fis.ReadStruct<MDLBlockPrimitiveTag>();

                for (int j = 0; j < primitiveBlockTag.count; ++j)
                {
                    if (primitiveBlockTag.type > PrimitiveReadFunc.Length)
                        throw new Exception($"Unknown MDL Primitive Type: {primitiveBlockTag.type:X4}");

                    // Read primitive data for type
                    try
                    {
                        PrimitiveReadFunc[primitiveBlockTag.type](fis, mdlObjectContext);
                    } catch
                    {
                        Logger.Critical($"Unsupported Primitive Type '0x{primitiveBlockTag.type:X4}' at offset = 0x{fis.Position:X8}");
                        return false;
                    }
                }

                // Decrement the total primitive count with each primitive from a sub block we read
                totalPrimitiveCount -= primitiveBlockTag.count;

            } while (totalPrimitiveCount > 0);

            fis.Return();

            // Store read context
            mdlObjectContexts[i] = mdlObjectContext;
        }

        //
        // Pass #1 - Build Mesh Data
        //
        List<ModelMaterialDefinition> unityMaterialData = new List<ModelMaterialDefinition>();
        List<ModelMeshDefinition> unityMeshData         = new List<ModelMeshDefinition>();
        List<ModelStaticVertex> unityVertexData         = new List<ModelStaticVertex>();
        List<ushort> unityIndexData                     = new List<ushort>();

        // Add default material...
        unityMaterialData.Add(new ModelMaterialDefinition { textureFileName = string.Empty, blendMode = ModelMaterialBlendMode.Default, colourAlbedo = Color.white, colourEmissive = Color.black });

        for (int i = 0; i < mdlObjectContexts.Length; ++i)
        {
            // Get context
            MDLObjectContext objectContext = mdlObjectContexts[i];

            int startIndex = unityIndexData.Count;

            // We must go through each triangle...
            for (int j = 0; j < objectContext.Triangles.Count; ++j)
            {
                // Get Triangle
                MDLTriangle mdlTriangle = objectContext.Triangles[j];

                // Prepare...
                MDLSVector vPosition, vNormal;
                MDLColour vColour;

                // Vertex 1
                vPosition = objectContext.Vertices[mdlTriangle.VI0];
                vNormal   = objectContext.Normals[mdlTriangle.NI0];
                vColour   = objectContext.Colours[mdlTriangle.CI0];

                unityVertexData.Add(
                    new ModelStaticVertex
                    {
                        position = new Vector3(vPosition.VX / 1024F, -vPosition.VY / 1024F, vPosition.VZ / 1024F),
                        normal   = ModelResource.PackNormal1010102(new Vector3(vNormal.VX / 4096F, -vNormal.VY / 4096F, vNormal.VZ / 4096F).normalized),
                        colour   = new Color32(vColour.R, vColour.G, vColour.B, vColour.A),
                        texcoord = new Vector2(0F, 0F)
                    });

                unityIndexData.Add((ushort)(unityVertexData.Count - 1));

                // Vertex 2
                vPosition = objectContext.Vertices[mdlTriangle.VI1];
                vNormal   = objectContext.Normals[mdlTriangle.NI1];
                vColour   = objectContext.Colours[mdlTriangle.CI1];

                unityVertexData.Add(
                    new ModelStaticVertex
                    {
                        position = new Vector3(vPosition.VX / 1024F, -vPosition.VY / 1024F, vPosition.VZ / 1024F),
                        normal   = ModelResource.PackNormal1010102(new Vector3(vNormal.VX / 4096F, -vNormal.VY / 4096F, vNormal.VZ / 4096F).normalized),
                        colour   = new Color32(vColour.R, vColour.G, vColour.B, vColour.A),
                        texcoord = new Vector2(0F, 0F)
                    });

                unityIndexData.Add((ushort)(unityVertexData.Count - 1));

                // Vertex 3
                vPosition = objectContext.Vertices[mdlTriangle.VI2];
                vNormal   = objectContext.Normals[mdlTriangle.NI2];
                vColour   = objectContext.Colours[mdlTriangle.CI2];

                unityVertexData.Add(
                    new ModelStaticVertex
                    {
                        position = new Vector3(vPosition.VX / 1024F, -vPosition.VY / 1024F, vPosition.VZ / 1024F),
                        normal   = ModelResource.PackNormal1010102(new Vector3(vNormal.VX / 4096F, -vNormal.VY / 4096F, vNormal.VZ / 4096F).normalized),
                        colour   = new Color32(vColour.R, vColour.G, vColour.B, vColour.A),
                        texcoord = new Vector2(0F, 0F)
                    });

                unityIndexData.Add((ushort)(unityVertexData.Count - 1));
            }

            // Define the mesh
            unityMeshData.Add(new ModelMeshDefinition
            {
                materialID = 0,
                indexCount = objectContext.Triangles.Count * 3,
                indexStart = startIndex
            });
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

    #region MDL Primitive Read Helper
    Action<FileInputStream, MDLObjectContext>[] PrimitiveReadFunc = new Action<FileInputStream, MDLObjectContext>[]
    {
        // Named primitive types    - These are ones that names could be found for (from MapComp)
        ReadPrimitiveFC30,   // FC30             Flat, Colour, Tri                   Used for CPs
        ReadPrimitiveFT30,   // FT30             Flat, Texture, Tri
        ReadPrimitiveStub,   // FG30             Flat, Gradiant, Tri

        ReadPrimitiveStub,   // GC30             Smooth, Colour, Tri
        ReadPrimitiveGT30,   // GT30             Smooth, Texture, Tri   
        ReadPrimitiveStub,   // GG30             Smooth, Gradiant, Tri

        ReadPrimitiveStub,   // FC40             Flat, Colour, Quad
        ReadPrimitiveFT40,   // FT40             Flat, Texture, Quad
        ReadPrimitiveStub,   // FG40             Flat, Gradiant, Quad

        ReadPrimitiveStub,   // GC40             Smooth, Colour, Quad
        ReadPrimitiveGT40,   // GT40             Smooth, Texture, Quad
        ReadPrimitiveStub,   // GG40             Smooth, Gradiant, Quad

        ReadPrimitiveStub,   // GT31             Flat, Texture, Tri, Unlit           Not supported by som_rt? (it's null)
        ReadPrimitiveFT31,   // FT31             Smooth, Texture, Tri, Unlit
        ReadPrimitiveStub,   // GT41             Flat, Texture, Quad, Unlit          Not supported by som_rt? (it's null)
        ReadPrimitiveStub,   // FT41             Smooth, Texture, Quad, Unlit 

        // Unnamed primitive types
        ReadPrimitiveStub,   // ----UV           ? ? ? ? ? ?, External UV            Not supported by som_rt? (it's null)
        ReadPrimitiveFT41,   // FT41             Smooth, Texture, Quad, Unlit 
        ReadPrimitiveStub,   // ----UV           ? ? ? ? ? ?, External UV            Not supported by som_rt? (it's null)
        ReadPrimitiveStub    // ----UV           ? ? ? ? ? ?, External UV
    };

    static void ReadPrimitiveFC30(FileInputStream fis, MDLObjectContext context)
    {
        // PSX Equivalent = FC3 (0x20 0x00 0x03 0x04)

        byte CR0  = fis.ReadU8();
        byte CG0  = fis.ReadU8();
        byte CB0  = fis.ReadU8();
        byte mode = fis.ReadU8();
        short NI0 = fis.ReadS16();
        short VI0 = fis.ReadS16();
        short VI1 = fis.ReadS16();
        short VI2 = fis.ReadS16();

        short CI0 = context.AddUniqueColour(new MDLColour { R = CR0, G = CG0, B = CB0, A = (byte)((mode & 0x02) > 0 ? 255 : 127) });
        short TI0 = context.AddUniqueTexcoord(new MDLTexcoord { U = 0, V = 0 });

        context.AddTriangles(
            new MDLTriangle { VI0 = VI0, VI1 = VI1, VI2 = VI2, NI0 = NI0, NI1 = NI0, NI2 = NI0, CI0 = CI0, CI1 = CI0, CI2 = CI0, TI0 = TI0, TI1 = TI0, TI2 = TI0 }
            );
    }

    static void ReadPrimitiveFT30(FileInputStream fis, MDLObjectContext context)
    {
        // PSX Equivalent = GT3 (0x24 0x00 0x05 0x07)

        byte TU0 = fis.ReadU8();
        byte TV0 = fis.ReadU8();
        ushort CBA = fis.ReadU16();

        byte TU1 = fis.ReadU8();
        byte TV1 = fis.ReadU8();
        ushort TSB = fis.ReadU16();

        byte TU2 = fis.ReadU8();
        byte TV2 = fis.ReadU8();
        ushort UNK = fis.ReadU16();

        short NI0 = fis.ReadS16();
        short VI0 = fis.ReadS16();
        short VI1 = fis.ReadS16();
        short VI2 = fis.ReadS16();

        short CI0 = context.AddUniqueColour(new MDLColour { R = 255, G = 255, B = 255, A = 255 });

        short TI0 = context.AddUniqueTexcoord(new MDLTexcoord { U = TU0, V = TV0 });
        short TI1 = context.AddUniqueTexcoord(new MDLTexcoord { U = TU1, V = TV1 });
        short TI2 = context.AddUniqueTexcoord(new MDLTexcoord { U = TU2, V = TV2 });

        context.AddTriangles(
            new MDLTriangle { VI0 = VI0, VI1 = VI1, VI2 = VI2, NI0 = NI0, NI1 = NI0, NI2 = NI0, CI0 = CI0, CI1 = CI0, CI2 = CI0, TI0 = TI0, TI1 = TI1, TI2 = TI2 }
            );
    }

    static void ReadPrimitiveGT30(FileInputStream fis, MDLObjectContext context)
    {
        // PSX Equivalent = GT3 (0x34 0x00 0x06 0x09)

        byte TU0   = fis.ReadU8();
        byte TV0   = fis.ReadU8();
        ushort CBA = fis.ReadU16();

        byte TU1   = fis.ReadU8();
        byte TV1   = fis.ReadU8();
        ushort TSB = fis.ReadU16();

        byte TU2   = fis.ReadU8();
        byte TV2   = fis.ReadU8();
        ushort UNK = fis.ReadU16();

        short NI0  = fis.ReadS16();
        short VI0  = fis.ReadS16();
        short NI1  = fis.ReadS16();
        short VI1  = fis.ReadS16();
        short NI2  = fis.ReadS16();
        short VI2  = fis.ReadS16();

        short CI0 = context.AddUniqueColour(new MDLColour { R = 255, G = 255, B = 255, A = 255 });

        short TI0 = context.AddUniqueTexcoord(new MDLTexcoord { U = TU0, V = TV0 });
        short TI1 = context.AddUniqueTexcoord(new MDLTexcoord { U = TU1, V = TV1 });
        short TI2 = context.AddUniqueTexcoord(new MDLTexcoord { U = TU2, V = TV2 });

        context.AddTriangles(
            new MDLTriangle { VI0 = VI0, VI1 = VI1, VI2 = VI2, NI0 = NI0, NI1 = NI1, NI2 = NI2, CI0 = CI0, CI1 = CI0, CI2 = CI0, TI0 = TI0, TI1 = TI1, TI2 = TI2 }
            );
        
    }

    static void ReadPrimitiveFT40(FileInputStream fis, MDLObjectContext context)
    {
        // PSX Equivalent = FT4 (0x2c 0x00 0x07 0x09)   !! USES INDEXED UVS !!

        ushort uvIndex = fis.ReadU16();
        byte unkx02    = fis.ReadU8();
        byte mode      = fis.ReadU8();

        short NI0 = fis.ReadS16();
        short VI0 = fis.ReadS16();
        short VI1 = fis.ReadS16();
        short VI2 = fis.ReadS16();
        short VI3 = fis.ReadS16();
        short PAD = fis.ReadS16();

        short CI0 = context.AddUniqueColour(new MDLColour { R = 255, G = 255, B = 255, A = (byte)((mode & 0x02) > 0 ? 255 : 127) });

        // Look up texcoord data
        MDLBlockUVS uvData = context.UVBlocks[0][uvIndex];

        short TI0 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u0, V = uvData.v0 });
        short TI1 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u1, V = uvData.v1 });
        short TI2 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u2, V = uvData.v2 });
        short TI3 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u3, V = uvData.v3 });

        context.AddTriangles(
            new MDLTriangle { VI0 = VI0, VI1 = VI1, VI2 = VI2, NI0 = NI0, NI1 = NI0, NI2 = NI0, CI0 = CI0, CI1 = CI0, CI2 = CI0, TI0 = TI0, TI1 = TI1, TI2 = TI2 },
            new MDLTriangle { VI0 = VI3, VI1 = VI2, VI2 = VI1, NI0 = NI0, NI1 = NI0, NI2 = NI0, CI0 = CI0, CI1 = CI0, CI2 = CI0, TI0 = TI3, TI1 = TI2, TI2 = TI1 }
            );
    }

    static void ReadPrimitiveGT40(FileInputStream fis, MDLObjectContext context)
    {
        // PSX Equivalent = GT4 (0x3c 0x00 0x08 0x0c)   !! USES INDEXED UVS !!
        ushort uvIndex = fis.ReadU16();
        byte unkx02    = fis.ReadU8();
        byte mode      = fis.ReadU8();

        short NI0 = fis.ReadS16();
        short VI0 = fis.ReadS16();
        short NI1 = fis.ReadS16();
        short VI1 = fis.ReadS16();
        short NI2 = fis.ReadS16();
        short VI2 = fis.ReadS16();
        short NI3 = fis.ReadS16();
        short VI3 = fis.ReadS16();

        short CI0 = context.AddUniqueColour(new MDLColour { R = 255, G = 255, B = 255, A = (byte)((mode & 0x02) > 0 ? 255 : 127) });

        // Look up texcoord data
        MDLBlockUVS uvData = context.UVBlocks[0][uvIndex];

        short TI0 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u0, V = uvData.v0 });
        short TI1 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u1, V = uvData.v1 });
        short TI2 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u2, V = uvData.v2 });
        short TI3 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u3, V = uvData.v3 });

        context.AddTriangles(
            new MDLTriangle { VI0 = VI0, VI1 = VI1, VI2 = VI2, NI0 = NI0, NI1 = NI1, NI2 = NI2, CI0 = CI0, CI1 = CI0, CI2 = CI0, TI0 = TI0, TI1 = TI1, TI2 = TI2 },
            new MDLTriangle { VI0 = VI3, VI1 = VI2, VI2 = VI1, NI0 = NI3, NI1 = NI2, NI2 = NI1, CI0 = CI0, CI1 = CI0, CI2 = CI0, TI0 = TI3, TI1 = TI2, TI2 = TI1 }
            );
    }

    static void ReadPrimitiveFT31(FileInputStream fis, MDLObjectContext context)
    {
        // PSX Equivalent = FT3 Unlit (0x25 0x01 0x06 0x07)

        byte TU0 = fis.ReadU8();
        byte TV0 = fis.ReadU8();
        ushort CBA = fis.ReadU16();

        byte TU1 = fis.ReadU8();
        byte TV1 = fis.ReadU8();
        ushort TSB = fis.ReadU16();

        byte TU2 = fis.ReadU8();
        byte TV2 = fis.ReadU8();
        ushort UNK = fis.ReadU16();

        byte CR0 = fis.ReadU8();
        byte CG0 = fis.ReadU8();
        byte CB0 = fis.ReadU8();
        byte mode = fis.ReadU8();

        short VI0 = fis.ReadS16();
        short VI1 = fis.ReadS16();
        short VI2 = fis.ReadS16();
        short pad = fis.ReadS16();

        short CI0 = context.AddUniqueColour(new MDLColour { R = CR0, G = CG0, B = CB0, A = (byte)((mode & 0x02) > 0 ? 255 : 127) });
        short NI0 = context.AddUniqueNormal(new MDLSVector { VX = 0, VY = -4096, VZ = 0 });

        short TI0 = context.AddUniqueTexcoord(new MDLTexcoord { U = TU0, V = TV0 });
        short TI1 = context.AddUniqueTexcoord(new MDLTexcoord { U = TU1, V = TV1 });
        short TI2 = context.AddUniqueTexcoord(new MDLTexcoord { U = TU2, V = TV2 });

        context.AddTriangles(
            new MDLTriangle { VI0 = VI0, VI1 = VI1, VI2 = VI2, NI0 = NI0, NI1 = NI0, NI2 = NI0, CI0 = CI0, CI1 = CI0, CI2 = CI0, TI0 = TI0, TI1 = TI1, TI2 = TI2 }
            );
    }

    static void ReadPrimitiveFT41(FileInputStream fis, MDLObjectContext context)
    {
        // PSX Equivalent = FT4 Unlit (0x2d 0x01 0x07 0x09)   !! USES INDEXED UVS !!

        ushort uvIndex = fis.ReadU16();
        byte unkx02    = fis.ReadU8();
        byte unkx03    = fis.ReadU8();

        byte CR0  = fis.ReadU8();
        byte CG0  = fis.ReadU8();
        byte CB0  = fis.ReadU8();
        byte mode = fis.ReadU8();

        short VI0 = fis.ReadS16();
        short VI1 = fis.ReadS16();
        short VI2 = fis.ReadS16();
        short VI3 = fis.ReadS16();

        short CI0 = context.AddUniqueColour(new MDLColour { R = CR0, G = CG0, B = CB0, A = (byte)((mode & 0x02) > 0 ? 255 : 127) });
        short NI0 = context.AddUniqueNormal(new MDLSVector { VX = 0, VY = -4096, VZ = 0 });

        // Look up texcoord data
        MDLBlockUVS uvData = context.UVBlocks[0][uvIndex];

        short TI0 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u0, V = uvData.v0 });
        short TI1 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u1, V = uvData.v1 });
        short TI2 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u2, V = uvData.v2 });
        short TI3 = context.AddUniqueTexcoord(new MDLTexcoord { U = uvData.u3, V = uvData.v3 });

        context.AddTriangles(
            new MDLTriangle { VI0 = VI0, VI1 = VI1, VI2 = VI2, NI0 = NI0, NI1 = NI0, NI2 = NI0, CI0 = CI0, CI1 = CI0, CI2 = CI0, TI0 = TI0, TI1 = TI1, TI2 = TI2 },
            new MDLTriangle { VI0 = VI3, VI1 = VI2, VI2 = VI1, NI0 = NI0, NI1 = NI0, NI2 = NI0, CI0 = CI0, CI1 = CI0, CI2 = CI0, TI0 = TI3, TI1 = TI2, TI2 = TI1 }
            );
    }

    static void ReadPrimitiveStub(FileInputStream fis, MDLObjectContext context)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Data Definition

    [Flags]
    public enum MDLContentType : byte
    {
        SkinnedAnimation = (1 << 0),        // Skinned Animations are contained
        UVDataBlock      = (1 << 1),        // UV Block is contained
        VertexAnimation  = (1 << 2),        // Vertex Animations are contained
        X2MDL            = (1 << 3)         // Special flag added by Michael's X2MDL to signify that the model was created using it?
    }

    /// <summary>
    /// MDL Header Type
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack=1)]
    struct MDLHeader
    {
        [FieldOffset(0x00)] public MDLContentType flags;
        [FieldOffset(0x01)] public byte numSkeletalAnim;
        [FieldOffset(0x02)] public byte numVertexAnim;
        [FieldOffset(0x03)] public byte numInternalTexture;
        [FieldOffset(0x04)] public byte numTmdObject;
        [FieldOffset(0x05)] public byte numUVBlocks;
        [FieldOffset(0x06)] public ushort meshDataSize;
        [FieldOffset(0x08)] public ushort padx08;
        [FieldOffset(0x0A)] public ushort padx0A;
        [FieldOffset(0x0C)] public ushort skeletonAnimDataSize;
        [FieldOffset(0x0E)] public ushort vertexAnimDataSize;
    }

    /// <summary>
    /// MDL Object Type (Actually TMD Object)
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLObject
    {
        [FieldOffset(0x00)] public uint vertexBase;
        [FieldOffset(0x04)] public int vertexNum;       // Actually unsigned, but it doesn't matter and fuck csharp
        [FieldOffset(0x08)] public uint normalBase;
        [FieldOffset(0x0C)] public int normalNum;
        [FieldOffset(0x10)] public uint primitiveBase;
        [FieldOffset(0x14)] public int primitiveNum;
        [FieldOffset(0x18)] public int scale;
    }

    /// <summary>
    /// MDL UV Item
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLBlockUVS
    {
        [FieldOffset(0x00)] public byte u0;
        [FieldOffset(0x01)] public byte v0;
        [FieldOffset(0x02)] public ushort psxCBA;   // "Clut Buffer Address" Unsure if this is used.
        [FieldOffset(0x04)] public byte u1;
        [FieldOffset(0x05)] public byte v1;
        [FieldOffset(0x06)] public ushort psxTSB;
        [FieldOffset(0x08)] public byte u2;
        [FieldOffset(0x09)] public byte v2;
        [FieldOffset(0x0A)] public byte u3;
        [FieldOffset(0x0B)] public byte v3;
    }

    /// <summary>
    /// MDL SVECTOR (short). Actually PSX SVECTOR type.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLSVector
    {
        [FieldOffset(0x00)] public short VX;
        [FieldOffset(0x02)] public short VY;
        [FieldOffset(0x04)] public short VZ;
        [FieldOffset(0x06)] public short VW;        // PSX considers this padding - but fuck sony
    }

    /// <summary>
    /// MDL Primitive List Tag.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLBlockPrimitiveTag
    {
        [FieldOffset(0x00)] public short type;
        [FieldOffset(0X02)] public short count;
    }

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
        [FieldOffset(0x00)] public short VI0;
        [FieldOffset(0x02)] public short NI0;
        [FieldOffset(0x04)] public short CI0;
        [FieldOffset(0x06)] public short TI0;
        [FieldOffset(0x08)] public short VI1;
        [FieldOffset(0x0A)] public short NI1;
        [FieldOffset(0x0C)] public short CI1;
        [FieldOffset(0x0E)] public short TI1;
        [FieldOffset(0x10)] public short VI2;
        [FieldOffset(0x12)] public short NI2;
        [FieldOffset(0x14)] public short CI2;
        [FieldOffset(0x16)] public short TI2;
    }


    /// <summary>
    /// Class is used to ease loading MDL Data
    /// </summary>
    class MDLObjectContext
    {
        public readonly List<MDLSVector>  Vertices;
        public readonly List<MDLSVector>  Normals;
        public readonly List<MDLColour>   Colours;
        public readonly List<MDLTexcoord> Texcoords;
        public readonly List<MDLTriangle> Triangles;

        public readonly List<MDLBlockUVS[]> UVBlocks;

        public MDLObjectContext(MDLSVector[] vertices, MDLSVector[] normals, MDLBlockUVS[][] uvBlocks)
        {
            Vertices  = new List<MDLSVector>(vertices);
            Normals   = new List<MDLSVector>(normals);
            Colours   = new List<MDLColour>();
            Texcoords = new List<MDLTexcoord>();
            Triangles = new List<MDLTriangle>();

            UVBlocks = new List<MDLBlockUVS[]>(uvBlocks);
        }

        public short AddUniqueNormal(MDLSVector normal)
        {
            for (int i = 0; i < Normals.Count; ++i)
                if (Normals[i].Equals(normal))
                    return (short)i;

            Normals.Add(normal);
            return (short)(Normals.Count - 1);
        }

        /// <summary>
        /// Adds a unique colour to the colour data list, if an identical one is not found.<br/>
        /// The index of the pre-existing duplicate is returned if it is matched.
        /// </summary>
        public short AddUniqueColour(MDLColour colour)
        {
            for (int i = 0; i < Colours.Count; ++i)
                if (Colours[i].Equals(colour))
                    return (short)i;

            Colours.Add(colour);
            return (short)(Colours.Count - 1);
        }

        /// <summary>
        /// Adds a unique texcoord to the texcoord data list, if an identical one is not found.<br/>
        /// The index of the pre-existing duplicate is returned if it is matched.
        /// </summary>
        public short AddUniqueTexcoord(MDLTexcoord texcoord)
        {
            for (int i = 0; i < Texcoords.Count; ++i)
                if (Texcoords[i].Equals(texcoord))
                    return (short)i;

            Texcoords.Add(texcoord);
            return (short)(Texcoords.Count - 1);
        }

        /// <summary>
        /// Adds a number of triangles to the triangle data list
        /// </summary>
        public void AddTriangles(params MDLTriangle[] triangles) =>
            Triangles.AddRange(triangles);
    }

    #endregion
}