using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Auto, Pack = 1), Serializable]
public unsafe struct SoMItemProfile
{
    public string name;
    public string modelFile;
    public SoMItemType type;
    public float menuElevationOffset;
    public ushort menuTilt;
    public ushort worldTilt;
    public SoMItemProfileData data;
}