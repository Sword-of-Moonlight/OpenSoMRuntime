using UnityEngine;

public partial class FileInputStream
{
    /// <summary>
    /// Reads a Color32 from the stream
    /// </summary>
    public Color32 ReadColor32_BGRX32()
    {
        uint components = ReadU32();

        return new Color32((byte)((components >> 16) & 0xFF), (byte)((components >> 08) & 0xFF), (byte)((components >> 00) & 0xFF), 255);
    }

    /// <summary>
    /// Reads a Vector3 from the stream
    /// </summary>
    public Vector3 ReadVector3()
    {
        float X = ReadF32();
        float Y = ReadF32();
        float Z = ReadF32();

        return new Vector3(X, Y, Z);
    }
}
