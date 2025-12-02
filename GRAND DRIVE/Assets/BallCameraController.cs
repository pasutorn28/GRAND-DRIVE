using UnityEngine;

/// <summary>
/// กล้องติดตามลูกกอล์ฟ - ซูมเข้าออก + หมุนมุมกล้องได้
/// Ball Camera Controller - Orbit camera with zoom, pan left/right, tilt up/down
/// </summary>
public class BallCameraController : MonoBehaviour
{
    [Header("--- Target ---")]
    [Tooltip("ลูกกอล์ฟที่กล้องจะติดตาม")]
    public Transform ball;

    [Header("--- Zoom Settings ---")]
    [Tooltip("ระยะห่างจากลูก (ปรับซูมด้วย Scroll)")]
    public float distance = 10f;
    
    [Tooltip("ระยะห่างต่ำสุด (ซูมเข้าสุด)")]
    public float minDistance = 3f;
    
    [Tooltip("ระยะห่างสูงสุด (ซูมออกสุด)")]
    public float maxDistance = 30f;
    
    [Tooltip("ความเร็วในการซูม")]
    public float zoomSpeed = 5f;

    [Header("--- Orbit Settings (หมุนมุมกล้อง) ---")]
    [Tooltip("มุมหมุนรอบลูก (ซ้าย-ขวา) องศา")]
    public float horizontalAngle = 0f;
    
    [Tooltip("มุมก้ม-เงย (บน-ล่าง) องศา")]
    public float verticalAngle = 30f;
    
    [Tooltip("มุม Vertical ต่ำสุด (ก้มลง)")]
    public float minVerticalAngle = 5f;
    
    [Tooltip("มุม Vertical สูงสุด (เงยขึ้น/มองจากบน)")]
    public float maxVerticalAngle = 80f;
    
    [Tooltip("ความเร็วในการหมุนกล้อง")]
    public float orbitSpeed = 100f;
    
    [Tooltip("ความเร็วหมุนด้วย Mouse (กดปุ่มกลางค้าง)")]
    public float mouseOrbitSpeed = 3f;

    [Header("--- Input Settings ---")]
    [Tooltip("ใช้ Arrow Keys หมุนกล้อง")]
    public bool useArrowKeys = true;
    
    [Tooltip("ใช้ WASD หมุนกล้อง (ถ้า false จะใช้ Arrow Keys เท่านั้น)")]
    public bool useWASD = false;
    
    [Tooltip("ใช้ Mouse กดปุ่มกลางค้างหมุนกล้อง")]
    public bool useMiddleMouse = true;
    
    [Tooltip("ใช้ Mouse กดขวาค้างหมุนกล้อง")]
    public bool useRightMouse = true;

    [Header("--- Smoothing ---")]
    [Tooltip("ความเร็วในการเคลื่อนที่ตามลูก (ยิ่งต่ำยิ่ง Smooth)")]
    public float followSpeed = 5f;
    
    [Tooltip("ความเร็วในการหมุนกล้อง (smooth orbit)")]
    public float orbitSmoothSpeed = 10f;

    [Header("--- Mode ---")]
    [Tooltip("ติดตามลูกตลอดเวลา หรือเฉพาะตอนลูกลอย")]
    public bool alwaysFollow = true;
    
    [Tooltip("ล็อคหมุนกล้องขณะลูกบิน")]
    public bool lockOrbitWhileFlying = false;

    // Private variables
    private Vector3 currentVelocity;
    private bool isFollowing = true;
    private float targetHorizontalAngle;
    private float targetVerticalAngle;
    private float currentHorizontalAngle;
    private float currentVerticalAngle;

    void Start()
    {
        // ถ้าไม่ได้กำหนด ball ให้หาอัตโนมัติ
        if (ball == null)
        {
            GolfBallController golfBall = FindFirstObjectByType<GolfBallController>();
            if (golfBall != null)
            {
                ball = golfBall.transform;
                Debug.Log("BallCameraController: Auto-assigned ball target");
            }
            else
            {
                Debug.LogError("BallCameraController: No ball found! Please assign a ball target.");
            }
        }

        // Initialize angles
        targetHorizontalAngle = horizontalAngle;
        targetVerticalAngle = verticalAngle;
        currentHorizontalAngle = horizontalAngle;
        currentVerticalAngle = verticalAngle;
    }

    void LateUpdate()
    {
        if (ball == null) return;

        // จัดการ Zoom ด้วย Mouse Scroll
        HandleZoom();
        
        // จัดการหมุนกล้อง
        HandleOrbitInput();

        // ติดตามลูก
        if (alwaysFollow || isFollowing)
        {
            FollowBall();
        }
    }

    void HandleZoom()
    {
        // อ่านค่า Scroll Wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (scrollInput != 0)
        {
            // ปรับระยะห่าง (Scroll ขึ้น = ซูมเข้า, Scroll ลง = ซูมออก)
            distance -= scrollInput * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void HandleOrbitInput()
    {
        // ถ้าล็อคขณะบิน และกำลังติดตาม ไม่ให้หมุน
        if (lockOrbitWhileFlying && isFollowing)
        {
            return;
        }

        float horizontalInput = 0f;
        float verticalInput = 0f;

        // Arrow Keys Input
        if (useArrowKeys)
        {
            if (Input.GetKey(KeyCode.LeftArrow)) horizontalInput -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) horizontalInput += 1f;
            if (Input.GetKey(KeyCode.UpArrow)) verticalInput += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) verticalInput -= 1f;
        }

        // WASD Input (optional)
        if (useWASD)
        {
            if (Input.GetKey(KeyCode.A)) horizontalInput -= 1f;
            if (Input.GetKey(KeyCode.D)) horizontalInput += 1f;
            if (Input.GetKey(KeyCode.W)) verticalInput += 1f;
            if (Input.GetKey(KeyCode.S)) verticalInput -= 1f;
        }

        // Mouse Input (กดปุ่มกลางหรือขวาค้างแล้วลาก)
        bool mouseOrbitActive = (useMiddleMouse && Input.GetMouseButton(2)) || 
                                (useRightMouse && Input.GetMouseButton(1));
        
        if (mouseOrbitActive)
        {
            horizontalInput += Input.GetAxis("Mouse X") * mouseOrbitSpeed;
            verticalInput -= Input.GetAxis("Mouse Y") * mouseOrbitSpeed;
        }

        // Apply input to target angles
        if (Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f)
        {
            targetHorizontalAngle += horizontalInput * orbitSpeed * Time.deltaTime;
            targetVerticalAngle += verticalInput * orbitSpeed * Time.deltaTime;
            
            // Clamp vertical angle
            targetVerticalAngle = Mathf.Clamp(targetVerticalAngle, minVerticalAngle, maxVerticalAngle);
            
            // Wrap horizontal angle
            if (targetHorizontalAngle > 360f) targetHorizontalAngle -= 360f;
            if (targetHorizontalAngle < 0f) targetHorizontalAngle += 360f;
        }

        // Smooth interpolation
        currentHorizontalAngle = Mathf.LerpAngle(currentHorizontalAngle, targetHorizontalAngle, orbitSmoothSpeed * Time.deltaTime);
        currentVerticalAngle = Mathf.Lerp(currentVerticalAngle, targetVerticalAngle, orbitSmoothSpeed * Time.deltaTime);
        
        // Update public values
        horizontalAngle = currentHorizontalAngle;
        verticalAngle = currentVerticalAngle;
    }

    void FollowBall()
    {
        // คำนวณตำแหน่งกล้องแบบ Orbit (โคจรรอบลูก)
        // ใช้ Spherical Coordinates: distance, horizontalAngle, verticalAngle
        
        float hRad = currentHorizontalAngle * Mathf.Deg2Rad;
        float vRad = currentVerticalAngle * Mathf.Deg2Rad;
        
        // คำนวณ offset จากมุม
        // x = distance * cos(vertical) * sin(horizontal)
        // y = distance * sin(vertical)
        // z = distance * cos(vertical) * cos(horizontal)
        Vector3 offset = new Vector3(
            distance * Mathf.Cos(vRad) * Mathf.Sin(hRad),
            distance * Mathf.Sin(vRad),
            -distance * Mathf.Cos(vRad) * Mathf.Cos(hRad)  // negative Z = ด้านหลังลูก
        );
        
        Vector3 targetPosition = ball.position + offset;

        // เคลื่อนที่แบบ Smooth
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            1f / followSpeed
        );

        // มองไปที่ลูก
        transform.LookAt(ball.position);
    }

    /// <summary>
    /// เรียกจาก GolfBallController เมื่อตีลูก
    /// </summary>
    public void StartFollowing()
    {
        isFollowing = true;
        Debug.Log($"📷 Camera: Start following");
    }

    /// <summary>
    /// เรียกเมื่อลูกหยุด
    /// </summary>
    public void StopFollowing()
    {
        isFollowing = false;
        Debug.Log("📷 Camera: Stop following");
    }

    /// <summary>
    /// รีเซ็ตมุมกล้องกลับค่าเริ่มต้น
    /// </summary>
    public void ResetOrbit()
    {
        targetHorizontalAngle = 0f;
        targetVerticalAngle = 30f;
    }

    /// <summary>
    /// ตั้งมุมกล้องโดยตรง
    /// </summary>
    public void SetOrbitAngles(float horizontal, float vertical)
    {
        targetHorizontalAngle = horizontal;
        targetVerticalAngle = Mathf.Clamp(vertical, minVerticalAngle, maxVerticalAngle);
    }

    /// <summary>
    /// หมุนกล้องสัมพัทธ์
    /// </summary>
    public void RotateOrbit(float deltaHorizontal, float deltaVertical)
    {
        targetHorizontalAngle += deltaHorizontal;
        targetVerticalAngle = Mathf.Clamp(targetVerticalAngle + deltaVertical, minVerticalAngle, maxVerticalAngle);
    }

    /// <summary>
    /// ตั้งค่าซูม
    /// </summary>
    public void SetZoom(float newDistance)
    {
        distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }

    /// <summary>
    /// ได้รับทิศที่กล้องกำลังมอง (สำหรับ aim)
    /// </summary>
    public Vector3 GetAimDirection()
    {
        Vector3 dir = ball.position - transform.position;
        dir.y = 0;
        return dir.normalized;
    }
}
