using UnityEngine;

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
    
    [Tooltip("ระยะห่างด้านหลังลูก")]
    public float behindOffset = 8f;

    [Header("--- Smoothing ---")]
    [Tooltip("ความเร็วในการเคลื่อนที่ตามลูก (ยิ่งต่ำยิ่ง Smooth)")]
    public float followSpeed = 5f;
    
    [Tooltip("ความเร็วในการหมุนกล้อง")]
    public float rotationSpeed = 5f;

    [Header("--- Mode ---")]
    [Tooltip("ติดตามลูกตลอดเวลา หรือเฉพาะตอนลูกลอย")]
    public bool alwaysFollow = true;

    private Vector3 currentVelocity;
    private bool isFollowing = true;

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
        // อยู่ด้านหลังและสูงกว่าลูก
        Vector3 targetPosition = ball.position 
            - ball.forward * behindOffset  // ด้านหลังลูก
            + Vector3.up * heightOffset;   // สูงกว่าลูก

        // ปรับระยะห่างตาม distance (Zoom)
        Vector3 directionToTarget = (targetPosition - ball.position).normalized;
        targetPosition = ball.position + directionToTarget * distance;

        // เคลื่อนที่แบบ Smooth
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            1f / followSpeed
        );

        // หมุนกล้องให้มองไปที่ลูกเสมอ
        Quaternion targetRotation = Quaternion.LookRotation(ball.position - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            rotationSpeed * Time.deltaTime
        );
    }

    // เรียกจาก GolfBallController เมื่อตีลูก
    public void StartFollowing()
    {
        isFollowing = true;
    }

    // เรียกเมื่อลูกหยุด
    public void StopFollowing()
    {
        isFollowing = false;
    }
}
