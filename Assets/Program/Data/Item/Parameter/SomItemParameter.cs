using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
public unsafe struct SomItemParameter
{
    public short profileId;                 // Index into the PR2 file for base PRF data.
    public string name;                     // Name
    public string description;              // Description
    public uint unkx112;                    // Unknown Bytes. Always 0?.. Could be more description.
    public uint unkx116;                    // ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^
    public uint unkx11A;                    // ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^
    public uint unkx11E;                    // ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^   ^
    public byte priority;                   // 0 = Default, 1 = Crucial (does not despawn)
    public byte unkx123;                    // Unknown Bytes. Always 0?
    public uint unkx124;                    // ^   ^   ^   ^   ^   ^   ^
    public SomItemParameterData data;       // Data depending on the item type (defined in the profile)
}