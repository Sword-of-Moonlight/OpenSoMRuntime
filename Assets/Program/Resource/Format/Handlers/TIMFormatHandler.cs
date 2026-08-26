using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class TIMFormatHandler : FormatHandler<TextureResource>
{
    public override FormatCapabilities Capabilities => new()
    {
        allowExport = false,
        allowImport = true,
        deprecated = false,
        experimental = false
    };

    public override FormatMetadata Metadata => new()
    {
        name = "Sony PlayStation [T]exture [IM]age (*.TIM)",
        description = "Sony PlayStation VRAM Slice. The format is essentially one or two buffers which are loaded directly to VRAM.",
        version = "1.0",
        authors = new string[] { "Sony", "SN Systems" },
        extensions = new string[] { ".TIM" }
    };

    /// <summary>
    /// Validates the content of a stream as an TIM file.
    /// </summary>
    /// <param name="finStream">A stream containing the data to check</param>
    /// <returns>True if it is, false if it is not</returns>
    public override bool Validate(FileInputStream finStream)
    {
        // TIM has a decent enough header for validation.
        uint timTag = finStream.ReadU32();
        uint timMode = finStream.ReadU32();

        // We want to accumulate validation across multiple checks
        bool valid = true;
        valid &= ((timTag >> 00) & 0xFF) == 0x10;    // Tag should always be 0x10
        valid &= ((timTag >> 08) & 0xFF) == 0x00;    // Version should always be 0
        valid &= ((timTag >> 16) & 0xFF) == 0x0000;  // The last two bytes of the tag should always be 0, they are reserved.
        valid &= (((timMode & 0x3) <= 1) & ((timMode & 0x8) != 0)) | ((timMode & 0x3) > 1);   // BPP 4 or 8 + has clut, or BMP is 15 or 24.

        return valid;
    }

    /// <summary>
    /// Parses an TIM file
    /// </summary>
    public override bool Load(FileInputStream finStream, in TextureResource resource, ResourceParameters parameters = null)
    {
        // The stream is reused from the validation pass, so it's good practice to seek to the start
        finStream.SeekBegin(0);

        //
        // Reading
        //

        // Header
        uint timTag = finStream.ReadU32();
        uint timMode = finStream.ReadU32();

        // Optional CLUT...
        Color32[] timClut = null;
        if ((timMode & 0x3) <= 1 || ((timMode & 0x8) != 0))
        {
            uint timClutBSize = finStream.ReadU32();
            uint timClutLoadX = finStream.ReadU16();
            uint timClutLoadY = finStream.ReadU16();
            uint timClutLoadW = finStream.ReadU16();
            uint timClutLoadH = finStream.ReadU16();

            timClut = new Color32[(int)(timClutLoadW * timClutLoadH)];

            for (int i = 0; i < timClutLoadW * timClutLoadH; ++i)
                timClut[i] = UnpackPSXColour(finStream.ReadU16());
        }

        // Surface
        uint timSurfBSize = finStream.ReadU32();
        uint timSurfLoadX = finStream.ReadU16();
        uint timSurfLoadY = finStream.ReadU16();
        uint timSurfLoadW = finStream.ReadU16();
        uint timSurfLoadH = finStream.ReadU16();

        ushort[] timSurf = finStream.ReadU16Array((int)(timSurfLoadW * timSurfLoadH));

        //
        // Converting
        //
        NativeArray<Color32> imageBuffer;
        int imageWidth = 0, imageHeight = 0;
        int srcRowOffset, dstRowOffset;

        switch (timMode & 0x3)
        {
            // Indexed (4 BPP)
            case 0:
                // Create buffer for pixel data...
                imageWidth  = (int)timSurfLoadW << 2;
                imageHeight = (int)timSurfLoadH;
                imageBuffer = new NativeArray<Color32>(imageWidth * imageHeight, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

                for (int y = 0; y < timSurfLoadH; ++y)
                {
                    srcRowOffset = (int)((timSurfLoadW << 0) * y);
                    dstRowOffset = (int)((timSurfLoadW << 2) * y);

                    for (int x = 0; x < timSurfLoadW; ++x)
                    {
                        ushort psxPixels = timSurf[srcRowOffset + x];

                        imageBuffer[(dstRowOffset + (x << 2)) + 0] = timClut[(psxPixels >> 00) & 0xF];
                        imageBuffer[(dstRowOffset + (x << 2)) + 1] = timClut[(psxPixels >> 04) & 0xF];
                        imageBuffer[(dstRowOffset + (x << 2)) + 2] = timClut[(psxPixels >> 08) & 0xF];
                        imageBuffer[(dstRowOffset + (x << 2)) + 3] = timClut[(psxPixels >> 12) & 0xF];
                    }
                }
                break;

            // Indexed (8 BPP)
            case 1:
                // Create buffer for pixel data...
                imageWidth  = (int)timSurfLoadW << 1;
                imageHeight = (int)timSurfLoadH;
                imageBuffer = new NativeArray<Color32>(imageWidth * imageHeight, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

                for (int y = 0; y < timSurfLoadH; ++y)
                {
                    srcRowOffset = (int)((timSurfLoadW << 0) * y);
                    dstRowOffset = (int)((timSurfLoadW << 1) * y);

                    for (int x = 0; x < timSurfLoadW; ++x)
                    {
                        ushort psxPixels = timSurf[srcRowOffset + x];

                        imageBuffer[(dstRowOffset + (x << 1)) + 0] = timClut[(psxPixels >> 00) & 0xFF];
                        imageBuffer[(dstRowOffset + (x << 1)) + 1] = timClut[(psxPixels >> 08) & 0xFF];
                    }
                }
                break;

            // Direct (15 BPP)
            case 2:
                // Create buffer for pixel data...
                imageWidth  = (int)timSurfLoadW;
                imageHeight = (int)timSurfLoadH;
                imageBuffer = new NativeArray<Color32>(imageWidth * imageHeight, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

                for (int y = 0; y < timSurfLoadH; ++y)
                {
                    srcRowOffset = (int)((timSurfLoadW << 0) * y);
                    dstRowOffset = srcRowOffset;

                    for (int x = 0; x < timSurfLoadW; ++x)
                        imageBuffer[dstRowOffset + x] = UnpackPSXColour(timSurf[srcRowOffset + x]);
                }
                break;

            // Direct (24 BPP)
            case 3:
                throw new Exception("24-bit TIM files are unsupported.");

            default:
                throw new NotImplementedException("Unknown and literally impossible TIM bpp.");
        }

        //
        // Storing
        //
        resource.LoadPixels(imageBuffer.Reinterpret<byte>(UnsafeUtility.SizeOf<Color32>()), imageWidth, imageHeight);

        return true;
    }

    public Color32 UnpackPSXColour(ushort colour)
    {
        return new Color32(
            (byte)((((colour >> 00) & 0x1F) << 3) | (((colour >> 00) & 0x1F) >> 2)),
            (byte)((((colour >> 05) & 0x1F) << 3) | (((colour >> 05) & 0x1F) >> 2)),
            (byte)((((colour >> 10) & 0x1F) << 3) | (((colour >> 10) & 0x1F) >> 2)),
            255);
    }
}