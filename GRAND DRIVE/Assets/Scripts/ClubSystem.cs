using UnityEngine;
using System.Collections.Generic;

public class ClubSystem : MonoBehaviour
{
    [Header("--- Player Stats Reference ---")]
    public CharacterStats playerStats;

    [Header("--- Club Set ---")]
    public List<Club> bag = new List<Club>();
    public int currentClubIndex = 0;

    [Header("--- Debug ---")]
    public float currentDriverDistance = 0f;

    // Events
    public System.Action<Club> OnClubChanged;

    void Start()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<CharacterStats>();

        InitializeAllClubs();
        RecalculateDistances();
    }

    void InitializeAllClubs()
    {
        bag.Clear();

        // STARTER SET SCALING LOGIC
        // Base (1W):  P:6, C:12, A:8,  S:2, Crv:2
        // Max (XW/PT): x2 -> C:24, A:16, S:4, Crv:4 (Power decreases naturally)
        
        // --- WOODS ---
        AddClub("1W", ClubType.Driver_1W, 100f, 6, 12, 8,  2, 2, 10f);
        AddClub("2W", ClubType.Wood_2W, 96f,    6, 13, 8,  2, 2, 12f);
        AddClub("3W", ClubType.Wood_3W, 92f,    6, 14, 9,  2, 2, 13f);
        AddClub("4W", ClubType.Wood_4W, 88f,    6, 14, 9,  2, 2, 15f);
        AddClub("5W", ClubType.Wood_5W, 84f,    6, 15, 10, 2, 2, 17f);
        AddClub("7W", ClubType.Wood_7W, 78f,    6, 15, 10, 3, 3, 20f);

        // --- HYBRIDS (Scale up slightly) ---
        AddClub("2H", ClubType.Hybrid_2H, 86f,  5, 16, 10, 2, 2, 16f);
        AddClub("3H", ClubType.Hybrid_3H, 78f,  5, 16, 11, 2, 3, 18f);
        AddClub("4H", ClubType.Hybrid_4H, 74f,  5, 17, 11, 3, 3, 20f);
        AddClub("5H", ClubType.Hybrid_5H, 70f,  5, 17, 12, 3, 3, 22f);

        // --- IRONS (Mid scaling) ---
        // 1I-4I ~ 1.25x Base
        AddClub("1I", ClubType.Iron_1I, 90f,    6, 14, 9,  2, 4, 12f); // Special Curve
        AddClub("2I", ClubType.Iron_2I, 82f,    6, 15, 10, 2, 4, 14f);
        AddClub("3I", ClubType.Iron_3I, 76f,    5, 16, 11, 3, 3, 16f);
        AddClub("4I", ClubType.Iron_4I, 74f,    5, 17, 11, 3, 3, 19f);
        
        // 5I-9I ~ 1.5x Base -> C:18, A:12, S:3
        AddClub("5I", ClubType.Iron_5I, 70f,    5, 18, 12, 3, 3, 22f);
        AddClub("6I", ClubType.Iron_6I, 66f,    5, 19, 13, 3, 3, 25f);
        AddClub("7I", ClubType.Iron_7I, 62f,    5, 20, 13, 3, 3, 28f);
        AddClub("8I", ClubType.Iron_8I, 58f,    5, 21, 14, 3, 3, 32f); 
        AddClub("9I", ClubType.Iron_9I, 54f,    5, 22, 15, 4, 4, 36f); // Approaching x2

        // --- WEDGES (Max x2) ---
        // Target Max: C:24, A:16, S:4, Crv:4
        AddClub("PW", ClubType.Wedge_PW, 48f,   4, 23, 15, 4, 4, 44f);
        AddClub("AW", ClubType.Wedge_AW, 40f,   4, 23, 16, 4, 4, 50f);
        AddClub("SW", ClubType.Wedge_SW, 32f,   3, 24, 16, 4, 4, 54f); // Max Stats
        AddClub("LW", ClubType.Wedge_LW, 24f,   3, 24, 16, 4, 4, 60f); // Max Stats
        AddClub("XW", ClubType.Wedge_XW, 16f,   2, 24, 16, 4, 4, 64f); // Max Stats

        // --- PUTTER ---
        AddClub("PT", ClubType.Putter_PT, 10f,  2, 24, 16, 0, 0, 0f); // Max C/A for Putter
    }

    void AddClub(string name, ClubType type, float pwrPct, int p, int c, int a, int s, int crv, float loft)
    {
        ClubStats stats = new ClubStats 
        { 
            power = p, 
            control = c, 
            accuracy = a, 
            spin = s, 
            curve = crv,
            loftAngle = loft 
        };
        bag.Add(new Club(name, type, pwrPct, stats));
    }

    public void RecalculateDistances()
    {
        // Calculate Base Distance per club using (Player Power + Club Power)
        // Rule: Start 200Y, Power 1 = +2Y
        
        int playerPower = 0;
        if (playerStats != null) playerPower = playerStats.power;

        foreach (var club in bag)
        {
            // Total Power for this specific club setup
            int totalPower = playerPower + club.stats.power;

            // Base 1W Distance for this power level
            float base1W = 200f + (totalPower * 2f);

            // Update Debug info only for Driver to verify
            if (club.clubType == ClubType.Driver_1W)
            {
                currentDriverDistance = base1W;
            }

            // Calculate actual distance for this club type relative to its Base 1W potential
            club.maxDistance = CalculateClubDistance(club.clubType, base1W);
        }
    }

    float CalculateClubDistance(ClubType type, float driverDist)
    {
        // Rules:
        // 1W = Base
        // 2W = 1W - 20
        // 3W = 2W - 20 (or 1W - 40)
        // 5W = 3W - 20 (or 1W - 60) -> Special Branch
        
        // Irons:
        // User confirmed: 3I is 162y when 3W is 172y. So 3I = 3W - 10y.
        // 4I = 3I - 10...
        
        // Calculation Map relative to 1W:
        float d1W = driverDist;
        float d3W = d1W - 40f; // 1W -> 2W(-20) -> 3W(-20)

        switch (type)
        {
            case ClubType.Driver_1W: return d1W;
            case ClubType.Wood_2W:   return d1W - 20f;
            case ClubType.Wood_3W:   return d3W;
            case ClubType.Wood_5W:   return d3W - 20f; // 5W is 20y less than 3W
            
            // Irons Chain (Starts from 3W - 10)
            case ClubType.Iron_3I:   return d3W - 10f;
            case ClubType.Iron_4I:   return d3W - 20f;
            case ClubType.Iron_5I:   return d3W - 30f;
            case ClubType.Iron_6I:   return d3W - 40f;
            case ClubType.Iron_7I:   return d3W - 50f;
            case ClubType.Iron_8I:   return d3W - 60f;
            case ClubType.Iron_9I:   return d3W - 70f;
            
            // Wedges
            case ClubType.Wedge_PW:  return (d3W - 70f) - 10f; // 9I - 10
            case ClubType.Wedge_SW:  return (d3W - 70f) - 30f; // PW - 20 (Total -30 from 9I)

            case ClubType.Putter_PT: return 30f; // Mockup distance for Putter
            
            default: return 0f;
        }
    }

    public void NextClub()
    {
        currentClubIndex = (currentClubIndex + 1) % bag.Count;
        OnClubChanged?.Invoke(GetCurrentClub());
    }

    public void PrevClub()
    {
        currentClubIndex--;
        if (currentClubIndex < 0) currentClubIndex = bag.Count - 1;
        OnClubChanged?.Invoke(GetCurrentClub());
    }

    public Club GetCurrentClub()
    {
        if (bag.Count == 0) return null;
        return bag[currentClubIndex];
    }
}
