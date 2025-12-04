using UnityEngine;

public class GolfBallController : MonoBehaviour
{

    [Header("--- Golf Physics Settings ---")]
    public float powerMultiplier = 6f;   // ความแรงในการตี (6 = ~200y at 87% power)
    public float spinMultiplier = 50f;    // ความแรงในการหมุน (ส่งผลต่อการเลี้ยว/หยุด)
    public float magnusCoefficient = 1.0f; // ค่าสัมประสิทธิ์แรงยก (ยิ่งเยอะ ลูกยิ่งเลี้ยวจัด)

    [Header("--- Environment ---")]
    public Vector3 windDirection = new Vector3(0, 0, 0); // ทิศทางลม (X,Y,Z)

    [Header("--- Dynamic Impact Point (Simulation) ---")]
    [Tooltip("จุดตีแนวนอน: -1(ซ้ายสุด/Hook) ถึง 1(ขวาสุด/Slice)")]
    [Range(-1f, 1f)] public float impactHorizontal = 0f; 

    [Tooltip("จุดตีแนวตั้ง: -1(ล่างสุด/Backspin) ถึง 1(บนสุด/Topspin)")]
    [Range(-1f, 1f)] public float impactVertical = 0f;

    [Header("--- Special Shots ---")]
    public SpecialShotType currentShotType = SpecialShotType.Normal;

    [Header("--- Swing System ---")]
    [Tooltip("อ้างอิง SwingSystem (ถ้าไม่กำหนดจะหาอัตโนมัติ)")]
    public SwingSystem swingSystem;
    
    [Tooltip("ใช้ SwingSystem แทนการกด Spacebar ตรงๆ")]
    public bool useSwingSystem = true;

    [Header("--- Character Stats ---")]
    [Tooltip("อ้างอิง CharacterStats (ถ้าไม่กำหนดจะหาอัตโนมัติ)")]
    public CharacterStats characterStats;

    [Header("--- Shot Config ---")]
    [Tooltip("ScriptableObject เก็บค่า config ของ Special Shots (ถ้าไม่กำหนดจะใช้ค่า default)")]
    public ShotConfig shotConfig;

    private Rigidbody rb;
    private bool isInAir = false;
    private bool isApexReached = false;
    private bool hasLanded = false; // ลูกตกพื้นแล้วหรือยัง
    private bool cobraLaunched = false; // Cobra พุ่งขึ้นแล้วหรือยัง
    private Vector3 startPosition; // ตำแหน่งเริ่มต้น
    private float expectedDistance; // ระยะที่คาดหวัง (คำนวณจาก power)
    private BallCameraController cameraController;
    private float lastShotTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // FIX: Use Continuous to avoid physics explosions
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // ⚠️ FORCE: บังคับใช้ค่า powerMultiplier จาก ShotConfig หรือ default
        if (shotConfig != null)
        {
            powerMultiplier = shotConfig.powerMultiplier;
        }
        else
        {
            powerMultiplier = 2.045f; // Default: power 100% = 183m (200y)
        }
        
        // ⭐ เริ่มต้นลูกให้หยุดนิ่ง ไม่ให้ตก
        rb.isKinematic = true;
        // NOTE: ไม่ต้อง set velocity ตอน kinematic (จะ error)
        
        // หากล้องที่ติดตามลูก
        cameraController = FindFirstObjectByType<BallCameraController>();
        
        // หา SwingSystem อัตโนมัติ
        if (swingSystem == null)
        {
            swingSystem = FindFirstObjectByType<SwingSystem>();
        }
        
        // หา CharacterStats อัตโนมัติ
        if (characterStats == null)
        {
            characterStats = FindFirstObjectByType<CharacterStats>();
        }
        
        // Subscribe to SwingSystem events
        if (swingSystem != null && useSwingSystem)
        {
            swingSystem.OnSwingComplete.AddListener(OnSwingComplete);
        }
    }

    void FixedUpdate()
    {
        // ถ้าลูกหยุดแล้ว (isKinematic = true) ไม่ต้องทำอะไร
        if (rb.isKinematic) return;
        
        float speed = rb.linearVelocity.magnitude;
        float angularSpeed = rb.angularVelocity.magnitude;
        
        // ⚠️ FIX: ลด spin เฉพาะตอนลูกช้ามากๆ เท่านั้น (ให้ลูกกลิ้งได้ตามธรรมชาติ)
        if (speed < 0.5f && angularSpeed > 0.1f)
        {
            rb.angularVelocity *= 0.98f; // ลด spin ลง 2% ทุก frame (ช้าลงกว่าเดิม)
        }

        // ฟิสิกส์จะทำงานเมื่อลูกลอยอยู่ และยังไม่ตกพื้น
        if (isInAir && !hasLanded && speed > 0.5f)
        {
            ApplyEnvironmentEffects();
            HandleSpecialShotPhysics();
        }

        // เช็คว่าลูกหยุดหรือยัง - ต้องรอ 3 วินาทีหลังตี
        if (isInAir && Time.time - lastShotTime > 3.0f)
        {
            // ลูกหยุดเมื่อ: ความเร็วต่ำมากๆ AND ลูกแตะพื้นแล้ว
            bool isSlowEnough = speed < 0.2f && angularSpeed < 0.5f;
            bool isOnGround = transform.position.y < 2.0f && transform.position.y > -5f;
            
            if (isSlowEnough && isOnGround)
            {
                Debug.Log($"⛳ Ball stopped at: {transform.position}, Velocity: {speed:F2}");
                StopBallAndRest();
            }
        }
    }
    
    /// <summary>
    /// หยุดลูกและพักรอตีใหม่
    /// </summary>
    void StopBallAndRest()
    {
        isInAir = false;
        isApexReached = false;
        
        // ⚠️ FIX: ต้อง set velocity ก่อน enable kinematic
        // หยุด velocity ทั้งหมด (ต้องทำก่อน kinematic)
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Lock ลูกไม่ให้ตก (หลังจาก clear velocity แล้ว)
        rb.isKinematic = true;
        
        // แจ้งกล้องให้หยุดติดตาม
        if (cameraController != null)
        {
            cameraController.StopFollowing();
        }
        
        // แจ้ง SwingSystem ว่าลูกหยุดแล้ว
        if (swingSystem != null)
        {
            swingSystem.OnBallStopped();
        }
        
        Debug.Log("⛳ Ball Stopped / Ready to shoot again");
    }

    void HandleSpecialShotPhysics()
    {
        if (isApexReached) return;

        // Cobra Phase 1: ต้านแรงโน้มถ่วงเพื่อให้ลูกบินเป็นเส้นตรงที่มุม 6°
        if (currentShotType == SpecialShotType.Cobra && !cobraLaunched)
        {
            // ต้านแรงโน้มถ่วง 100% เพื่อให้ลูกบินตรงๆ
            rb.AddForce(Vector3.up * Physics.gravity.magnitude * rb.mass, ForceMode.Force);
            
            float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), 
                                               new Vector3(startPosition.x, 0, startPosition.z));
            
            // Trigger Phase 2 เมื่อไปได้ตาม cobraTriggerRatio ของระยะทั้งหมด (dynamic)
            float triggerRatio = shotConfig != null ? shotConfig.cobraTriggerRatio : (4f / 6f);
            float cobraTriggerDistance = expectedDistance * triggerRatio;
            if (distance >= cobraTriggerDistance)
            {
                cobraLaunched = true;
                OnCobraLaunch();
                return;
            }
        }

        // Check for Apex (Vertical velocity changes from positive to negative)
        // Cobra ไม่ใช้ apex detection
        if (currentShotType != SpecialShotType.Cobra && rb.linearVelocity.y < 0)
        {
            isApexReached = true;
            OnApexReached();
        }
    }

    void OnApexReached()
    {
        // Get current velocity for all shot types
        Vector3 currentVel = rb.linearVelocity;
        float currentSpeed = currentVel.magnitude;
        
        switch (currentShotType)
        {
            case SpecialShotType.Spike:
                // Spike: พุ่งลงเฉียง 30° ด้วยความเร็ว 3.5 เท่าของตอนตี
                
                // คงทิศทางแนวนอนไว้ แต่เพิ่มความเร็ว 3.5x และหักลง 30°
                Vector3 flatForward = new Vector3(currentVel.x, 0, currentVel.z).normalized;
                
                // ถ้าไม่มีทิศแนวนอน ใช้ forward ของลูก
                if (flatForward.magnitude < 0.1f)
                {
                    flatForward = transform.forward;
                }
                
                // พุ่งลง (ใช้ spikeDiveAngle จาก config)
                float diveTan = shotConfig != null ? shotConfig.GetSpikeDiveTan() : 0.577f;
                Vector3 diveDir = (flatForward + Vector3.down * diveTan).normalized;
                
                // ความเร็วพุ่งลง (ใช้ spikeDiveSpeedMultiplier จาก config)
                float diveMultiplier = shotConfig != null ? shotConfig.spikeDiveSpeedMultiplier : 3.5f;
                float diveSpeed = currentSpeed * diveMultiplier;
                rb.linearVelocity = diveDir * diveSpeed;
                
                float diveAngle = shotConfig != null ? shotConfig.spikeDiveAngle : 30f;
                Debug.Log($"🟡 SPIKE APEX! Diving at {diveSpeed:F1} m/s ({diveMultiplier}x, {diveAngle}° angle)");
                break;

            // Cobra: ใช้ OnCobraLaunch() แทน (trigger ที่ระยะ 120m)
            // Tomahawk: ไม่มี apex dive - ตีเหมือน Normal แต่ไม่เด้ง
        }
    }

    void OnCobraLaunch()
    {
        // Cobra Phase 2: หยุดต้านแรงโน้มถ่วง แล้วพุ่งขึ้นสูง!
        Vector3 currentVel = rb.linearVelocity;
        float currentSpeed = currentVel.magnitude;
        
        Vector3 cobraForward = new Vector3(currentVel.x, 0, currentVel.z).normalized;
        if (cobraForward.magnitude < 0.1f) cobraForward = transform.forward;
        
        // พุ่งขึ้นมุม (ใช้ค่าจาก ShotConfig)
        float cobraLaunchAngle = shotConfig != null ? shotConfig.cobraLaunchAngle : 68f;
        float cobraSpeedMult = shotConfig != null ? shotConfig.cobraSpeedMultiplier : 1.1720f;
        
        // หมุนรอบแกนขวา (cross product ต้องเป็น up x forward ไม่ใช่ forward x up)
        Vector3 rightAxis = Vector3.Cross(Vector3.up, cobraForward);
        Vector3 cobraLaunchDir = Quaternion.AngleAxis(-cobraLaunchAngle, rightAxis) * cobraForward;
        float cobraSpeed = currentSpeed * cobraSpeedMult;
        
        rb.linearVelocity = cobraLaunchDir * cobraSpeed;
        float triggerRatio = shotConfig != null ? shotConfig.cobraTriggerRatio : (4f / 6f);
        float triggerDist = expectedDistance * triggerRatio;
        Debug.Log($"🐍 COBRA LAUNCH at {Vector3.Distance(transform.position, startPosition):F1}m! (trigger: {triggerDist:F1}m) Speed: {cobraSpeed:F1} m/s ({cobraLaunchAngle}° up)");
    }

    void Update()
    {
        // TEST MODE: กด Space ครั้งเดียวตีเลย 200y (100% Power)
        if (Input.GetKeyDown(KeyCode.Space) && !isInAir)
        {
            // 100% Power = 200y (183m)
            float testPower = 1.0f;
            ShootBall(testPower);
            Debug.Log($"🎯 TEST SHOT: 200y (100% Power)");
            
            if (swingSystem != null)
            {
                swingSystem.SetCooldown();
            }
            return;
        }
        
        // TEST: กด R เพื่อรีเซ็ตลูกกลับมาที่เดิม
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBall();
            if (swingSystem != null)
            {
                swingSystem.ResetSwing();
            }
        }
        
        // TEST KEYS FOR SPECIAL SHOTS (1-4)
        if (Input.GetKeyDown(KeyCode.Alpha1)) { currentShotType = SpecialShotType.Normal; Debug.Log("🟢 Selected: Normal Shot"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { currentShotType = SpecialShotType.Spike; Debug.Log("🟡 Selected: Spike Shot"); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { currentShotType = SpecialShotType.Tomahawk; Debug.Log("🔴 Selected: Tomahawk Shot"); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { currentShotType = SpecialShotType.Cobra; Debug.Log("🔵 Selected: Cobra Shot"); }
    }
    
    /// <summary>
    /// เรียกเมื่อ SwingSystem ตีเสร็จ
    /// Called when SwingSystem completes a swing
    /// </summary>
    void OnSwingComplete(float power, float accuracy, bool isPerfect)
    {
        if (isInAir) return; // ถ้าลูกยังอยู่กลางอากาศ ไม่ให้ตีซ้ำ
        
        // คำนวณพลังจริงจาก Power และ Accuracy
        float finalPower = power * accuracy;
        
        // ถ้า Perfect Impact ได้โบนัส 10%
        if (isPerfect)
        {
            finalPower = Mathf.Min(finalPower * 1.1f, 1.0f);
        }
        
        // ตีลูก!
        ShootBall(finalPower);
        
        // เปลี่ยน SwingSystem เป็น Cooldown
        if (swingSystem != null)
        {
            swingSystem.SetCooldown();
        }
    }

    // ฟังก์ชันสั่งตีลูก
    public void ShootBall(float powerPercentage)
    {
        isInAir = true;
        isApexReached = false;
        hasLanded = false; // Reset landing flag
        lastShotTime = Time.time;
        bounceCount = 0; // Reset bounce counter
        
        // Ensure physics is active
        rb.isKinematic = false;

        float launchAngle = 0f;
        float powerMod = 1.0f;

        // Determine launch parameters based on shot type (use ShotConfig if available)
        float distanceScale = 1.0f;
        switch (currentShotType)
        {
            case SpecialShotType.Normal:
                launchAngle = shotConfig != null ? shotConfig.normalLaunchAngle : 30f;
                powerMod = shotConfig != null ? shotConfig.normalPowerMod : 1.000f;
                distanceScale = shotConfig != null ? shotConfig.normalDistanceScale : 1.0f;
                break;
            case SpecialShotType.Spike:
                // Spike: ยิงสูงกว่า Normal แต่ต้องไปได้ไกลเท่ากัน
                launchAngle = shotConfig != null ? shotConfig.spikeLaunchAngle : 50f;
                powerMod = shotConfig != null ? shotConfig.spikePowerMod : 1.170f;
                distanceScale = shotConfig != null ? shotConfig.spikeDistanceScale : 1.0f;
                break;
            case SpecialShotType.Tomahawk:
                // Tomahawk: ตีเหมือน Normal แต่สูงกว่า และไม่เด้ง
                launchAngle = shotConfig != null ? shotConfig.tomahawkLaunchAngle : 40f;
                powerMod = shotConfig != null ? shotConfig.tomahawkPowerMod : 1.260f;
                distanceScale = shotConfig != null ? shotConfig.tomahawkDistanceScale : 1.0f;
                break;
            case SpecialShotType.Cobra:
                // Cobra Phase 1: ยิงต่ำบินเป็นเส้นตรง (ต้านแรงโน้มถ่วง)
                launchAngle = shotConfig != null ? shotConfig.cobraPhase1Angle : 6f;
                powerMod = shotConfig != null ? shotConfig.cobraPowerMod : 1.100f;
                distanceScale = shotConfig != null ? shotConfig.cobraDistanceScale : 1.0f;
                break;
        }

        // 1. คำนวณทิศทาง
        // Convert angle to direction vector
        // Forward is Z, Up is Y. 
        // Rotate forward vector up by launchAngle around X axis
        Vector3 forwardDir = transform.forward;
        Vector3 shotDir = Quaternion.AngleAxis(-launchAngle, transform.right) * forwardDir;
        
        // 2. ใส่แรงระเบิด (Impulse)
        // distanceScale ชดเชย non-linear physics เมื่อ powerMultiplier > 1.0
        float effectiveMultiplier = powerMultiplier > 1.0f ? powerMultiplier * distanceScale : powerMultiplier;
        float totalPower = powerPercentage * effectiveMultiplier * powerMod;
        
        // คำนวณระยะที่คาดหวัง (power 100% = targetDistance)
        float targetDist = shotConfig != null ? shotConfig.targetDistance : 183f;
        expectedDistance = powerPercentage * targetDist;
        
        // เก็บตำแหน่งเริ่มต้นสำหรับ Cobra
        startPosition = transform.position;
        cobraLaunched = false;
        
        rb.AddForce(shotDir * totalPower, ForceMode.Impulse);

        // 3. ใส่การหมุน (Torque) ตามจุด Impact
        // Impact Vertical (บน/ล่าง) -> หมุนแกน X (Topspin = หมุนไปข้างหน้า, Backspin = หมุนกลับ)
        // Impact Horizontal (ซ้าย/ขวา) -> หมุนแกน Y (Side Spin สำหรับ Hook/Slice)
        // Note: ค่าติดลบ impactVertical = ตีใต้ลูก = Backspin = หมุนแกน X ในทิศบวก
        
        // ใช้ CharacterStats SPN bonus
        float actualSpinMultiplier = characterStats != null 
            ? characterStats.GetSpinMultiplierWithBonus(spinMultiplier) 
            : spinMultiplier;
        
        Vector3 spinAxis = new Vector3(-impactVertical, impactHorizontal, 0);
        rb.AddTorque(spinAxis * actualSpinMultiplier, ForceMode.Impulse);
        
        Debug.Log($"Spin Applied: X={-impactVertical * actualSpinMultiplier}, Y={impactHorizontal * actualSpinMultiplier}");

        // แจ้งกล้องให้เริ่มติดตามลูก
        if (cameraController != null)
        {
            cameraController.StartFollowing();
        }

        Debug.Log($"SCH-WING! Shot: {currentShotType} | Angle: {launchAngle}° | Power: {totalPower} | distanceScale: {distanceScale} | effectiveMultiplier: {effectiveMultiplier}");
    }

    void ApplyEnvironmentEffects()
    {
        // ⚠️ FIX: Don't apply wind/magnus if we are in the "Dive" phase of a special shot
        // This ensures Spike/Tomahawk lines are straight and sharp as drawn
        if (isApexReached && (currentShotType == SpecialShotType.Spike || currentShotType == SpecialShotType.Tomahawk))
        {
            return;
        }

        // ⚠️ SAFETY: ไม่ใส่แรงเพิ่มถ้าลูกช้ามากแล้ว (ป้องกัน physics explosion)
        float speed = rb.linearVelocity.magnitude;
        if (speed < 1.0f)
        {
            return; // ลูกช้ามากแล้ว ไม่ต้องใส่ wind/magnus
        }

        // 1. ใส่แรงลม (เฉพาะเมื่อลูกยังเร็วอยู่)
        rb.AddForce(windDirection, ForceMode.Force);

        // 2. ใส่ Magnus Effect (แรงยกจากการหมุน)
        // สูตรฟิสิกส์: แรงยก = ความเร็ว x ความเร็วเชิงมุม
        // ใช้ CharacterStats CRV bonus
        float actualMagnus = characterStats != null 
            ? characterStats.GetMagnusCoefficientWithBonus(magnusCoefficient) 
            : magnusCoefficient;
        
        Vector3 magnusForce = Vector3.Cross(rb.linearVelocity, rb.angularVelocity) * actualMagnus;
        
        // ⚠️ SAFETY: จำกัดแรง magnus ไม่ให้เกิน
        if (magnusForce.magnitude > 50f)
        {
            magnusForce = magnusForce.normalized * 50f;
        }
        
        rb.AddForce(magnusForce);
    }

    void ResetBall()
    {
        Debug.Log($"🔄 ResetBall called! Was at: {transform.position}");
        
        // ⚠️ FIX: ต้อง disable kinematic ก่อน set velocity แล้วค่อย enable กลับ
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        
        transform.position = new Vector3(0, 0.5f, 0);
        transform.rotation = Quaternion.identity;
        isInAir = false;
        isApexReached = false;
    }
    
    /// <summary>
    /// หยุดลูกทันที (สำหรับ Spike/Tomahawk)
    /// Stop ball immediately (for Spike/Tomahawk special shots)
    /// </summary>
    public void StopBallImmediately()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true; // FIX: Prevent falling through map
        isInAir = false;
        
        // แจ้งกล้องให้หยุดติดตาม
        if (cameraController != null)
        {
            cameraController.StopFollowing();
        }
        
        // แจ้ง SwingSystem ว่าลูกหยุดแล้ว
        if (swingSystem != null)
        {
            swingSystem.OnBallStopped();
        }
        
        Debug.Log("💥 Ball DEAD STOP! / ลูกหยุดนิ่งทันที!");
    }

    private int bounceCount = 0; // นับจำนวนครั้งที่เด้ง
    
    void OnCollisionEnter(Collision collision)
    {
        // ถ้าลูกหยุดแล้ว ไม่ต้องทำอะไร
        if (!isInAir || rb.isKinematic) return;
        
        bounceCount++;
        hasLanded = true; // ลูกตกพื้นแล้ว - หยุด Magnus/Wind
        
        Debug.Log($"🏐 Ball hit: {collision.gameObject.name} at {transform.position} (bounce #{bounceCount})");
        
        // ⚠️ FIX: บังคับหยุดหลังเด้ง 10 ครั้ง
        if (bounceCount >= 10)
        {
            Debug.Log("⛳ Ball stopped after 10 bounces");
            StopBallAndRest();
            return;
        }

        // Special handling for landing
        if (currentShotType == SpecialShotType.Spike || currentShotType == SpecialShotType.Tomahawk)
        {
            // Stop immediately on first bounce
            StopBallImmediately();
        }
        else if (currentShotType == SpecialShotType.Cobra && cobraLaunched)
        {
            // Cobra Phase 2: ปล่อยให้เด้งตามธรรมชาติ (ไม่มี SUPER BOUNCE)
            Debug.Log($"🐍 Cobra bounce #{bounceCount}! vel.y = {rb.linearVelocity.y:F1}");
        }
        // Normal - ปล่อยให้เด้งตามปกติ
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (swingSystem != null)
        {
            swingSystem.OnSwingComplete.RemoveListener(OnSwingComplete);
        }
    }
    
    /// <summary>
    /// สถานะลูกอยู่กลางอากาศหรือไม่
    /// Is the ball currently in the air?
    /// </summary>
    public bool IsInAir => isInAir;
}