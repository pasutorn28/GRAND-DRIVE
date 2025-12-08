using UnityEngine;

public enum ClubType
{
    // Woods
    Driver_1W = 0,
    Wood_2W,
    Wood_3W,
    Wood_4W,
    Wood_5W,
    Wood_7W,

    // Hybrids
    Hybrid_2H,
    Hybrid_3H,
    Hybrid_4H,
    Hybrid_5H,

    // Irons
    Iron_1I,
    Iron_2I,
    Iron_3I,
    Iron_4I,
    Iron_5I,
    Iron_6I,
    Iron_7I,
    Iron_8I,
    Iron_9I,

    // Wedges
    Wedge_PW,
    Wedge_AW,
    Wedge_SW,
    Wedge_LW,
    Wedge_XW,

    // Putter
    Putter_PT
}

[System.Serializable]
public struct ClubStats
{
    [Header("Base Stats")]
    public int power;      // Increases Base Distance (2 yards per point)
    public int control;    // Increases Bar Speed (Inverse) - Max 50
    public int accuracy;   // Increases Perfect Zone Size - Max 50
    public int spin;       // Increases Impact Vertical Range - Max 50
    public int curve;      // Increases Impact Horizontal Range - Max 50

    [Header("Settings")]
    public float loftAngle; // Launch angle (deg)
}

[System.Serializable]
public class Club
{
    public string clubName;
    public ClubType clubType;
    [Range(0, 100)] public float powerPercentage; // % of Total Power (e.g. 1W=100%, SW=32%)
    public ClubStats stats;
    
    // Runtime calculated distance
    [HideInInspector] public float maxDistance; 

    public Club(string name, ClubType type, float powerPct, ClubStats stats)
    {
        this.clubName = name;
        this.clubType = type;
        this.powerPercentage = powerPct;
        this.stats = stats;
    }
}
