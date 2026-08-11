using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SomItemUsableDataRecovery
{
    [FieldOffset(0x00)] public short hp;
    [FieldOffset(0x02)] public short mp;
    [FieldOffset(0x04)] public byte curePoison;
    [FieldOffset(0x05)] public byte cureParalyse;
    [FieldOffset(0x06)] public byte cureDark;
    [FieldOffset(0x07)] public byte cureCurse;
    [FieldOffset(0x08)] public byte cureSlow;
}