using System;

/// <summary>
/// File Format Metadata stores basic informative data about a FileFormat
/// </summary>
public class FormatMetadata
{
    /// <summary>Name of the format, e.g. Bitmap</summary>
    public string name         = string.Empty;

    /// <summary>Brief description of the format</summary>
    public string description  = string.Empty;

    /// <summary>Version of the format</summary>
    public string version      = string.Empty;

    /// <summary>Names/companies involved in creating the format</summary>
    public string[] authors    = Array.Empty<string>();

    /// <summary>Common extensions of the format, e.g. '.jpg', '.jpeg'. DO NOT FORGET THE DOT!</summary>
    public string[] extensions = Array.Empty<string>();
}
