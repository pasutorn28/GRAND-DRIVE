using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ระบบการตีกอล์ฟแบบ 3-Click
/// Swing System with 3-Click mechanic: Power → Accuracy → Hit
/// </summary>
public class SwingSystem : MonoBehaviour
{
    [Header("--- Swing Settings ---")]
    [Tooltip("ความเร็วของ Power Bar (ยิ่งสูงยิ่งเร็ว)")]
    public float powerBarSpeed = 1.5f;
    
    [Tooltip("ความเร็วของ Accuracy Bar")]
    public float accuracyBarSpeed = 2.0f;
    
    [Tooltip("ขนาดของ Perfect Zone (0-1, ยิ่งน้อยยิ่งยาก)")]
    [Range(0.05f, 0.3f)]
    public float perfectZoneSize = 0.15f;

    [Header("--- Current Values (Read Only) ---")]
    [SerializeField] private float currentPower = 0f;
    [SerializeField] private float currentAccuracy = 0f;
    [SerializeField] private SwingState currentState = SwingState.Ready;

    [Header("--- Events ---")]
    [Tooltip("เรียกเมื่อตีลูกสำเร็จ (power, accuracy, isPerfect)")]
    public UnityEvent<float, float, bool> OnSwingComplete;
    
    [Tooltip("เรียกเมื่อ State เปลี่ยน")]
    public UnityEvent<SwingState> OnStateChanged;

    // Swing States / สถานะการตี
    public enum SwingState
    {
        Ready,          // พร้อมตี
        PowerPhase,     // กำลังวัด Power
        AccuracyPhase,  // กำลังวัด Accuracy
        Hitting,        // กำลังตี (animation)
        Cooldown        // รอลูกหยุด
    }

    // Private variables
    private float barDirection = 1f;  // 1 = ไปขวา, -1 = ไปซ้าย
    private float perfectZoneCenter = 0.5f;  // จุดกลาง Perfect Zone

    // Properties for UI
    public float CurrentPower => currentPower;
    public float CurrentAccuracy => currentAccuracy;
    public SwingState CurrentState => currentState;
    public float PerfectZoneCenter => perfectZoneCenter;
    public float PerfectZoneSize => perfectZoneSize;

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
                // Power Bar เคลื่อนที่ไป-กลับ (0 → 1 → 0)
                currentPower += barDirection * powerBarSpeed * Time.deltaTime;
                
                // สะท้อนกลับเมื่อถึงขอบ
                if (currentPower >= 1f)
                {
                    currentPower = 1f;
                    barDirection = -1f;
                }
                else if (currentPower <= 0f)
                {
                    currentPower = 0f;
                    barDirection = 1f;
                }
                break;

            case SwingState.AccuracyPhase:
                // Accuracy Bar เคลื่อนที่ไป-กลับ
                currentAccuracy += barDirection * accuracyBarSpeed * Time.deltaTime;
                
                if (currentAccuracy >= 1f)
                {
                    currentAccuracy = 1f;
                    barDirection = -1f;
                }
                else if (currentAccuracy <= 0f)
                {
                    currentAccuracy = 0f;
                    barDirection = 1f;
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
        barDirection = 1f;
        
        // สุ่มตำแหน่ง Perfect Zone ใหม่ทุกครั้ง
        perfectZoneCenter = Random.Range(0.3f, 0.7f);
        
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
        currentAccuracy = 0f;
        barDirection = 1f;
        
        OnStateChanged?.Invoke(currentState);
        Debug.Log($"🎯 Accuracy Phase! Power = {currentPower:P0}");
    }

    /// <summary>
    /// ตีลูก!
    /// Execute the swing!
    /// </summary>
    void ExecuteSwing()
    {
        currentState = SwingState.Hitting;
        
        // คำนวณว่าตี Perfect หรือไม่
        float distanceFromPerfect = Mathf.Abs(currentAccuracy - perfectZoneCenter);
        bool isPerfect = distanceFromPerfect <= (perfectZoneSize / 2f);
        
        // คำนวณ Accuracy Penalty (ยิ่งห่างจาก Perfect ยิ่งแย่)
        float accuracyMultiplier = 1f - (distanceFromPerfect * 2f);
        accuracyMultiplier = Mathf.Clamp(accuracyMultiplier, 0.3f, 1f);
        
        // Log ผลลัพธ์
        if (isPerfect)
        {
            Debug.Log($"✨ SCH-WING! PERFECT IMPACT! ✨");
            Debug.Log($"Power: {currentPower:P0} | Accuracy: {accuracyMultiplier:P0}");
        }
        else
        {
            Debug.Log($"🏌️ Swing Complete!");
            Debug.Log($"Power: {currentPower:P0} | Accuracy: {accuracyMultiplier:P0}");
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
        currentAccuracy = 0f;
        barDirection = 1f;
        
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
}
