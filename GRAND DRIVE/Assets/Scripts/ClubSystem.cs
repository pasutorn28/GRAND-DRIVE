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

        InitializeStarterSet();
        RecalculateDistances();
    }

    void InitializeStarterSet()
    {
        bag.Clear();

        // Starter Stats: Power 6, Ctrl 12, Acc 8, Spin 2, Curve 2
        ClubStats starterStats = new ClubStats 
        { 
            power = 6, 
            control = 12, 
            accuracy = 8, 
            spin = 2, 
            curve = 2,
            loftAngle = 10f // Default loft, override per club later
        };

        // Add Clubs
        bag.Add(new Club("1W", ClubType.Driver_1W, starterStats));
        bag.Add(new Club("2W", ClubType.Wood_2W, starterStats));
        bag.Add(new Club("3W", ClubType.Wood_3W, starterStats));
        bag.Add(new Club("5W", ClubType.Wood_5W, starterStats));
        
        bag.Add(new Club("3I", ClubType.Iron_3I, starterStats));
        bag.Add(new Club("4I", ClubType.Iron_4I, starterStats));
        bag.Add(new Club("5I", ClubType.Iron_5I, starterStats));
        bag.Add(new Club("6I", ClubType.Iron_6I, starterStats));
        bag.Add(new Club("7I", ClubType.Iron_7I, starterStats));
        bag.Add(new Club("8I", ClubType.Iron_8I, starterStats));
        bag.Add(new Club("9I", ClubType.Iron_9I, starterStats));

        bag.Add(new Club("PW", ClubType.Wedge_PW, starterStats));
        bag.Add(new Club("SW", ClubType.Wedge_SW, starterStats));
        
        bag.Add(new Club("PT", ClubType.Putter_PT, starterStats));
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
