using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ระบบการตีกอล์ฟแบบ Pangya Style
/// Swing System with Pangya-style mechanic: Power → Accuracy (Spin)
/// </summary>
public class SwingSystem : MonoBehaviour
{
    [Header("--- Swing Settings ---")]
    [Tooltip("ความเร็วของ Power Bar (ยิ่งสูงยิ่งเร็ว)")]
    public float powerBarSpeed = 1.2f;
    
    [Tooltip("ความเร็วของ Accuracy Indicator (เร็วกว่า Power)")]
    public float accuracyBarSpeed = 2.5f;
    
    [Tooltip("ขนาดของ Perfect Zone (0-1, ยิ่งน้อยยิ่งยาก)")]
    [Range(0.02f, 0.15f)]
    public float perfectZoneSize = 0.08f;

    [Header("--- Distance Settings ---")]
    [Tooltip("ระยะสูงสุดของไม้ปัจจุบัน (yards)")]
    public float maxDistance = 230f;
    
    [Tooltip("ระยะต่ำสุด (yards)")]
    public float minDistance = 0f;

    [Header("--- Current Values (Read Only) ---")]
    [SerializeField] private float currentPower = 0f;
    [SerializeField] private float currentAccuracy = 0.5f; // เริ่มกลาง
    [SerializeField] private SwingState currentState = SwingState.Ready;

    [Header("--- Events ---")]
    [Tooltip("เรียกเมื่อตีลูกสำเร็จ (power, accuracy, isPerfect)")]
    public UnityEvent<float, float, bool> OnSwingComplete;
    
    [Tooltip("เรียกเมื่อ State เปลี่ยน")]
    public UnityEvent<SwingState> OnStateChanged;
    
    [Tooltip("เรียกทุกเฟรมเพื่ออัปเดต UI")]
    public UnityEvent<float, float> OnValuesUpdated;

    // Swing States / สถานะการตี
    public enum SwingState
    {
        Ready,          // พร้อมตี
        PowerPhase,     // กำลังวัด Power (กดครั้งแรกเริ่ม, กดครั้งที่ 2 หยุด)
        AccuracyPhase,  // กำลังวัด Accuracy (ลูกศรเลื่อนไป-กลับ)
        Hitting,        // กำลังตี (animation)
        Cooldown        // รอลูกหยุด
    }

    // Private variables
    private float accuracyDirection = 1f;  // 1 = ไปขวา, -1 = ไปซ้าย
    private float perfectZoneCenter = 0.5f;  // จุดกลาง Perfect Zone (กลางเสมอ)
    private bool powerMaxReached = false;    // Power ชนขอบขวาหรือยัง

    // Properties for UI
    public float CurrentPower => currentPower;
    public float CurrentAccuracy => currentAccuracy;
    public SwingState CurrentState => currentState;
    public float PerfectZoneCenter => perfectZoneCenter;
    public float PerfectZoneSizeValue => perfectZoneSize;
    public float MaxDistance => maxDistance;
    public float CurrentDistance => currentPower * maxDistance;

    void Start()
    {
        ResetSwing();
    }

    void Update()
    {
        // จัดการ Input ตาม State
        HandleInput();
        
        // อัปเดต Bar ตาม State
        UpdateBars();
        
        // แจ้ง UI ทุกเฟรม
        OnValuesUpdated?.Invoke(currentPower, currentAccuracy);
    }

    /// <summary>
    /// จัดการ Input ของผู้เล่น
    /// Handle player input based on current state
    /// </summary>
    void HandleInput()
    {
        // กด Space หรือ Click ซ้าย
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            switch (currentState)
            {
                case SwingState.Ready:
                    StartPowerPhase();
                    break;
                    
                case SwingState.PowerPhase:
                    StopPowerStartAccuracy();
                    break;
                    
                case SwingState.AccuracyPhase:
                    ExecuteSwing();
                    break;
            }
        }

        // กด R เพื่อ Reset (สำหรับทดสอบ)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetSwing();
        }
    }

    /// <summary>
    /// อัปเดตการเคลื่อนที่ของ Bar
    /// Update bar movement based on state
    /// </summary>
    void UpdateBars()
    {
        switch (currentState)
        {
            case SwingState.PowerPhase:
                // Power Bar เคลื่อนที่ไปทางขวาอย่างเดียว (0 → 1)
                // Pangya style: ไม่ย้อนกลับ ถ้าชนขอบขวาจะหยุดที่ max
                if (!powerMaxReached)
                {
                    currentPower += powerBarSpeed * Time.deltaTime;
                    
                    if (currentPower >= 1f)
                    {
                        currentPower = 1f;
                        powerMaxReached = true;
                        // Auto stop at max power
                        StopPowerStartAccuracy();
                    }
                }
                break;

            case SwingState.AccuracyPhase:
                // Accuracy Indicator เคลื่อนที่ไป-กลับ (Pangya style)
                currentAccuracy += accuracyDirection * accuracyBarSpeed * Time.deltaTime;
                
                if (currentAccuracy >= 1f)
                {
                    currentAccuracy = 1f;
                    accuracyDirection = -1f;
                }
                else if (currentAccuracy <= 0f)
                {
                    currentAccuracy = 0f;
                    accuracyDirection = 1f;
                }
                break;
        }
    }

    /// <summary>
    /// เริ่ม Phase วัดพลัง
    /// Start Power measurement phase
    /// </summary>
    void StartPowerPhase()
    {
        currentState = SwingState.PowerPhase;
        currentPower = 0f;
        powerMaxReached = false;
        
        OnStateChanged?.Invoke(currentState);
        Debug.Log("⚡ Power Phase Started! / เริ่มวัดพลัง!");
    }

    /// <summary>
    /// หยุด Power และเริ่ม Accuracy
    /// Stop Power and start Accuracy phase
    /// </summary>
    void StopPowerStartAccuracy()
    {
        currentState = SwingState.AccuracyPhase;
        currentAccuracy = 0f; // เริ่มจากซ้ายสุด
        accuracyDirection = 1f; // เคลื่อนที่ไปขวา
        
        OnStateChanged?.Invoke(currentState);
        Debug.Log($"🎯 Accuracy Phase! Power = {currentPower:P0} ({CurrentDistance:F0}y)");
    }

    /// <summary>
    /// ตีลูก!
    /// Execute the swing!
    /// </summary>
    void ExecuteSwing()
    {
        currentState = SwingState.Hitting;
        
        // คำนวณว่าตี Perfect หรือไม่ (ต้องกดตอน Indicator อยู่ใน Perfect Zone)
        float distanceFromPerfect = Mathf.Abs(currentAccuracy - perfectZoneCenter);
        bool isPerfect = distanceFromPerfect <= (perfectZoneSize / 2f);
        
        // คำนวณ Accuracy Multiplier
        // Perfect = 1.0, ยิ่งห่างยิ่งแย่
        float accuracyMultiplier;
        if (isPerfect)
        {
            accuracyMultiplier = 1f;
        }
        else
        {
            // คำนวณระยะห่างจาก Perfect Zone
            float distanceFromZone = distanceFromPerfect - (perfectZoneSize / 2f);
            accuracyMultiplier = 1f - (distanceFromZone * 2f);
            accuracyMultiplier = Mathf.Clamp(accuracyMultiplier, 0.3f, 0.95f);
        }
        
        // Log ผลลัพธ์
        if (isPerfect)
        {
            Debug.Log($"✨ SCH-WING! PERFECT IMPACT! ✨");
            Debug.Log($"Power: {currentPower:P0} ({CurrentDistance:F0}y) | Accuracy: 100%");
        }
        else
        {
            string result = accuracyMultiplier >= 0.8f ? "Good!" : 
                           accuracyMultiplier >= 0.5f ? "OK" : "Miss...";
            Debug.Log($"🏌️ {result}");
            Debug.Log($"Power: {currentPower:P0} ({CurrentDistance:F0}y) | Accuracy: {accuracyMultiplier:P0}");
        }
        
        // แจ้ง Event
        OnStateChanged?.Invoke(currentState);
        OnSwingComplete?.Invoke(currentPower, accuracyMultiplier, isPerfect);
    }

    /// <summary>
    /// รีเซ็ตระบบ Swing
    /// Reset swing system to ready state
    /// </summary>
    public void ResetSwing()
    {
        currentState = SwingState.Ready;
        currentPower = 0f;
        currentAccuracy = 0.5f;
        accuracyDirection = 1f;
        powerMaxReached = false;
        
        OnStateChanged?.Invoke(currentState);
        Debug.Log("🔄 Swing Reset / รีเซ็ตการตี");
    }

    /// <summary>
    /// เรียกจากภายนอกเมื่อลูกหยุด
    /// Called externally when ball stops
    /// </summary>
    public void OnBallStopped()
    {
        if (currentState == SwingState.Cooldown || currentState == SwingState.Hitting)
        {
            ResetSwing();
        }
    }

    /// <summary>
    /// เปลี่ยนเป็น Cooldown state (รอลูกหยุด)
    /// Set to cooldown state (waiting for ball to stop)
    /// </summary>
    public void SetCooldown()
    {
        currentState = SwingState.Cooldown;
        OnStateChanged?.Invoke(currentState);
    }
    
    /// <summary>
    /// เปลี่ยนระยะสูงสุด (เมื่อเปลี่ยนไม้)
    /// Set max distance (when changing clubs)
    /// </summary>
    public void SetMaxDistance(float distance)
    {
        maxDistance = distance;
    }
}
