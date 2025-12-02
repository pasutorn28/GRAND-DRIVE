using UnityEngine;

public class GolfBallController : MonoBehaviour
{
    [Header("--- Golf Physics Settings ---")]
    public float powerMultiplier = 20f;   // ความแรงในการตี
    public float spinMultiplier = 50f;    // ความแรงในการหมุน (ส่งผลต่อการเลี้ยว/หยุด)
    public float magnusCoefficient = 1.0f; // ค่าสัมประสิทธิ์แรงยก (ยิ่งเยอะ ลูกยิ่งเลี้ยวจัด)

    [Header("--- Environment ---")]
    public Vector3 windDirection = new Vector3(0, 0, 0); // ทิศทางลม (X,Y,Z)

    [Header("--- Dynamic Impact Point (Simulation) ---")]
    [Tooltip("จุดตีแนวนอน: -1(ซ้ายสุด/Hook) ถึง 1(ขวาสุด/Slice)")]
    [Range(-1f, 1f)] public float impactHorizontal = 0f; 

    [Tooltip("จุดตีแนวตั้ง: -1(ล่างสุด/Backspin) ถึง 1(บนสุด/Topspin)")]
    [Range(-1f, 1f)] public float impactVertical = 0f;

    private Rigidbody rb;
    private bool isInAir = false;
    private BallCameraController cameraController;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // หากล้องที่ติดตามลูก
        cameraController = FindFirstObjectByType<BallCameraController>();
    }

    void FixedUpdate()
    {
        // ฟิสิกส์จะทำงานเมื่อลูกลอยอยู่และมีความเร็วเท่านั้น
        if (isInAir && rb.linearVelocity.magnitude > 0.1f) // Unity 6 ใช้ linearVelocity แทน velocity
        {
            ApplyEnvironmentEffects();
        }

        // เช็คว่าลูกหยุดหรือยัง
        if (isInAir && rb.linearVelocity.magnitude < 0.1f && transform.position.y < 0.6f)
        {
            isInAir = false;
            
            // แจ้งกล้องให้หยุดติดตาม
            if (cameraController != null)
            {
                cameraController.StopFollowing();
            }
            
            Debug.Log("Ball Stopped / Ready to shoot again");
        }
    }

    void Update()
    {
        // TEST: กด Spacebar เพื่อทดสอบการตี (ดีกว่าปุ่ม UI ในช่วงแรก)
        if (Input.GetKeyDown(KeyCode.Space) && !isInAir)
        {
            ShootBall(1.0f); // ตีด้วยแรง 100%
        }

        // TEST: กด R เพื่อรีเซ็ตลูกกลับมาที่เดิม
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBall();
        }
    }

    // ฟังก์ชันสั่งตีลูก
    public void ShootBall(float powerPercentage)
    {
        isInAir = true;

        // 1. คำนวณทิศทาง: ตีไปข้างหน้า (Z) และงัดขึ้นนิดหน่อย (Y)
        Vector3 shotDir = (transform.forward + new Vector3(0, 0.3f, 0)).normalized;
        
        // 2. ใส่แรงระเบิด (Impulse)
        float totalPower = powerPercentage * powerMultiplier;
        rb.AddForce(shotDir * totalPower, ForceMode.Impulse);

        // 3. ใส่การหมุน (Torque) ตามจุด Impact
        // Impact Vertical (บน/ล่าง) -> หมุนแกน X (Topspin = หมุนไปข้างหน้า, Backspin = หมุนกลับ)
        // Impact Horizontal (ซ้าย/ขวา) -> หมุนแกน Y (Side Spin สำหรับ Hook/Slice)
        // Note: ค่าติดลบ impactVertical = ตีใต้ลูก = Backspin = หมุนแกน X ในทิศบวก
        Vector3 spinAxis = new Vector3(-impactVertical, impactHorizontal, 0);
        rb.AddTorque(spinAxis * spinMultiplier, ForceMode.Impulse);
        
        Debug.Log($"Spin Applied: X={-impactVertical * spinMultiplier}, Y={impactHorizontal * spinMultiplier}");

        // แจ้งกล้องให้เริ่มติดตามลูก
        if (cameraController != null)
        {
            cameraController.StartFollowing();
        }

        Debug.Log($"SCH-WING! Hit at Point: X={impactHorizontal}, Y={impactVertical}");
    }

    void ApplyEnvironmentEffects()
    {
        // 1. ใส่แรงลม
        rb.AddForce(windDirection, ForceMode.Force);

        // 2. ใส่ Magnus Effect (แรงยกจากการหมุน)
        // สูตรฟิสิกส์: แรงยก = ความเร็ว x ความเร็วเชิงมุม
        Vector3 magnusForce = Vector3.Cross(rb.linearVelocity, rb.angularVelocity) * magnusCoefficient;
        rb.AddForce(magnusForce);
    }

    void ResetBall()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = new Vector3(0, 0.5f, 0);
        transform.rotation = Quaternion.identity;
        isInAir = false;
    }
}