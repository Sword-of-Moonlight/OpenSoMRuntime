using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
public unsafe struct SomItemProfile
{
    public string name;
    public string modelFile;
    public SomItemType type;
    public float menuElevationOffset;
    public ushort menuTilt;
    public ushort worldTilt;
    public SomItemProfileData data;
}
