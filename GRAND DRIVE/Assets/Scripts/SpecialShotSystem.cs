using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ระบบ Special Shots - Tomahawk, Spike, Cobra
/// Special Shots System - Unique trajectories that change ball physics
/// </summary>
public class SpecialShotSystem : MonoBehaviour
{
    [Header("--- References ---")]
    public GolfBallController ballController;
    public SwingSystem swingSystem;

    [Header("--- Special Shot Types ---")]
    public SpecialShotType currentShot = SpecialShotType.Normal;

    [Header("--- Tomahawk Settings ---")]
    [Tooltip("แรงยกพิเศษ (ตีให้สูงแล้วตกลงแรง)")]
    public float tomahawkLiftForce = 15f;
    
    [Tooltip("เวลา delay ก่อนใส่แรงกด")]
    public float tomahawkDropDelay = 0.5f;
    
    [Tooltip("แรงกดลง")]
    public float tomahawkDropForce = 30f;

    [Header("--- Spike Settings ---")]
    [Tooltip("แรงตีต่ำ (ตีแบนๆ ไปข้างหน้าเร็ว)")]
    public float spikeForwardForce = 25f;
    
    [Tooltip("มุมต่ำ (องศา)")]
    public float spikeLowAngle = 5f;
    
    [Tooltip("หมุนหลังแรงมาก (หยุดเร็ว)")]
    public float spikeBackspinMultiplier = 2f;

    [Header("--- Cobra Settings ---")]
    [Tooltip("แรง Side Spin พิเศษ")]
    public float cobraSideSpinForce = 40f;
    
    [Tooltip("ทิศทาง: 1 = ขวา (Slice), -1 = ซ้าย (Hook)")]
    public float cobraDirection = 1f;
    
    [Tooltip("เวลา delay ก่อนเลี้ยว")]
    public float cobraCurveDelay = 0.3f;

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

    public enum SpecialShotType
    {
        Normal,     // ตีปกติ
        Tomahawk,   // ตีขึ้นสูงแล้วกดลง (เหมือนขวาน)
        Spike,      // ตีแบนๆ ไปข้างหน้าเร็ว หยุดทันที
        Cobra       // เลี้ยวกลางอากาศ (งูเห่า)
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
    /// 1 = Normal, 2 = Tomahawk, 3 = Spike, 4 = Cobra
    /// </summary>
    void HandleSpecialShotInput()
    {
        // Keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            SelectShot(SpecialShotType.Normal);
        
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            SelectShot(SpecialShotType.Tomahawk);
        
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            SelectShot(SpecialShotType.Spike);
        
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
            
            // Apply initial special shot physics
            ApplyInitialSpecialShot();
            
            OnSpecialShotExecuted?.Invoke(currentShot);
            Debug.Log($"✨ {currentShot} SHOT! ✨");
        }
    }

    /// <summary>
    /// ใส่ Physics เริ่มต้นตาม Shot Type
    /// </summary>
    void ApplyInitialSpecialShot()
    {
        if (ballRb == null) return;

        switch (currentShot)
        {
            case SpecialShotType.Tomahawk:
                // เพิ่มแรงยกขึ้น
                ballRb.AddForce(Vector3.up * tomahawkLiftForce, ForceMode.Impulse);
                break;

            case SpecialShotType.Spike:
                // เปลี่ยนทิศให้ต่ำลง + เพิ่ม backspin
                Vector3 currentVel = ballRb.linearVelocity;
                Vector3 flatDirection = new Vector3(currentVel.x, 0, currentVel.z).normalized;
                float spikeAngleRad = spikeLowAngle * Mathf.Deg2Rad;
                
                Vector3 spikeDir = flatDirection * Mathf.Cos(spikeAngleRad) + 
                                   Vector3.up * Mathf.Sin(spikeAngleRad);
                
                ballRb.linearVelocity = spikeDir * spikeForwardForce;
                
                // เพิ่ม backspin
                ballRb.AddTorque(Vector3.right * ballController.spinMultiplier * spikeBackspinMultiplier, 
                                ForceMode.Impulse);
                break;

            case SpecialShotType.Cobra:
                // เพิ่ม side spin พิเศษ (จะเลี้ยวทีหลัง)
                ballRb.AddTorque(Vector3.up * cobraSideSpinForce * cobraDirection, ForceMode.Impulse);
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
            isExecutingSpecial = false;
            // รีเซ็ตกลับ Normal หลังตี
            currentShot = SpecialShotType.Normal;
            return;
        }

        switch (currentShot)
        {
            case SpecialShotType.Tomahawk:
                // หลังจาก delay ให้กดลงแรง
                if (specialShotTimer > tomahawkDropDelay)
                {
                    ballRb.AddForce(Vector3.down * tomahawkDropForce, ForceMode.Force);
                }
                break;

            case SpecialShotType.Cobra:
                // หลังจาก delay ให้เลี้ยว
                if (specialShotTimer > cobraCurveDelay)
                {
                    // เพิ่มแรงเลี้ยวต่อเนื่อง
                    Vector3 currentVel = ballRb.linearVelocity;
                    Vector3 sideForce = Vector3.Cross(currentVel.normalized, Vector3.up) * 
                                       cobraSideSpinForce * cobraDirection * 0.1f;
                    ballRb.AddForce(sideForce, ForceMode.Force);
                }
                break;
        }
    }

    /// <summary>
    /// ได้รับชื่อ Special Shot
    /// </summary>
    public string GetShotName()
    {
        return currentShot.ToString();
    }

    /// <summary>
    /// ได้รับสี Special Shot
    /// </summary>
    public Color GetShotColor()
    {
        switch (currentShot)
        {
            case SpecialShotType.Tomahawk:
                return new Color(1f, 0.5f, 0f);  // ส้ม
            case SpecialShotType.Spike:
                return new Color(0.3f, 1f, 0.3f);  // เขียว
            case SpecialShotType.Cobra:
                return new Color(0.8f, 0.3f, 1f);  // ม่วง
            default:
                return Color.white;
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
