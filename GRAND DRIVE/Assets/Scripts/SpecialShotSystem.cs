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

    // Private
    private bool isExecutingSpecial = false;
    private float specialShotTimer = 0f;
    private Rigidbody ballRb;
    private bool hasReachedApex = false;
    private float lastYVelocity = 0f;
    private Vector3 forwardDirection;

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

        // Subscribe to swing complete
        if (swingSystem != null)
        {
            swingSystem.OnSwingComplete.AddListener(OnSwingComplete);
        }

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

        // Handle special shot timing
        if (isExecutingSpecial)
        {
            specialShotTimer += Time.deltaTime;
            ExecuteSpecialShotPhysics();
        }

        // Input for selecting special shots
        HandleSpecialShotInput();
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
    /// เรียกเมื่อตีลูกเสร็จ
    /// </summary>
    void OnSwingComplete(float power, float accuracy, bool isPerfect)
    {
        if (currentShot != SpecialShotType.Normal)
        {
            // หัก gauge
            specialGauge -= specialShotCost;
            OnGaugeChanged?.Invoke(specialGauge);
            
            // เริ่ม execute special shot
            isExecutingSpecial = true;
            specialShotTimer = 0f;
            hasReachedApex = false;
            lastYVelocity = 0f;
            
            // เก็บทิศทางไปข้างหน้า
            if (ballRb != null)
            {
                forwardDirection = ballRb.linearVelocity;
                forwardDirection.y = 0;
                forwardDirection.Normalize();
                if (forwardDirection.magnitude < 0.1f)
                {
                    forwardDirection = ballController.transform.forward;
                }
            }
            
            // Apply initial special shot physics
            ApplyInitialSpecialShot(power);
            
            OnSpecialShotExecuted?.Invoke(currentShot);
            Debug.Log($"✨ {currentShot} SHOT! ✨");
        }
    }

    /// <summary>
    /// ใส่ Physics เริ่มต้นตาม Shot Type
    /// </summary>
    void ApplyInitialSpecialShot(float power)
    {
        if (ballRb == null) return;

        // หยุด velocity เดิมก่อน แล้วใส่ใหม่ตาม shot type
        float speed = ballRb.linearVelocity.magnitude * power;

        switch (currentShot)
        {
            case SpecialShotType.Spike:
                // 🟡 Spike: ยิงขึ้นสูงที่สุด (มุม 75°+)
                float spikeAngleRad = spikeLaunchAngle * Mathf.Deg2Rad;
                Vector3 spikeDir = forwardDirection * Mathf.Cos(spikeAngleRad) + 
                                   Vector3.up * Mathf.Sin(spikeAngleRad);
                ballRb.linearVelocity = spikeDir * speed;
                ballRb.AddForce(Vector3.up * spikeLiftForce, ForceMode.Impulse);
                Debug.Log($"🟡 SPIKE: Launch angle {spikeLaunchAngle}° - HIGHEST trajectory!");
                break;

            case SpecialShotType.Tomahawk:
                // 🔴 Tomahawk: ยิงขึ้นสูงมาก (มุม 65°)
                float tomahawkAngleRad = tomahawkLaunchAngle * Mathf.Deg2Rad;
                Vector3 tomahawkDir = forwardDirection * Mathf.Cos(tomahawkAngleRad) + 
                                      Vector3.up * Mathf.Sin(tomahawkAngleRad);
                ballRb.linearVelocity = tomahawkDir * speed;
                ballRb.AddForce(Vector3.up * tomahawkLiftForce, ForceMode.Impulse);
                Debug.Log($"🔴 TOMAHAWK: Launch angle {tomahawkLaunchAngle}° - Will drop straight down!");
                break;

            case SpecialShotType.Cobra:
                // 🔵 Cobra: ยิงต่ำมาก (มุม 12°) → เด้งหลายครั้ง
                float cobraAngleRad = cobraLaunchAngle * Mathf.Deg2Rad;
                Vector3 cobraDir = forwardDirection * Mathf.Cos(cobraAngleRad) + 
                                   Vector3.up * Mathf.Sin(cobraAngleRad);
                ballRb.linearVelocity = cobraDir * cobraForwardForce;
                Debug.Log($"🔵 COBRA: Launch angle {cobraLaunchAngle}° - LOW trajectory, will bounce!");
                break;
        }
    }

    /// <summary>
    /// Execute Physics ตลอดเวลาที่ลูกบิน
    /// </summary>
    void ExecuteSpecialShotPhysics()
    {
        if (ballRb == null || !ballController.IsInAir)
        {
            // ลูกตกพื้นแล้ว
            HandleSpecialShotLanding();
            return;
        }

        // ตรวจจับ Apex (จุดสูงสุด) - เมื่อ Y velocity เปลี่ยนจากบวกเป็นลบ
        float currentYVelocity = ballRb.linearVelocity.y;
        
        if (!hasReachedApex && lastYVelocity > 0 && currentYVelocity <= 0)
        {
            hasReachedApex = true;
            OnReachedApex();
        }
        
        lastYVelocity = currentYVelocity;

        // Execute continuous physics based on shot type
        switch (currentShot)
        {
            case SpecialShotType.Tomahawk:
                // 🔴 หลังถึง apex → กดลงตรงๆ แรงมาก
                if (hasReachedApex)
                {
                    ballRb.AddForce(Vector3.down * tomahawkDropForce, ForceMode.Force);
                }
                break;

            case SpecialShotType.Spike:
                // 🟡 หลังถึง apex → พุ่งเฉียงลงไปข้างหน้า
                // (จัดการใน OnReachedApex แล้ว)
                break;

            case SpecialShotType.Cobra:
                // 🔵 ไม่ต้องทำอะไรระหว่างบิน ให้ physics ปกติทำงาน
                break;
        }
    }

    /// <summary>
    /// เรียกเมื่อลูกถึงจุดสูงสุด (Apex)
    /// </summary>
    void OnReachedApex()
    {
        Debug.Log($"📍 Reached APEX! Shot: {currentShot}");

        switch (currentShot)
        {
            case SpecialShotType.Spike:
                // 🟡 Spike: พุ่งเฉียงลงไปข้างหน้า (ไม่ใช่ตกตรง)
                float diveAngleRad = spikeDiveAngle * Mathf.Deg2Rad;
                Vector3 diveDir = forwardDirection * Mathf.Cos(diveAngleRad) + 
                                  Vector3.down * Mathf.Sin(diveAngleRad);
                ballRb.linearVelocity = diveDir.normalized * spikeDiveForce;
                Debug.Log($"🟡 SPIKE: Diving at {spikeDiveAngle}° angle!");
                break;

            case SpecialShotType.Tomahawk:
                // 🔴 Tomahawk: เริ่มดิ่งลงตรงๆ
                // หยุด velocity แนวนอน ให้ตกตรงลง
                Vector3 vel = ballRb.linearVelocity;
                ballRb.linearVelocity = new Vector3(vel.x * 0.1f, vel.y, vel.z * 0.1f);
                Debug.Log($"🔴 TOMAHAWK: Dropping STRAIGHT down!");
                break;
        }
    }

    /// <summary>
    /// จัดการเมื่อลูกตกพื้น
    /// </summary>
    void HandleSpecialShotLanding()
    {
        switch (currentShot)
        {
            case SpecialShotType.Spike:
            case SpecialShotType.Tomahawk:
                // 🟡🔴 หยุดนิ่งทันที!
                if (ballController != null)
                {
                    ballController.StopBallImmediately();
                }
                Debug.Log($"💥 {currentShot}: DEAD STOP!");
                break;

            case SpecialShotType.Cobra:
                // 🔵 Cobra: ปล่อยให้เด้งตามปกติ (ไม่ต้องทำอะไร)
                Debug.Log($"🔵 COBRA: Bouncing...");
                break;
        }

        isExecutingSpecial = false;
        currentShot = SpecialShotType.Normal;
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
        if (swingSystem != null)
        {
            swingSystem.OnSwingComplete.RemoveListener(OnSwingComplete);
        }
    }
}
