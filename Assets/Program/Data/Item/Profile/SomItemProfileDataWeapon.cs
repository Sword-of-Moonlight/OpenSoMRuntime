using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Pack = 1), Serializable]
public struct SomItemProfileDataWeapon
{
    [FieldOffset(0x00)] public byte swingAnimationId;       // The animation ID (in arm.mdl) to use for the swing
    [FieldOffset(0x01)] public byte soundDelay;             // Delay (in frames) after swing when sound will play    
    [FieldOffset(0x02)] public short soundId;               // The sound effect to play for the swing.
    [FieldOffset(0x04)] public byte hitWindowStart;         // The starting frame after swing where hits will register on an entity
    [FieldOffset(0x05)] public byte hitWindowEnd;           // The ending frame where hits will no longer register on an entity
    [FieldOffset(0x06)] public ushort hitArc;               // The arc of the attack in degrees, either side of the player.
    [FieldOffset(0x08)] public float hitRange;              // The range of the attack. 1 = 1 metre
    [FieldOffset(0x0C)] public ushort unkx0C;               // Could be padding? Not sure. Sound pitch maybe?.. (needs testing)
    [FieldOffset(0x0E)] public byte magicWindowStart;       // The starting frame where sword magic can be cast
    [FieldOffset(0x0F)] public byte magicWindowEnd;         // The ending frame where sword magic can no longer be cast
}