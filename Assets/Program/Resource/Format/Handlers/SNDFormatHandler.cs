using System;
using Unity.Collections;

public class SNDFormatHandler : FormatHandler<AudioResource>
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
        name        = "Sword of Moonlight [S]ou[ND] (*.SND)",
        description = "Proprietary audio file format created for Sword of Moonlight: King's Field Making Tool",
        version     = "1.0",
        authors     = new string[] { "FromSoftware" },
        extensions  = new string[] { ".SND" }
    };

    /// <summary>
    /// Validates the content of a stream as an SND file.
    /// </summary>
    /// <param name="finStream">A stream containing the data to check</param>
    /// <returns>True if it is, false if it is not</returns>
    public override bool Validate(FileInputStream finStream) => true;   // TO-DO

    /// <summary>
    /// Parses an SND file
    /// </summary>
    public override bool Load(FileInputStream finStream, in AudioResource resource, ResourceParameters parameters = null)
    {
        // The stream is reused from the validation pass, so it's good practice to seek to the start
        finStream.SeekBegin(0);

        //
        // Reading
        //

        // Read the SND Header
        ushort formatType    = finStream.ReadU16();     // usually WAVE_FORMAT_PCM
        ushort channelNum    = finStream.ReadU16();     // number of channels
        uint sampleRate      = finStream.ReadU32();     // number of samples per second
        uint byteRate        = finStream.ReadU32();     // Number of bytes per second
        ushort blockAlign    = finStream.ReadU16();     // Alignment of blocks (samples)
        ushort bitsPerSample = finStream.ReadU16();     // Number of bits per sample
        ushort cbSize        = finStream.ReadU16();     // WaveFormatEx cbSize

        uint byteLength      = finStream.ReadU32();     // Length of the sample buffer in bytes

        // Read the SND Sample Buffer
        byte[] byteBuffer = finStream.ReadU8Array((int)byteLength);

        //
        // Parsing
        //

        // First calculate how many samples we have.
        int sampleLength = (int)(byteLength / 2);

        NativeArray<float> sampleBuffer = new (sampleLength, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        // Loop by sample length
        for (int i = 0; i < sampleLength; ++i)
            sampleBuffer[i] = BitConverter.ToInt16(byteBuffer, 2 * i) / (float)short.MaxValue;

        //
        // Storing
        //

        // Now we must store our parsed data in our resource.
        resource.LoadSamples(sampleBuffer, (int)sampleRate, channelNum);

        return true;
    }
}