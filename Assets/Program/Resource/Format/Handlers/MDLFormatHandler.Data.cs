using System;
using System.Runtime.InteropServices;

public partial class MDLFormatHandler
{
    /// <summary>
    /// MDL Flags specify the type of data contained within the MDL file
    /// </summary>
    [Flags]
    public enum MDLFlags : byte
    {
        SkinnedAnimation = (1 << 0),        // Skinned Animations are contained
        UVDataBlock      = (1 << 1),        // UV Block is contained
        VertexAnimation  = (1 << 2),        // Vertex Animations are contained
        X2MDL            = (1 << 3)         // Special flag added by Michael's X2MDL to signify that the model was created using it?
    }

    /// <summary>
    /// MDL Header stores basic information about the MDL file
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLHeader
    {
        [FieldOffset(0x00)] public MDLFlags flags;
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
    /// MDL Object stores offset and count data for meshes, it is identical to a PS1 TMD Object
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLObject
    {
        [FieldOffset(0x00)] public uint vertexBase;
        [FieldOffset(0x04)] public int vertexNum;
        [FieldOffset(0x08)] public uint normalBase;
        [FieldOffset(0x0C)] public int normalNum;
        [FieldOffset(0x10)] public uint primitiveBase;
        [FieldOffset(0x14)] public int primitiveNum;
        [FieldOffset(0x18)] public int scale;
    }

    /// <summary>
    /// MDL UV stores UV information for a 4 point primitive
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLBlockUVS
    {
        [FieldOffset(0x00)] public byte u0;
        [FieldOffset(0x01)] public byte v0;
        [FieldOffset(0x02)] public ushort cba;
        [FieldOffset(0x04)] public byte u1;
        [FieldOffset(0x05)] public byte v1;
        [FieldOffset(0x06)] public ushort tsb;
        [FieldOffset(0x08)] public byte u2;
        [FieldOffset(0x09)] public byte v2;
        [FieldOffset(0x0A)] public byte u3;
        [FieldOffset(0x0B)] public byte v3;
    }

    /// <summary>
    /// MDL SVECTOR (short). It is identical to a PS1 SVECTOR.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLVector
    {
        [FieldOffset(0x00)] public short VX;
        [FieldOffset(0x02)] public short VY;
        [FieldOffset(0x04)] public short VZ;
        [FieldOffset(0x06)] public short VW;        // PSX considers this padding - but fuck sony
    }

    /// <summary>
    /// MDL primitive tag is used before a list of primitives.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLPrimitiveTag
    {
        [FieldOffset(0x00)] public short type;
        [FieldOffset(0X02)] public short count;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLPrimitiveFC30
    {
        [FieldOffset(0x00)] public byte red;        // Face Red
        [FieldOffset(0x01)] public byte green;      // Face Green
        [FieldOffset(0x02)] public byte blue;       // Face Blue
        [FieldOffset(0x03)] public byte mode;       // Mode / Flag
        [FieldOffset(0x04)] public ushort normal0;  // Normal
        [FieldOffset(0x06)] public ushort vertex0;  // Vertex Index 0
        [FieldOffset(0x08)] public ushort vertex1;  // Vertex Index 1
        [FieldOffset(0x0A)] public ushort vertex2;  // Vertex Index 2
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLPrimitiveFT30
    {
        [FieldOffset(0x00)] public byte u0;         // Texture U 0
        [FieldOffset(0x01)] public byte v0;         // Texture V 0
        [FieldOffset(0x02)] public ushort cba;      // "clut buffer address" (unused)
        [FieldOffset(0x04)] public byte u1;         // Texture U 1
        [FieldOffset(0x05)] public byte v1;         // Texture V 1
        [FieldOffset(0x06)] public ushort tsb;      // Texture Page, Format and Blend Mode
        [FieldOffset(0x08)] public byte u2;         // Texture U 2
        [FieldOffset(0x09)] public byte v2;         // Texture V 2 
        [FieldOffset(0x0A)] public ushort modeFlag; // Mode / Flag
        [FieldOffset(0x0C)] public ushort normal0;  // Normal
        [FieldOffset(0x0E)] public ushort vertex0;  // Vertex Index 0
        [FieldOffset(0x10)] public ushort vertex1;  // Vertex Index 1
        [FieldOffset(0x12)] public ushort vertex2;  // Vertex Index 2
    }
    
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLPrimitiveGT30
    {
        [FieldOffset(0x00)] public byte u0;         // Texture U 0
        [FieldOffset(0x01)] public byte v0;         // Texture V 0
        [FieldOffset(0x02)] public ushort cba;      // "clut buffer address" (unused)
        [FieldOffset(0x04)] public byte u1;         // Texture U 1
        [FieldOffset(0x05)] public byte v1;         // Texture V 1
        [FieldOffset(0x06)] public ushort tsb;      // Texture Page, Format and Blend Mode
        [FieldOffset(0x08)] public byte u2;         // Texture U 2
        [FieldOffset(0x09)] public byte v2;         // Texture V 2 
        [FieldOffset(0x0A)] public ushort modeFlag; // Mode / Flag
        [FieldOffset(0x0C)] public ushort normal0;   // Normal Index 0
        [FieldOffset(0x0E)] public ushort vertex0;   // Vertex Index 0
        [FieldOffset(0x10)] public ushort normal1;   // Normal Index 1
        [FieldOffset(0x12)] public ushort vertex1;   // Vertex Index 1
        [FieldOffset(0x14)] public ushort normal2;   // Normal Index 2
        [FieldOffset(0x16)] public ushort vertex2;   // Vertex Index 2
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLPrimitiveFT40
    {
        [FieldOffset(0x00)] public ushort uvIndex;   // Index into UV Map Blocks
        [FieldOffset(0x02)] public ushort modeFlag;  // Mode / Flag
        [FieldOffset(0x04)] public ushort normal0;   // Normal Index 0
        [FieldOffset(0x06)] public ushort vertex0;   // Vertex Index 0
        [FieldOffset(0x08)] public ushort vertex1;   // Vertex Index 1
        [FieldOffset(0x0A)] public ushort vertex2;   // Vertex Index 2
        [FieldOffset(0x0C)] public ushort vertex3;   // Vertex Index 3
        [FieldOffset(0x0E)] public ushort paddingx0E;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLPrimitiveGT40
    {
        [FieldOffset(0x00)] public ushort uvIndex;   // Index into UV Map Blocks
        [FieldOffset(0x02)] public ushort modeFlag;  // Mode / Flag
        [FieldOffset(0x04)] public ushort normal0;   // Normal Index 0
        [FieldOffset(0x06)] public ushort vertex0;   // Vertex Index 0
        [FieldOffset(0x08)] public ushort normal1;   // Normal Index 1
        [FieldOffset(0x0A)] public ushort vertex1;   // Vertex Index 1
        [FieldOffset(0x0C)] public ushort normal2;   // Normal Index 2
        [FieldOffset(0x0E)] public ushort vertex2;   // Vertex Index 2
        [FieldOffset(0x10)] public ushort normal3;   // Normal Index 3
        [FieldOffset(0x12)] public ushort vertex3;   // Vertex Index 3
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLPrimitiveFT31
    {
        [FieldOffset(0x00)] public byte u0;         // Texture U 0
        [FieldOffset(0x01)] public byte v0;         // Texture V 0
        [FieldOffset(0x02)] public ushort cba;      // "clut buffer address" (unused)
        [FieldOffset(0x04)] public byte u1;         // Texture U 1
        [FieldOffset(0x05)] public byte v1;         // Texture V 1
        [FieldOffset(0x06)] public ushort tsb;      // Texture Page, Format and Blend Mode
        [FieldOffset(0x08)] public byte u2;         // Texture U 2
        [FieldOffset(0x09)] public byte v2;         // Texture V 2 
        [FieldOffset(0x0A)] public ushort modeFlag; // Mode / Flag
        [FieldOffset(0x0C)] public byte red;        // Face Red
        [FieldOffset(0x0D)] public byte green;      // Face Green
        [FieldOffset(0x0E)] public byte blue;       // Face Blue
        [FieldOffset(0x0F)] public byte paddingx0F;
        [FieldOffset(0x10)] public ushort vertex0;  // Vertex Index 0
        [FieldOffset(0x12)] public ushort vertex1;  // Vertex Index 1
        [FieldOffset(0x14)] public ushort vertex2;  // Vertex Index 2
        [FieldOffset(0x16)] public ushort paddingx16;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct MDLPrimitiveFT41
    {
        [FieldOffset(0x00)] public ushort uvIndex;   // Index into UV Map Blocks
        [FieldOffset(0x02)] public ushort modeFlag;  // Mode / Flag
        [FieldOffset(0x04)] public byte red;         // Face Red
        [FieldOffset(0x05)] public byte green;       // Face Green
        [FieldOffset(0x06)] public byte blue;        // Face Blue
        [FieldOffset(0x07)] public byte paddingx07;

        [FieldOffset(0x08)] public ushort vertex0;   // Vertex Index 0
        [FieldOffset(0x0A)] public ushort vertex1;   // Vertex Index 1
        [FieldOffset(0x0C)] public ushort vertex2;   // Vertex Index 2
        [FieldOffset(0x0E)] public ushort vertex3;   // Vertex Index 3
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct TIMHeader
    {
        [FieldOffset(0x00)] public uint tag;    // Always 10
        [FieldOffset(0x04)] public uint mode;   // BITS 0, 1 = BPP (0 = 4BPP, 1 = 8BPP, 2 = 15BPP, 3 = 24BPP), BIT 3 = Has Clut (1 = YES, 0 = NO)
    }

    class TIMSurface
    {
        public uint byteSize;
        public ushort loadX;
        public ushort loadY;
        public ushort loadW;
        public ushort loadH;
        public byte[] data;
    }
}