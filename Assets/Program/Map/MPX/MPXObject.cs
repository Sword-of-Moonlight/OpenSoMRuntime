using System;
using System.Runtime.InteropServices;
using UnityEngine;

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
    [FieldOffset(0x20)] public float scale;             // Uniform scale of the object
    [FieldOffset(0x24)] public MPXObjectFlags flags;    // Flag data for the object
}