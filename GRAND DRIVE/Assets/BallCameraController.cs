using UnityEngine;

/// <summary>
/// กล้องติดตามลูกกอล์ฟ - เคลื่อนที่ตามแต่ไม่หมุนตามลูก
/// Ball Camera Controller - Follow ball position but don't rotate with ball
/// </summary>
public class BallCameraController : MonoBehaviour
{
    [Header("--- Target ---")]
    [Tooltip("ลูกกอล์ฟที่กล้องจะติดตาม")]
    public Transform ball;

    [Header("--- Camera Settings ---")]
    [Tooltip("ระยะห่างจากลูก (ปรับซูมด้วย Scroll)")]
    public float distance = 10f;
    
    [Tooltip("ระยะห่างต่ำสุด (ซูมเข้าสุด)")]
    public float minDistance = 3f;
    
    [Tooltip("ระยะห่างสูงสุด (ซูมออกสุด)")]
    public float maxDistance = 30f;
    
    [Tooltip("ความเร็วในการซูม")]
    public float zoomSpeed = 5f;

    [Header("--- Position Offset ---")]
    [Tooltip("ความสูงของกล้องเหนือลูก")]
    public float heightOffset = 5f;
    
    [Tooltip("ระยะห่างด้านหลังลูก (ใช้ทิศตอนตี ไม่ใช่ทิศของลูก)")]
    public float behindOffset = 8f;

    [Header("--- Smoothing ---")]
    [Tooltip("ความเร็วในการเคลื่อนที่ตามลูก (ยิ่งต่ำยิ่ง Smooth)")]
    public float followSpeed = 5f;
    
    [Tooltip("ความเร็วในการหมุนกล้องมอง (smooth look at)")]
    public float lookAtSpeed = 3f;

    [Header("--- Mode ---")]
    [Tooltip("ติดตามลูกตลอดเวลา หรือเฉพาะตอนลูกลอย")]
    public bool alwaysFollow = true;

    // Private variables
    private Vector3 currentVelocity;
    private bool isFollowing = true;
    private Vector3 initialShotDirection;  // ทิศทางตอนตี (จำไว้)
    private Vector3 fixedCameraOffset;     // offset ที่คำนวณตอนเริ่มตี
    private bool hasFixedOffset = false;

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

        // เก็บทิศทางเริ่มต้น (ใช้ทิศหน้าของกล้องปัจจุบัน)
        initialShotDirection = transform.forward;
        initialShotDirection.y = 0;
        initialShotDirection.Normalize();
        
        if (initialShotDirection.magnitude < 0.1f)
        {
            initialShotDirection = Vector3.forward;
        }
    }

    void LateUpdate()
    {
        if (ball == null) return;

        // จัดการ Zoom ด้วย Mouse Scroll
        HandleZoom();

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

    void FollowBall()
    {
        // คำนวณตำแหน่งเป้าหมายของกล้อง
        // ใช้ทิศทางตอนตี (initialShotDirection) แทน ball.forward
        // เพราะลูกกอล์ฟจะหมุนไปเรื่อยๆ แต่กล้องไม่ควรหมุนตาม
        
        Vector3 targetPosition;
        
        if (hasFixedOffset)
        {
            // ใช้ offset ที่คำนวณไว้ตอนเริ่มตี
            targetPosition = ball.position + fixedCameraOffset;
        }
        else
        {
            // คำนวณ offset จากทิศเริ่มต้น
            targetPosition = ball.position 
                - initialShotDirection * behindOffset  // ด้านหลังตามทิศเริ่มต้น (ไม่ใช่ ball.forward)
                + Vector3.up * heightOffset;           // สูงกว่าลูก
        }

        // เคลื่อนที่แบบ Smooth
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            1f / followSpeed
        );

        // มองไปที่ลูกแบบ Smooth (ไม่หมุนตามลูก แค่มองไปที่ตำแหน่งลูก)
        Vector3 lookDirection = ball.position - transform.position;
        if (lookDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                lookAtSpeed * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// เรียกจาก GolfBallController เมื่อตีลูก
    /// จะจำทิศทางตอนตีไว้
    /// </summary>
    public void StartFollowing()
    {
        isFollowing = true;
        
        // จำทิศทางตอนตี (จากทิศหน้าของลูกตอนนั้น หรือจากทิศกล้องปัจจุบัน)
        if (ball != null)
        {
            // ใช้ทิศที่กล้องกำลังมอง (ไม่ใช่ทิศของลูก)
            initialShotDirection = transform.forward;
            initialShotDirection.y = 0;
            initialShotDirection.Normalize();
            
            if (initialShotDirection.magnitude < 0.1f)
            {
                initialShotDirection = Vector3.forward;
            }
            
            // คำนวณ fixed offset
            fixedCameraOffset = -initialShotDirection * behindOffset + Vector3.up * heightOffset;
            hasFixedOffset = true;
        }
        
        Debug.Log($"📷 Camera: Start following, direction = {initialShotDirection}");
    }

    /// <summary>
    /// เรียกเมื่อลูกหยุด
    /// </summary>
    public void StopFollowing()
    {
        isFollowing = false;
        hasFixedOffset = false;
        Debug.Log("📷 Camera: Stop following");
    }

    /// <summary>
    /// ตั้งค่าทิศทางกล้องใหม่ (เช่น เมื่อผู้เล่นหมุน aim)
    /// </summary>
    public void SetAimDirection(Vector3 direction)
    {
        direction.y = 0;
        if (direction.magnitude > 0.1f)
        {
            initialShotDirection = direction.normalized;
        }
    }

    /// <summary>
    /// หมุนทิศทางกล้อง (สำหรับ aim)
    /// </summary>
    public void RotateAim(float angle)
    {
        initialShotDirection = Quaternion.Euler(0, angle, 0) * initialShotDirection;
    }
}
