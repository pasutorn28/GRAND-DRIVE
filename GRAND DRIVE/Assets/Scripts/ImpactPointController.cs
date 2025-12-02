using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ระบบ Impact Point - ลาก cursor บน Impact Circle เพื่อกำหนด spin
/// Impact Point System - Drag on Impact Circle to set Hook/Slice/Top/Back spin
/// </summary>
public class ImpactPointController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("--- References ---")]
    [Tooltip("อ้างอิง GolfBallController")]
    public GolfBallController ballController;
    
    [Tooltip("อ้างอิง SwingUI")]
    public SwingUI swingUI;

    [Header("--- UI Elements ---")]
    [Tooltip("วงกลม Impact (Image ที่รับ Raycast)")]
    public RectTransform impactCircle;
    
    [Tooltip("Marker แสดงจุด Impact")]
    public RectTransform impactMarker;
    
    [Tooltip("Crosshair เส้นแนวนอน")]
    public RectTransform horizontalLine;
    
    [Tooltip("Crosshair เส้นแนวตั้ง")]
    public RectTransform verticalLine;

    [Header("--- Impact Settings ---")]
    [Tooltip("จำกัดการเคลื่อนที่ภายในวงกลม")]
    public bool constrainToCircle = true;
    
    [Tooltip("ความไวในการลาก")]
    public float dragSensitivity = 1f;
    
    [Tooltip("Snap กลับกลางเมื่อปล่อย")]
    public bool snapToCenter = false;
    
    [Tooltip("ความเร็วในการ Snap กลับ")]
    public float snapSpeed = 5f;

    [Header("--- Keyboard/Controller Support ---")]
    [Tooltip("ใช้ Arrow Keys / D-Pad เลื่อน Impact Point")]
    public bool useArrowKeys = true;
    
    [Tooltip("ความเร็วเมื่อใช้ Arrow Keys")]
    public float arrowKeySpeed = 1f;

    [Header("--- Output Values (-1 to 1) ---")]
    [SerializeField] private float impactX = 0f;  // -1 (Hook) to 1 (Slice)
    [SerializeField] private float impactY = 0f;  // -1 (Back) to 1 (Top)

    [Header("--- Events ---")]
    public UnityEvent<float, float> OnImpactChanged;

    // Private
    private bool isDragging = false;
    private float circleRadius;
    private Vector2 targetPosition;
    private Canvas parentCanvas;
    private Camera uiCamera;

    // Properties
    public float ImpactX => impactX;
    public float ImpactY => impactY;
    public bool IsDragging => isDragging;

    void Start()
    {
        // หา Components อัตโนมัติ
        if (ballController == null)
            ballController = FindFirstObjectByType<GolfBallController>();
        
        if (swingUI == null)
            swingUI = FindFirstObjectByType<SwingUI>();

        // คำนวณ radius
        if (impactCircle != null)
        {
            circleRadius = impactCircle.rect.width / 2f;
        }

        // หา Canvas และ Camera
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = parentCanvas.worldCamera;
        }

        // เริ่มต้นที่กลาง
        ResetToCenter();
    }

    void Update()
    {
        // Arrow Keys / Controller support
        if (useArrowKeys && !isDragging)
        {
            HandleArrowKeyInput();
        }

        // Snap back to center
        if (snapToCenter && !isDragging)
        {
            impactX = Mathf.Lerp(impactX, 0f, snapSpeed * Time.deltaTime);
            impactY = Mathf.Lerp(impactY, 0f, snapSpeed * Time.deltaTime);
            UpdateVisuals();
        }
    }

    /// <summary>
    /// จัดการ Arrow Key Input
    /// </summary>
    void HandleArrowKeyInput()
    {
        float h = 0f, v = 0f;

        // Keyboard
        if (Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.UpArrow)) v += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v -= 1f;

        // D-Pad (ถ้ามี Controller)
        h += Input.GetAxis("Horizontal");
        v += Input.GetAxis("Vertical");

        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            impactX += h * arrowKeySpeed * Time.deltaTime;
            impactY += v * arrowKeySpeed * Time.deltaTime;
            
            ClampImpactValues();
            UpdateVisuals();
            ApplyImpact();
        }
    }

    /// <summary>
    /// เมื่อกดลงบน Impact Circle
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        UpdateImpactFromPointer(eventData);
    }

    /// <summary>
    /// เมื่อลาก
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            UpdateImpactFromPointer(eventData);
        }
    }

    /// <summary>
    /// เมื่อปล่อย
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    /// <summary>
    /// คำนวณตำแหน่งจาก Pointer
    /// </summary>
    void UpdateImpactFromPointer(PointerEventData eventData)
    {
        if (impactCircle == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            impactCircle, eventData.position, uiCamera, out localPoint);

        // แปลงเป็น normalized (-1 to 1)
        impactX = (localPoint.x / circleRadius) * dragSensitivity;
        impactY = (localPoint.y / circleRadius) * dragSensitivity;

        ClampImpactValues();
        UpdateVisuals();
        ApplyImpact();
    }

    /// <summary>
    /// จำกัดค่า Impact ให้อยู่ในขอบเขต
    /// </summary>
    void ClampImpactValues()
    {
        if (constrainToCircle)
        {
            // จำกัดให้อยู่ในวงกลม
            Vector2 impact = new Vector2(impactX, impactY);
            if (impact.magnitude > 1f)
            {
                impact = impact.normalized;
                impactX = impact.x;
                impactY = impact.y;
            }
        }
        else
        {
            // จำกัดแบบสี่เหลี่ยม
            impactX = Mathf.Clamp(impactX, -1f, 1f);
            impactY = Mathf.Clamp(impactY, -1f, 1f);
        }
    }

    /// <summary>
    /// อัปเดต Visual elements
    /// </summary>
    void UpdateVisuals()
    {
        // อัปเดต Marker position
        if (impactMarker != null && impactCircle != null)
        {
            float x = impactX * circleRadius;
            float y = impactY * circleRadius;
            impactMarker.anchoredPosition = new Vector2(x, y);
        }

        // อัปเดต Crosshair
        if (horizontalLine != null)
        {
            horizontalLine.anchoredPosition = new Vector2(0, impactY * circleRadius);
        }
        if (verticalLine != null)
        {
            verticalLine.anchoredPosition = new Vector2(impactX * circleRadius, 0);
        }

        // อัปเดต SwingUI
        if (swingUI != null)
        {
            swingUI.UpdateImpactMarker(impactX, impactY);
        }
    }

    /// <summary>
    /// ส่งค่า Impact ไปยัง Ball Controller
    /// </summary>
    void ApplyImpact()
    {
        if (ballController != null)
        {
            ballController.impactHorizontal = impactX;
            ballController.impactVertical = impactY;
        }

        OnImpactChanged?.Invoke(impactX, impactY);
    }

    /// <summary>
    /// รีเซ็ตกลับกลาง
    /// </summary>
    public void ResetToCenter()
    {
        impactX = 0f;
        impactY = 0f;
        UpdateVisuals();
        ApplyImpact();
    }

    /// <summary>
    /// ตั้งค่า Impact โดยตรง
    /// </summary>
    public void SetImpact(float x, float y)
    {
        impactX = x;
        impactY = y;
        ClampImpactValues();
        UpdateVisuals();
        ApplyImpact();
    }

    /// <summary>
    /// ได้รับ Spin Info เป็น Text
    /// </summary>
    public string GetSpinInfoText()
    {
        string h = impactX > 0.1f ? "Slice" : impactX < -0.1f ? "Hook" : "";
        string v = impactY > 0.1f ? "Top" : impactY < -0.1f ? "Back" : "";
        
        if (string.IsNullOrEmpty(h) && string.IsNullOrEmpty(v))
            return "Center";
        
        return $"{v}{(string.IsNullOrEmpty(v) || string.IsNullOrEmpty(h) ? "" : " ")}{h}".Trim();
    }
}
