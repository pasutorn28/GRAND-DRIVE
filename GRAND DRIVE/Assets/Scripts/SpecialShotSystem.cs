using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ระบบ Special Shots - Spike, Tomahawk, Cobra
/// Special Shots System - Unique trajectories that change ball physics
/// 
/// 🟢 Normal: โค้งปกติ กลิ้งต่อได้
/// 🟡 Spike: ขึ้นสูงที่สุด → ถึง apex แล้วพุ่งเฉียงลง → หยุดนิ่งทันที
/// 🔴 Tomahawk: ขึ้นสูงมาก → ดิ่งลงตรงๆ → หยุดนิ่งทันที  
/// 🔵 Cobra: ต่ำมาก → เด้งหลายครั้ง → กลิ้งต่อได้
/// </summary>
public class SpecialShotSystem : MonoBehaviour
{
    [Header("--- References ---")]
    public GolfBallController ballController;
    public SwingSystem swingSystem;

    [Header("--- Special Shot Types ---")]
    public SpecialShotType currentShot = SpecialShotType.Normal;

    [Header("--- Spike Settings (🟡 สูงสุด → เฉียงลง → หยุดนิ่ง) ---")]
    [Tooltip("มุมยิงขึ้น (สูงที่สุดในทุก shot)")]
    public float spikeLaunchAngle = 75f;
    
    [Tooltip("แรงยิงขึ้นเพิ่มเติม")]
    public float spikeLiftForce = 20f;
    
    [Tooltip("แรงพุ่งเฉียงลงเมื่อถึง apex")]
    public float spikeDiveForce = 35f;
    
    [Tooltip("มุมเฉียงลง (องศาจากแนวนอน)")]
    public float spikeDiveAngle = 45f;

    [Header("--- Tomahawk Settings (🔴 สูงมาก → ดิ่งตรง → หยุดนิ่ง) ---")]
    [Tooltip("มุมยิงขึ้น (สูงมาก แต่ต่ำกว่า Spike)")]
    public float tomahawkLaunchAngle = 65f;
    
    [Tooltip("แรงยกพิเศษ")]
    public float tomahawkLiftForce = 15f;
    
    [Tooltip("แรงกดลงตรงๆ")]
    public float tomahawkDropForce = 50f;

    [Header("--- Cobra Settings (🔵 ต่ำมาก → เด้งหลายครั้ง) ---")]
    [Tooltip("มุมยิงต่ำมาก")]
    public float cobraLaunchAngle = 12f;
    
    [Tooltip("แรงยิงไปข้างหน้า")]
    public float cobraForwardForce = 30f;
    
    [Tooltip("ความยืดหยุ่นเมื่อเด้ง (ทำให้เด้งหลายครั้ง)")]
    public float cobraBounciness = 0.6f;

    [Header("--- Gauge Settings ---")]
    [Tooltip("พลังงาน Special Shot (0-100)")]
    [Range(0f, 100f)] public float specialGauge = 100f;
    
    [Tooltip("ค่าใช้ Special Shot")]
    public float specialShotCost = 30f;
    
    [Tooltip("รีเจน Gauge ต่อวินาที")]
    public float gaugeRegenRate = 5f;

    [Header("--- Events ---")]
    public UnityEvent<SpecialShotType> OnSpecialShotSelected;
    public UnityEvent<SpecialShotType> OnSpecialShotExecuted;
    public UnityEvent<float> OnGaugeChanged;

    // Private - สำหรับ reference เท่านั้น
    private Rigidbody ballRb;

    public enum SpecialShotType
    {
        Normal,     // 🟢 ตีปกติ โค้งปกติ
        Spike,      // 🟡 ขึ้นสูงสุด → เฉียงลง → หยุดนิ่ง
        Tomahawk,   // 🔴 ขึ้นสูงมาก → ดิ่งตรง → หยุดนิ่ง
        Cobra       // 🔵 ต่ำมาก → เด้งหลายครั้ง
    }

    void Start()
    {
        if (ballController == null)
            ballController = FindFirstObjectByType<GolfBallController>();
        
        if (swingSystem == null)
            swingSystem = FindFirstObjectByType<SwingSystem>();

        // NOTE: ไม่ต้อง subscribe OnSwingComplete เพราะ GolfBallController จัดการ Special Shots เองแล้ว
        // Special Shots logic อยู่ใน GolfBallController.cs
        // Script นี้ใช้สำหรับ UI และ Gauge เท่านั้น

        // Get ball rigidbody
        if (ballController != null)
        {
            ballRb = ballController.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        // Regen gauge
        if (specialGauge < 100f)
        {
            specialGauge = Mathf.Min(100f, specialGauge + gaugeRegenRate * Time.deltaTime);
            OnGaugeChanged?.Invoke(specialGauge);
        }

        // Input for selecting special shots
        HandleSpecialShotInput();
        
        // Sync selected shot type to GolfBallController
        SyncShotTypeToController();
    }
    
    /// <summary>
    /// Sync shot type ไปยัง GolfBallController
    /// </summary>
    void SyncShotTypeToController()
    {
        if (ballController == null) return;
        
        // แปลง SpecialShotType ของเราไปเป็นของ GolfBallController
        switch (currentShot)
        {
            case SpecialShotType.Normal:
                ballController.currentShotType = GolfBallController.SpecialShotType.Normal;
                break;
            case SpecialShotType.Spike:
                ballController.currentShotType = GolfBallController.SpecialShotType.Spike;
                break;
            case SpecialShotType.Tomahawk:
                ballController.currentShotType = GolfBallController.SpecialShotType.Tomahawk;
                break;
            case SpecialShotType.Cobra:
                ballController.currentShotType = GolfBallController.SpecialShotType.Cobra;
                break;
        }
    }

    /// <summary>
    /// จัดการ Input เลือก Special Shot
    /// 1 = Normal, 2 = Spike, 3 = Tomahawk, 4 = Cobra
    /// </summary>
    void HandleSpecialShotInput()
    {
        // Keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            SelectShot(SpecialShotType.Normal);
        
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            SelectShot(SpecialShotType.Spike);
        
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            SelectShot(SpecialShotType.Tomahawk);
        
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            SelectShot(SpecialShotType.Cobra);

        // Controller: LB/RB to cycle
        if (Input.GetKeyDown(KeyCode.Q))
            CycleShot(-1);
        if (Input.GetKeyDown(KeyCode.E))
            CycleShot(1);
    }

    /// <summary>
    /// เลือก Special Shot
    /// </summary>
    public void SelectShot(SpecialShotType type)
    {
        // เช็คว่ามี gauge พอไหม (ยกเว้น Normal)
        if (type != SpecialShotType.Normal && specialGauge < specialShotCost)
        {
            Debug.Log($"❌ Not enough gauge! Need {specialShotCost}, have {specialGauge:F0}");
            return;
        }

        currentShot = type;
        OnSpecialShotSelected?.Invoke(type);
        
        // Sync to GolfBallController immediately
        SyncShotTypeToController();
        
        Debug.Log($"🎯 Selected: {type}");
    }

    /// <summary>
    /// วน Special Shot
    /// </summary>
    public void CycleShot(int direction)
    {
        int count = System.Enum.GetValues(typeof(SpecialShotType)).Length;
        int current = (int)currentShot;
        current = (current + direction + count) % count;
        SelectShot((SpecialShotType)current);
    }

    /// <summary>
    /// ได้รับชื่อ Special Shot
    /// </summary>
    public string GetShotName()
    {
        return currentShot.ToString();
    }

    /// <summary>
    /// ได้รับสี Special Shot ตาม Pangya style
    /// </summary>
    public Color GetShotColor()
    {
        switch (currentShot)
        {
            case SpecialShotType.Spike:
                return Color.yellow;   // 🟡 เหลือง
            case SpecialShotType.Tomahawk:
                return Color.red;      // 🔴 แดง
            case SpecialShotType.Cobra:
                return Color.cyan;     // 🔵 ฟ้า
            default:
                return Color.green;    // 🟢 เขียว (Normal)
        }
    }

    void OnDestroy()
    {
        // ไม่มี event listener ที่ต้อง unsubscribe แล้ว
    }
}
