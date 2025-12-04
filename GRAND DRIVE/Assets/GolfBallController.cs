using UnityEngine;

public class GolfBallController : MonoBehaviour
{
    public enum SpecialShotType { Normal, Spike, Tomahawk, Cobra }

    [Header("--- Golf Physics Settings ---")]
    public float powerMultiplier = 50f;   // ความแรงในการตี (50 = ~200y at 87% power)
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

    private Rigidbody rb;
    private bool isInAir = false;
    private bool isApexReached = false;
    private BallCameraController cameraController;
    private float lastShotTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // FIX: Use Continuous to avoid physics explosions
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
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
        
        // SAFETY CHECK: ปิดไว้ก่อนเพื่อ debug - ไม่ reset อัตโนมัติ
        // if (transform.position.y < -50f || transform.position.y > 1000f)
        // {
        //     Debug.LogError($"⚠️ Ball out of bounds! Pos: {transform.position}");
        //     ResetBall();
        //     return;
        // }

        // ฟิสิกส์จะทำงานเมื่อลูกลอยอยู่และมีความเร็วเท่านั้น
        if (isInAir && rb.linearVelocity.magnitude > 0.1f)
        {
            ApplyEnvironmentEffects();
            HandleSpecialShotPhysics();
        }

        // เช็คว่าลูกหยุดหรือยัง - ต้องรอ 3 วินาทีหลังตี
        if (isInAir && Time.time - lastShotTime > 3.0f)
        {
            // ลูกหยุดเมื่อ: ความเร็วต่ำมาก AND ลูกแตะพื้นแล้ว
            bool isSlowEnough = rb.linearVelocity.magnitude < 0.5f;
            bool isOnGround = transform.position.y < 2.0f && transform.position.y > -5f;
            
            if (isSlowEnough && isOnGround)
            {
                Debug.Log($"⛳ Ball stopped at: {transform.position}, Velocity: {rb.linearVelocity.magnitude:F2}");
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

        // Check for Apex (Vertical velocity changes from positive to negative)
        if (rb.linearVelocity.y < 0)
        {
            isApexReached = true;
            OnApexReached();
        }
    }

    void OnApexReached()
    {
        switch (currentShotType)
        {
            case SpecialShotType.Spike:
                // Spike: Dive down at 45 degrees
                // Keep current horizontal speed but force downward angle
                Vector3 currentVel = rb.linearVelocity;
                float speed = currentVel.magnitude;
                
                // Calculate new direction: Forward + Down (1:1 ratio for 45 degrees)
                Vector3 flatForward = new Vector3(currentVel.x, 0, currentVel.z).normalized;
                Vector3 diveDir = (flatForward + Vector3.down).normalized;
                
                rb.linearVelocity = diveDir * (speed * 1.5f); // Accelerate down
                Debug.Log("🟡 SPIKE APEX! Diving down!");
                break;

            case SpecialShotType.Tomahawk:
                // Tomahawk: Drop straight down (Zero horizontal velocity)
                rb.linearVelocity = new Vector3(0, -50f, 0); // Strong downward force
                Debug.Log("🔴 TOMAHAWK APEX! Dropping straight down!");
                break;
        }
    }

    void Update()
    {
        // TEST MODE: กด Space ครั้งเดียวตีเลย 200y Perfect
        if (Input.GetKeyDown(KeyCode.Space) && !isInAir)
        {
            // 200y / 230y max = 0.87 power
            float testPower = 200f / 230f; // ≈ 0.87
            ShootBall(testPower);
            Debug.Log($"🎯 TEST SHOT: 200y Perfect! (Power: {testPower:P0})");
            
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
        lastShotTime = Time.time; // Record shot time
        
        // Ensure physics is active
        rb.isKinematic = false;

        float launchAngle = 0f;
        float powerMod = 1.0f;

        // Determine launch parameters based on shot type
        switch (currentShotType)
        {
            case SpecialShotType.Normal:
                launchAngle = 30f; // Normal arc
                break;
            case SpecialShotType.Spike:
                launchAngle = 75f; // High launch
                powerMod = 1.2f;   // Needs more power to go distance
                break;
            case SpecialShotType.Tomahawk:
                launchAngle = 65f; // High launch
                powerMod = 1.1f;
                break;
            case SpecialShotType.Cobra:
                launchAngle = 12f; // Low skim
                powerMod = 1.3f;   // Needs speed to skim
                break;
        }

        // 1. คำนวณทิศทาง
        // Convert angle to direction vector
        // Forward is Z, Up is Y. 
        // Rotate forward vector up by launchAngle around X axis
        Vector3 forwardDir = transform.forward;
        Vector3 shotDir = Quaternion.AngleAxis(-launchAngle, transform.right) * forwardDir;
        
        // 2. ใส่แรงระเบิด (Impulse)
        float totalPower = powerPercentage * powerMultiplier * powerMod;
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

        Debug.Log($"SCH-WING! Shot: {currentShotType} | Angle: {launchAngle}° | Power: {totalPower}");
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

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"🏐 Ball hit: {collision.gameObject.name} at {transform.position}");
        
        // ถ้าลูกหยุดแล้ว ไม่ต้องทำอะไร
        if (!isInAir || rb.isKinematic) return;

        // Special handling for landing
        if (currentShotType == SpecialShotType.Spike || currentShotType == SpecialShotType.Tomahawk)
        {
            // Stop immediately on first bounce
            StopBallImmediately();
        }
        else if (currentShotType == SpecialShotType.Cobra)
        {
            // Cobra Skim Logic: Maintain forward speed on bounce
            Vector3 velocity = rb.linearVelocity;
            Vector3 forwardDir = new Vector3(velocity.x, 0, velocity.z).normalized;
            float currentSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            
            // Apply a forward boost to simulate "skimming"
            if (currentSpeed > 2.0f)
            {
                rb.AddForce(forwardDir * 5.0f, ForceMode.Impulse);
                Debug.Log("🔵 Cobra Skim Boost!");
            }
        }
        // Normal shot - ไม่ทำอะไรพิเศษ ปล่อยให้เด้งตามปกติ
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