using System;
using System.Text;
using UnityEngine;

public static class EncodingExtensions
{
    public static readonly Encoding SJIS;

    /// <summary>
    /// Static Constructor.<br/>
    /// Initializes a few common encodings
    /// </summary>
    static EncodingExtensions()
    {
        // Must register providers...
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // SHIFT-JIS (Japanese, Page = 932)
        SJIS = Encoding.GetEncoding(932);
    }
}
