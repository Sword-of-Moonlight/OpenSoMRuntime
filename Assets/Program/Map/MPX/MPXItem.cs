using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public struct MPXItem
{
    [FieldOffset(0x00)] public byte unkx00;
    [FieldOffset(0x01)] public byte unkx01;
    [FieldOffset(0x02)] public byte unkx02;
    [FieldOffset(0x03)] public byte unkx03;
    [FieldOffset(0x04)] public Vector3 position;
    [FieldOffset(0x10)] public Vector3 rotation;
    [FieldOffset(0x1C)] public short declarationID;     // PR2-PRO ID
    [FieldOffset(0x1E)] public byte appearOdds;
    [FieldOffset(0x1F)] public byte appearType;
    [FieldOffset(0x20)] public byte appearTimes;
    [FieldOffset(0x21)] public byte pickable;
    [FieldOffset(0x22)] public byte unkx22;
    [FieldOffset(0x23)] public byte unkx23;
    [FieldOffset(0x24)] public byte unkx24;
    [FieldOffset(0x25)] public byte unkx25;
    [FieldOffset(0x26)] public byte unkx26;
    [FieldOffset(0x27)] public byte unkx27;
}


[StructLayout(LayoutKind.Explicit, Pack = 1)]
public unsafe struct MPXItemDeclaration
{
    [FieldOffset(0x00)] public fixed byte unkx00[4];
    [FieldOffset(0x04)] public float positionX;
    [FieldOffset(0x08)] public float positionY;
    [FieldOffset(0x0C)] public float positionZ;
    [FieldOffset(0x10)] public float rotationX;
    [FieldOffset(0x14)] public float rotationY;
    [FieldOffset(0x18)] public float rotationZ;
    [FieldOffset(0x1C)] public short registryInd;
    [FieldOffset(0x1E)] public byte appearOdds;
    [FieldOffset(0x1F)] public byte appearType;
    [FieldOffset(0x20)] public byte appearTimes;    // 00 = infinite
    [FieldOffset(0x21)] public byte pickable;
    [FieldOffset(0x22)] public fixed byte unkx22[6];
}