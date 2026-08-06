using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public unsafe struct SoMItemProfileData
{
    [FieldOffset(0x00)] public SoMItemProfileUsableData usable;
    [FieldOffset(0x00)] public SoMItemProfileWeaponData weapon;
    [FieldOffset(0x00)] public SoMItemProfileArmourData armour;
    [FieldOffset(0x00)] public fixed byte raw[16];
}

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public struct SoMItemProfileUsableData
{
    [FieldOffset(0x00)] public uint unkx00;     // Unknown.
    [FieldOffset(0x04)] public byte slotKeyID;  // Special ID which must match with an objects to be able to slot the item into the object.
    [FieldOffset(0x05)] public byte unkx05;     // Unknown.
    [FieldOffset(0x06)] public ushort unkx06;   // Unknown.
    [FieldOffset(0x08)] public uint unkx08;     // Unknown.
    [FieldOffset(0x0C)] public uint unkx0C;     // Unknown.
}

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public struct SoMItemProfileWeaponData
{
    [FieldOffset(0x00)] public byte swingAnimationID;       // The animation ID (in arm.mdl) to use for the swing
    [FieldOffset(0x01)] public byte soundDelay;             // Delay (in frames) after swing when sound will play    
    [FieldOffset(0x02)] public short soundID;               // The sound effect to play for the swing.
    [FieldOffset(0x04)] public byte hitWindowStart;         // The starting frame after swing where hits will register on an entity
    [FieldOffset(0x05)] public byte hitWindowEnd;           // The ending frame where hits will no longer register on an entity
    [FieldOffset(0x06)] public ushort hitArc;               // The arc of the attack in degrees, either side of the player.
    [FieldOffset(0x08)] public float hitRange;              // The range of the attack. 1 = 1 metre
    [FieldOffset(0x0C)] public ushort unkx0C;               // Could be padding? Not sure. Sound pitch maybe?.. (needs testing)
    [FieldOffset(0x0E)] public byte magicWindowStart;       // The starting frame where sword magic can be cast
    [FieldOffset(0x0F)] public byte magicWindowEnd;   // The ending frame where sword magic can no longer be cast
}

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public struct SoMItemProfileArmourData
{
    [FieldOffset(0x00)] public SoMItemEquipType equipType;  // The type of this piece of armour. 0 = Helm, 1 = Body, 2 = Arms, 3 = Boots, 4 = Suit, 5 = Shield, 6 = Accessory
    [FieldOffset(0x01)] public byte unkx01;                 // Unknown. These first few values are mixes of either 00 or FF (probably signed), They probably have _some_ purpose...
    [FieldOffset(0x02)] public ushort unkx02;               // Unknown. ^  ^  ^  ^  ^  ^  ^  ^  ^  ^
    [FieldOffset(0x04)] public uint unkx04;                 // Unknown. ^  ^  ^  ^  ^  ^  ^  ^  ^  ^
    [FieldOffset(0x08)] public uint unkx08;                 // Unknown.
    [FieldOffset(0x0C)] public uint unkx0C;                 // Unknown.
}