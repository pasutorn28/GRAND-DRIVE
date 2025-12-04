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
    [Tooltip("ความเร็วของขีดเคลื่อนที่")]
    public float barSpeed = 1.5f;
    
    [Tooltip("ระยะสูงสุดของไม้ปัจจุบัน (yards)")]
    public float maxDistance = 230f;

    [Header("--- Perfect Zone Settings ---")]
    [Tooltip("ขนาดของ Perfect Zone (0-1)")]
    [Range(0.05f, 0.3f)]
    public float perfectZoneSize = 0.2f;
    
    [Tooltip("ตำแหน่งกลางของ Perfect Zone (ค่าติดลบ = ด้านซ้ายของ 0)")]
    public float perfectZoneCenter = -0.75f;

    [Header("--- Current Values (Read Only) ---")]
    [SerializeField] private float markerPosition = -1f;  // -1 ถึง 1 (-1 = ซ้ายสุด, 0 = กลาง, 1 = ขวาสุด)
    [SerializeField] private float selectedPower = 0f;   // 0-1 (ระยะที่เลือก)
    [SerializeField] private float accuracyResult = 0f;  // ผลความแม่นยำ
    [SerializeField] private SwingState currentState = SwingState.Ready;

    [Header("--- Character Stats ---")]
    public CharacterStats characterStats;

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
    private bool powerSelected = false;  // เลือกระยะแล้วหรือยัง

    // Properties for UI
    public float MarkerPosition => markerPosition;
    public float SelectedPower => selectedPower;
    public float AccuracyResult => accuracyResult;
    public SwingState CurrentState => currentState;
    public float PerfectZoneCenter => perfectZoneCenter;
    public float PerfectZoneSizeValue => perfectZoneSize;
    public float MaxDistance => characterStats != null 
        ? characterStats.GetMaxDistanceWithBonus(maxDistance) 
        : maxDistance;
    public float CurrentDistance => selectedPower * MaxDistance;

    void Start()
    {
        if (characterStats == null)
            characterStats = FindFirstObjectByType<CharacterStats>();
        
        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        
        ResetSwing();
    }

    void Update()
    {
        HandleInput();
        UpdateMarker();
        OnValuesUpdated?.Invoke(markerPosition, selectedPower, currentState);
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            switch (currentState)
            {
                case SwingState.Ready:
                    // กดครั้งที่ 1: เริ่มเคลื่อนที่
                    StartPowerPhase();
                    break;
                    
                case SwingState.PowerPhase:
                    // กดครั้งที่ 2: เลือกระยะ
                    SelectPower();
                    break;
                    
                case SwingState.AccuracyPhase:
                    // กดครั้งที่ 3: ยืนยันการตี
                    TryExecuteSwing();
                    break;
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetSwing();
        }
    }

    void UpdateMarker()
    {
        switch (currentState)
        {
            case SwingState.PowerPhase:
                // ขีดเคลื่อนที่ไป-กลับ ระหว่าง -1 (ซ้ายสุด) ถึง 1 (ขวาสุด)
                markerPosition += barDirection * barSpeed * Time.deltaTime;
                
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
                markerPosition -= barSpeed * Time.deltaTime;
                
                // ถ้าไปถึงซ้ายสุดแล้ว = พลาด
                if (markerPosition <= -1f)
                {
                    Debug.Log("❌ MISS! Too slow - Reset");
                    ResetSwing();
                }
                break;
        }
    }

    void StartPowerPhase()
    {
        currentState = SwingState.PowerPhase;
        markerPosition = -1f;  // เริ่มจากซ้ายสุด
        barDirection = 1;      // เคลื่อนไปขวา
        powerSelected = false;
        selectedPower = 0f;
        
        OnStateChanged?.Invoke(currentState);
        Debug.Log("⚡ Power Phase - Press SPACE to set distance!");
    }

    void SelectPower()
    {
        // บันทึกระยะที่เลือก 
        // markerPosition -1 ถึง 1 → แปลงเป็น 0-1
        // -1 = 0%, 0 = 50%, 1 = 100%
        selectedPower = (markerPosition + 1f) / 2f;
        powerSelected = true;
        
        Debug.Log($"📏 Distance Selected: {selectedPower:P0} ({CurrentDistance:F0}y)");
        
        // เข้าสู่ Accuracy Phase - ขีดจะวิ่งต่อไปทางซ้าย
        StartAccuracyPhase();
    }

    void StartAccuracyPhase()
    {
        currentState = SwingState.AccuracyPhase;
        // ไม่ต้อง reset markerPosition - ให้วิ่งต่อจากตำแหน่งที่เลือก
        
        OnStateChanged?.Invoke(currentState);
        Debug.Log("🎯 Accuracy Phase - Press SPACE in the Perfect Zone!");
    }

    void TryExecuteSwing()
    {
        // เช็คว่าขีดอยู่ใน Perfect Zone หรือไม่
        float zoneLeft = perfectZoneCenter - (perfectZoneSize / 2f);
        float zoneRight = perfectZoneCenter + (perfectZoneSize / 2f);
        
        Debug.Log($"🔍 Marker: {markerPosition:F2}, Zone: [{zoneLeft:F2} to {zoneRight:F2}]");
        
        bool isInPerfectZone = markerPosition >= zoneLeft && markerPosition <= zoneRight;
        
        if (isInPerfectZone)
        {
            // คำนวณความแม่นยำ (ยิ่งใกล้กลางยิ่งดี)
            float distanceFromCenter = Mathf.Abs(markerPosition - perfectZoneCenter);
            float normalizedAccuracy = 1f - (distanceFromCenter / (perfectZoneSize / 2f));
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
        powerSelected = false;
        
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
