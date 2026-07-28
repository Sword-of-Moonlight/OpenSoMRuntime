using UnityEngine;

public class SessionData
{
    /// <summary>Counters store a single runtime value</summary>
    public ushort[] Counters { get; set; } = new ushort[1024];

    /// <summary>Player Current Level</summary>
    public int PlayerLevel { get; set; }

    /// <summary>Player Current Experience</summary>
    public int PlayerExperience { get; set; }

    /// <summary>Player Current Strength</summary>
    public int PlayerStrength { get; set; }

    /// <summary>Player Current Magic</summary>
    public int PlayerMagic { get; set; }

    /// <summary>Player current currency</summary>
    public int PlayerCoin { get; set; }
}
