using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ระบบการตีกอล์ฟแบบ Pangya Style - 3 Click System
/// 1. กดครั้งที่ 1: เริ่มให้ขีดเคลื่อนที่ (ซ้ายสุด → ขวาสุด → ซ้ายสุด วนloop)
/// 2. กดครั้งที่ 2: หยุดเพื่อกำหนดระยะ (ขีดจะวิ่งไปซ้ายสุดเพื่อหา Perfect Zone)
/// 3. กดครั้งที่ 3: กดในโซน Perfect เพื่อตีลูก
/// </summary>
public class SwingSystem : MonoBehaviour
{
    [Header("--- Bar Settings ---")]
    [Tooltip("ความเร็วพื้นฐานของขีด (Base Speed)")]
    public float baseBarSpeed = 2.0f; // Increased base so Control makes it slower
    
    [Tooltip("ระยะสูงสุดของไม้ปัจจุบัน (yards)")]
    public float maxDistance = 230f;

    [Header("--- Perfect Zone Settings ---")]
    [Tooltip("ขนาดของ Perfect Zone (Base)")]
    [Range(0.05f, 0.3f)]
    public float basePerfectZoneSize = 0.15f;
    
    [Tooltip("ตำแหน่งกลางของ Perfect Zone (ค่าติดลบ = ด้านซ้ายของ 0)")]
    public float perfectZoneCenter = -0.75f;
    
    [Header("--- References ---")]
    public CharacterStats characterStats;
    public ClubSystem clubSystem;
    
    // Calculated Properties
    public float CurrentBarSpeed 
    {
        get 
        {
            float spd = baseBarSpeed;
            // 1. Get Control from Club
            int clubControl = (clubSystem != null && clubSystem.GetCurrentClub() != null) 
                ? clubSystem.GetCurrentClub().stats.control : 0;
            
            // 2. Get Control from Player
            int playerControl = (characterStats != null) ? characterStats.control : 0;
            
            // 3. Calculate Reduction
            // สมมติแต่ละ Point ลด speed 0.02f
            float reduction = (clubControl + playerControl) * 0.02f;
            return Mathf.Max(0.5f, spd - reduction);
        }
    }
    
    public float CurrentPerfectZoneSizeValue
    {
         get
         {
             float size = basePerfectZoneSize;
             // 1. Club Accuracy
             int clubAcc = (clubSystem != null && clubSystem.GetCurrentClub() != null) 
                 ? clubSystem.GetCurrentClub().stats.accuracy : 0;
             // 2. Player Accuracy
             int playerAcc = (characterStats != null) ? characterStats.accuracy : 0;
             
             // 3. Bonus Size (+0.002 per point)
             float bonus = (clubAcc + playerAcc) * 0.002f;
             return Mathf.Clamp(size + bonus, 0.05f, 0.5f);
         }
    }

    [Header("--- Current Values (Read Only) ---")]
    [SerializeField] private float markerPosition = -1f;  // -1 ถึง 1 (-1 = ซ้ายสุด, 0 = กลาง, 1 = ขวาสุด)
    [SerializeField] private float selectedPower = 0f;   // 0-1 (ระยะที่เลือก)
    [SerializeField] private float accuracyResult = 0f;  // ผลความแม่นยำ
    [SerializeField] private SwingState currentState = SwingState.Ready;



    [Header("--- Audio ---")]
    [Tooltip("เสียง SCH-WING! เมื่อตี Perfect")]
    public AudioClip schwingSound;
    
    [Tooltip("เสียงตีปกติ")]
    public AudioClip hitSound;
    
    private AudioSource audioSource;

    [Header("--- Events ---")]
    public UnityEvent<float, float, bool> OnSwingComplete;
    public UnityEvent<SwingState> OnStateChanged;
    public UnityEvent<float, float, SwingState> OnValuesUpdated;

    // Swing States
    public enum SwingState
    {
        Ready,              // พร้อมตี - รอกดครั้งที่ 1
        PowerPhase,         // ขีดเคลื่อนที่ไป-กลับ - รอกดครั้งที่ 2
        AccuracyPhase,      // ขีดเคลื่อนเข้า Perfect Zone - รอกดครั้งที่ 3
        Hitting,            // กำลังตี
        Cooldown            // รอลูกหยุด
    }

    // Private variables
    private int barDirection = 1;        // 1 = ไปขวา, -1 = ไปซ้าย

    // Properties for UI
    public float MarkerPosition => markerPosition;
    public float SelectedPower => selectedPower;
    public float AccuracyResult => accuracyResult;
    public SwingState CurrentState => currentState;
    public float PerfectZoneCenter => perfectZoneCenter;

    public float MaxDistance => characterStats != null 
        ? characterStats.GetMaxDistanceWithBonus(maxDistance) 
        : maxDistance;
    public float CurrentDistance => selectedPower * MaxDistance;

    void Start()
    {
        if (characterStats == null)
            characterStats = FindFirstObjectByType<CharacterStats>();
            
        if (clubSystem == null)
            clubSystem = FindFirstObjectByType<ClubSystem>();
        
        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        
        ResetSwing();
    }

    // ...

    void UpdateMarker()
    {
        float speed = CurrentBarSpeed; // Use Dynamic Speed
        
        switch (currentState)
        {
            case SwingState.PowerPhase:
                // ขีดเคลื่อนที่ไป-กลับ ระหว่าง -1 (ซ้ายสุด) ถึง 1 (ขวาสุด)
                markerPosition += barDirection * speed * Time.deltaTime;
                
                if (markerPosition >= 1f)
                {
                    markerPosition = 1f;
                    barDirection = -1; // ย้อนกลับไปซ้าย
                }
                else if (markerPosition <= -1f)
                {
                    markerPosition = -1f;
                    barDirection = 1; // กลับไปขวา
                }
                break;

            case SwingState.AccuracyPhase:
                // ขีดเคลื่อนที่จากตำแหน่งที่เลือกไปซ้ายสุด (ผ่าน Perfect Zone)
                markerPosition -= speed * Time.deltaTime;
                
                // ถ้าไปถึงซ้ายสุดแล้ว = พลาด
                if (markerPosition <= -1f)
                {
                    Debug.Log("❌ MISS! Too slow - Reset");
                    ResetSwing();
                }
                break;
        }
    }

    // ...

    void TryExecuteSwing()
    {
        // เช็คว่าขีดอยู่ใน Perfect Zone หรือไม่
        float pzSize = CurrentPerfectZoneSizeValue; // Use Dynamic Size
        float zoneLeft = perfectZoneCenter - (pzSize / 2f);
        float zoneRight = perfectZoneCenter + (pzSize / 2f);
        
        Debug.Log($"🔍 Marker: {markerPosition:F2}, Zone: [{zoneLeft:F2} to {zoneRight:F2}]");
        
        bool isInPerfectZone = markerPosition >= zoneLeft && markerPosition <= zoneRight;
        
        if (isInPerfectZone)
        {
            // คำนวณความแม่นยำ (ยิ่งใกล้กลางยิ่งดี)
            float distanceFromCenter = Mathf.Abs(markerPosition - perfectZoneCenter);
            float normalizedAccuracy = 1f - (distanceFromCenter / (pzSize / 2f));
            accuracyResult = Mathf.Clamp01(normalizedAccuracy);
            
            // Perfect = กดตรงกลางพอดี
            bool isPerfect = distanceFromCenter < 0.03f;
            
            ExecuteSwing(isPerfect);
        }
        else if (markerPosition > zoneRight)
        {
            // ขีดยังไม่ถึง Perfect Zone (ยังอยู่ทางขวาของ zone)
            // ให้ตีได้เลย แต่ accuracy ต่ำ
            Debug.Log($"⚠️ Too early! Accuracy reduced.");
            accuracyResult = 0.3f; // ตีได้แต่ accuracy ต่ำ
            ExecuteSwing(false);
        }
        else
        {
            // markerPosition < zoneLeft = ผ่าน zone ไปแล้ว
            Debug.Log($"❌ Too late! Accuracy reduced.");
            accuracyResult = 0.2f;
            ExecuteSwing(false);
        }
    }

    void ExecuteSwing(bool isPerfect)
    {
        currentState = SwingState.Hitting;
        
        // คำนวณทิศทางเบี่ยง (ถ้าไม่ Perfect)
        // markerPosition ติดลบมาก = เอียงซ้าย = ลูกไปขวา
        // markerPosition ติดลบน้อย = เอียงขวา = ลูกไปซ้าย
        float deviation = 0f;
        if (!isPerfect)
        {
            deviation = (markerPosition - perfectZoneCenter) * 2f; // -1 ถึง 1
        }
        
        // เล่นเสียง
        if (isPerfect && schwingSound != null)
        {
            // เสียง SCH-WING! ดังสนั่น!
            audioSource.PlayOneShot(schwingSound, 1.0f);
            Debug.Log($"🎵 SCH-WING! 🎵 PERFECT SHOT! ✨");
            Debug.Log($"Distance: {CurrentDistance:F0}y | Accuracy: 100%");
        }
        else
        {
            // เสียงตีปกติ
            if (hitSound != null)
            {
                audioSource.PlayOneShot(hitSound, 0.8f);
            }
            string direction = deviation > 0 ? "LEFT" : "RIGHT";
            Debug.Log($"🏌️ Shot executed! Deviation: {direction}");
            Debug.Log($"Distance: {CurrentDistance:F0}y | Accuracy: {accuracyResult:P0}");
        }
        
        OnStateChanged?.Invoke(currentState);
        OnSwingComplete?.Invoke(selectedPower, accuracyResult, isPerfect);
    }

    public void ResetSwing()
    {
        currentState = SwingState.Ready;
        markerPosition = -1f;  // รอที่ซ้ายสุด
        selectedPower = 0f;
        accuracyResult = 0f;
        barDirection = 1;
        
        OnStateChanged?.Invoke(currentState);
        Debug.Log("🔄 Swing Reset - Press SPACE to start");
    }

    public void OnBallStopped()
    {
        if (currentState == SwingState.Cooldown || currentState == SwingState.Hitting)
        {
            ResetSwing();
        }
    }

    public void SetCooldown()
    {
        currentState = SwingState.Cooldown;
        OnStateChanged?.Invoke(currentState);
    }

    public void SetMaxDistance(float distance)
    {
        maxDistance = distance;
    }

    // Legacy properties for compatibility
    public float CurrentPower => selectedPower;
    public float CurrentAccuracy => markerPosition;
}
