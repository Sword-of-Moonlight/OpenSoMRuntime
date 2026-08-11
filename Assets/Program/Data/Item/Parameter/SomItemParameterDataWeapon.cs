using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public struct SomItemParameterDataWeapon
{
    [FieldOffset(0x00)] public float weight;
    [FieldOffset(0x04)] public byte slashDamage;
    [FieldOffset(0x05)] public byte smashDamage;
    [FieldOffset(0x06)] public byte stabDamage;
    [FieldOffset(0x07)] public byte fireDamage;
    [FieldOffset(0x08)] public byte earthDamage;
    [FieldOffset(0x09)] public byte windDamage;
    [FieldOffset(0x0A)] public byte waterDamage;
    [FieldOffset(0x0B)] public byte holyDamage;
    [FieldOffset(0x0C)] public SomItemEffectType effectType;
    [FieldOffset(0x0D)] public byte effectPotency;
    [FieldOffset(0x0E)] public byte magicID;
}