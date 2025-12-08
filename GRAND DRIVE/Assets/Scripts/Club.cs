using UnityEngine;

public enum ClubType
{
    Driver_1W,
    Wood_2W,
    Wood_3W,
    Wood_5W,
    Iron_3I,
    Iron_4I,
    Iron_5I,
    Iron_6I,
    Iron_7I,
    Iron_8I,
    Iron_9I,
    Wedge_PW,
    Wedge_SW,
    Putter_PT
}

[System.Serializable]
public struct ClubStats
{
    [Header("Base Stats")]
    public int power;      // Power override or bonus
    public int control;    // 1-40
    public int accuracy;   // 1-40 (Perfect Zone)
    public int spin;       // 1-30
    public int curve;      // 1-30

    [Header("Settings")]
    public float loftAngle; // Launch angle (deg)
}

[System.Serializable]
public class Club
{
    public string clubName;
    public ClubType clubType;
    public ClubStats stats;
    
    // Runtime calculated distance
    [HideInInspector] public float maxDistance; 

    public Club(string name, ClubType type, ClubStats stats)
    {
        this.clubName = name;
        this.clubType = type;
        this.stats = stats;
    }
}
