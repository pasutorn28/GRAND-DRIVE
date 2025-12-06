using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ระบบ Special Shots - UI และ Gauge Management
/// Special Shots System - UI selection and gauge management
/// 
/// NOTE: Physics logic อยู่ใน GolfBallController.cs
/// NOTE: Config ค่าต่างๆ อยู่ใน ShotConfig.cs (ScriptableObject)
/// 
/// 🟢 Normal: โค้งปกติ กลิ้งต่อได้
/// 🟡 Spike: ขึ้นสูงที่สุด → พุ่งเฉียงลง → หยุดนิ่งทันที
/// 🔴 Tomahawk: ขึ้นสูงมาก → ดิ่งตรง → หยุดนิ่งทันที  
/// 🔵 Cobra: ต่ำมาก → พุ่งขึ้น → ลงตรง
/// </summary>
public class SpecialShotSystem : MonoBehaviour
{
    [Header("--- References ---")]
    public GolfBallController ballController;
    public SwingSystem swingSystem;

    [Header("--- Special Shot Types ---")]
    [Tooltip("ท่าตีปัจจุบัน (ใช้ enum กลางจาก SpecialShotType.cs)")]
    public SpecialShotType currentShot = SpecialShotType.Normal;

    // NOTE: Config ค่า Shot ทั้งหมดย้ายไป ShotConfig.cs แล้ว
    // ไฟล์นี้เหลือแค่ Gauge และ UI selection

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

    void Start()
    {
        if (ballController == null)
            ballController = FindFirstObjectByType<GolfBallController>();
        
        if (swingSystem == null)
            swingSystem = FindFirstObjectByType<SwingSystem>();

        // NOTE: Physics logic อยู่ใน GolfBallController.cs
        // Script นี้ใช้สำหรับ UI และ Gauge เท่านั้น
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
        
        // ใช้ enum กลางแล้ว ไม่ต้องแปลง
        ballController.currentShotType = currentShot;
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
        // CONFLICT: Disabled to allow Club Switching on Q/E
        /*
        if (Input.GetKeyDown(KeyCode.Q))
            CycleShot(-1);
        if (Input.GetKeyDown(KeyCode.E))
            CycleShot(1);
        */
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
}
