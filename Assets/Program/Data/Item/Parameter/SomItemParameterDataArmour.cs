using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public struct SomItemParameterDataArmour
{
    [FieldOffset(0x00)] public float weight;
    [FieldOffset(0x04)] public byte slashDefence;
    [FieldOffset(0x05)] public byte smashDefence;
    [FieldOffset(0x06)] public byte stabDefence;
    [FieldOffset(0x07)] public byte fireDefence;
    [FieldOffset(0x08)] public byte earthDefence;
    [FieldOffset(0x09)] public byte windDefence;
    [FieldOffset(0x0A)] public byte waterDefence;
    [FieldOffset(0x0B)] public byte holyDefence;
    [FieldOffset(0x0C)] public SomItemEffectType effectType;
    [FieldOffset(0x0D)] public byte effectPotency;
}