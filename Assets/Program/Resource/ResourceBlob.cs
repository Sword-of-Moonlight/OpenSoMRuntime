using UnityEngine;

public class ResourceBlob
{
    /// <summary>
    /// A fake path to the origin
    /// </summary>
    public string VirtualOrigin { get; set; }

    /// <summary>
    /// A buffer containing resource data
    /// </summary>
    public byte[] Buffer { get; set; }
}
