using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// ระบบ Input แบบ New Input System
/// New Input System Integration - Support Keyboard/Mouse, Gamepad, Touch
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class GolfInputController : MonoBehaviour
{
    [Header("--- References ---")]
    public SwingSystem swingSystem;
    public ImpactPointController impactController;
    public SpecialShotSystem specialShotSystem;
    public BallCameraController cameraController;
    public GolfBallController ballController;
    public ClubSystem clubSystem; // Added reference

    // ... (Existing code) ...

    // ... (Existing code) ...

    [Header("--- Input Settings ---")]
    [Tooltip("ความไวในการเลื่อน Impact Point")]
    public float impactMoveSensitivity = 2f;
    
    [Tooltip("ความไวในการหมุนกล้อง")]
    public float cameraSensitivity = 0.5f;

    [Header("--- Events ---")]
    public UnityEvent OnSwingPressed;
    public UnityEvent OnResetPressed;
    public UnityEvent<Vector2> OnAimChanged;
    public UnityEvent<int> OnSpecialShotSelected;

    // Private
    private PlayerInput playerInput;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool swingPressed;
    private bool resetPressed;

    // Input Actions
    private InputAction swingAction;
    private InputAction resetAction;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction specialShot1Action;
    private InputAction specialShot2Action;
    private InputAction specialShot3Action;
    private InputAction specialShot4Action;
    private InputAction nextClubAction;
    private InputAction prevClubAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        SetupInputActions();
    }

    void Start()
    {
        // Auto-find references
        if (swingSystem == null)
            swingSystem = FindFirstObjectByType<SwingSystem>();
        
        if (impactController == null)
            impactController = FindFirstObjectByType<ImpactPointController>();
        
        if (specialShotSystem == null)
            specialShotSystem = FindFirstObjectByType<SpecialShotSystem>();
        
        if (cameraController == null)
            cameraController = FindFirstObjectByType<BallCameraController>();
        
        if (ballController == null)
            ballController = FindFirstObjectByType<GolfBallController>();

        if (clubSystem == null)
            clubSystem = FindFirstObjectByType<ClubSystem>();
    }

    void SetupInputActions()
    {
        if (playerInput == null) return;

        // ดึง Actions จาก Player map
        var playerMap = playerInput.actions.FindActionMap("Player");
        if (playerMap != null)
        {
            // Swing = Jump action (Space / A button)
            swingAction = playerMap.FindAction("Jump");
            
            // Reset = Interact action (R / Y button)
            resetAction = playerMap.FindAction("Interact");
            
            // Move = Move action (WASD / Left Stick)
            moveAction = playerMap.FindAction("Move");
            
            // Look = Look action (Mouse / Right Stick)
            lookAction = playerMap.FindAction("Look");
            
            // Previous/Next = สำหรับเปลี่ยนไม้
            prevClubAction = playerMap.FindAction("Previous");
            nextClubAction = playerMap.FindAction("Next");
        }

        // Subscribe to actions
        if (swingAction != null)
        {
            swingAction.performed += OnSwingPerformed;
        }
        
        if (resetAction != null)
        {
            resetAction.performed += OnResetPerformed;
        }
    }

    void Update()
    {
        // อ่านค่า Input ต่อเนื่อง
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
            HandleImpactPointInput(moveInput);
        }

        if (lookAction != null)
        {
            lookInput = lookAction.ReadValue<Vector2>();
            HandleCameraInput(lookInput);
        }

        // Special Shot selection ด้วย Number keys
        HandleSpecialShotInput();

        // Club Switching (Q/E)
        HandleClubInput();
    }

    void HandleClubInput()
    {
        if (clubSystem == null) return;

        // Use Q/E for Club Switching as requested
        // Q = Previous Club
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            clubSystem.PrevClub();
        }
        // E = Next Club
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            clubSystem.NextClub();
        }
    }

    /// <summary>
    /// เมื่อกดปุ่ม Swing (Space / A button)
    /// </summary>
    void OnSwingPerformed(InputAction.CallbackContext context)
    {
        if (swingSystem == null) return;

        switch (swingSystem.CurrentState)
        {
            case SwingSystem.SwingState.Ready:
            case SwingSystem.SwingState.PowerPhase:
            case SwingSystem.SwingState.AccuracyPhase:
                // ส่ง Input ไปยัง SwingSystem
                // (SwingSystem จะจัดการ state machine เอง)
                OnSwingPressed?.Invoke();
                Debug.Log("🎮 Swing Input Received");
                break;
        }
    }

    /// <summary>
    /// เมื่อกดปุ่ม Reset (R / Y button)
    /// </summary>
    void OnResetPerformed(InputAction.CallbackContext context)
    {
        if (swingSystem != null)
        {
            swingSystem.ResetSwing();
        }
        
        if (impactController != null)
        {
            impactController.ResetToCenter();
        }
        
        OnResetPressed?.Invoke();
        Debug.Log("🔄 Reset Input Received");
    }

    /// <summary>
    /// จัดการ Impact Point Input
    /// </summary>
    void HandleImpactPointInput(Vector2 input)
    {
        if (impactController == null) return;
        
        // ใช้ Move Input เลื่อน Impact Point (เมื่อไม่ได้ลาก Mouse)
        if (!impactController.IsDragging && input.magnitude > 0.1f)
        {
            float currentX = impactController.ImpactX;
            float currentY = impactController.ImpactY;
            
            float newX = currentX + input.x * impactMoveSensitivity * Time.deltaTime;
            float newY = currentY + input.y * impactMoveSensitivity * Time.deltaTime;
            
            impactController.SetImpact(newX, newY);
        }
    }

    /// <summary>
    /// จัดการ Camera Input
    /// </summary>
    void HandleCameraInput(Vector2 input)
    {
        // สามารถใช้ Look input เพื่อหมุนกล้องได้
        // (implement ใน BallCameraController)
        if (input.magnitude > 0.1f)
        {
            OnAimChanged?.Invoke(input * cameraSensitivity);
        }
    }

    /// <summary>
    /// จัดการ Special Shot Input
    /// </summary>
    void HandleSpecialShotInput()
    {
        // ใช้ Keyboard shortcuts แยก (ไม่ผ่าน Input System actions)
        // เพราะ Input System ไม่มี 1-4 ใน default map
        
        if (specialShotSystem == null) return;

        // Number keys สำหรับ Special Shots
        // (ใช้ Input.GetKeyDown เพราะ Input System default ไม่มี)
        // Note: ถ้าต้องการใช้ผ่าน Input System ต้องเพิ่ม Actions ใน .inputactions file
    }

    /// <summary>
    /// เรียกจากภายนอกเพื่อเลือก Special Shot
    /// </summary>
    public void SelectSpecialShot(int shotIndex)
    {
        if (specialShotSystem != null)
        {
            specialShotSystem.SelectShot((SpecialShotType)shotIndex);
            OnSpecialShotSelected?.Invoke(shotIndex);
        }
    }

    /// <summary>
    /// เปลี่ยนไม้ถัดไป/ก่อนหน้า
    /// </summary>
    public void ChangeClub(int direction)
    {
        // TODO: Implement club changing
        Debug.Log($"Club change: {(direction > 0 ? "Next" : "Previous")}");
    }

    void OnEnable()
    {
        swingAction?.Enable();
        resetAction?.Enable();
        moveAction?.Enable();
        lookAction?.Enable();
    }

    void OnDisable()
    {
        swingAction?.Disable();
        resetAction?.Disable();
        moveAction?.Disable();
        lookAction?.Disable();
    }

    void OnDestroy()
    {
        if (swingAction != null)
            swingAction.performed -= OnSwingPerformed;
        
        if (resetAction != null)
            resetAction.performed -= OnResetPerformed;
    }
}
