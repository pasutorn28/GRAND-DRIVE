using UnityEngine;

public class MinimapSetup : MonoBehaviour
{
    private Camera minimapCam;
    private GolfBallController ball;

    void Start()
    {
        ball = FindFirstObjectByType<GolfBallController>();
        SetupMinimapCamera();
    }

    void SetupMinimapCamera()
    {
        // Create new camera object
        GameObject camObj = new GameObject("SideViewMinimapCamera");
        minimapCam = camObj.AddComponent<Camera>();

        // Settings for 2D Side View (Graph Style)
        minimapCam.orthographic = true;
        // Field is ~300m long. To fit 300m width in 16:9 aspect, we need Height = 300/1.77 = ~170
        // Ortho Size is Half Height. User requested 3x zoom out from 120 -> 360.
        minimapCam.orthographicSize = 360f; 
        minimapCam.clearFlags = CameraClearFlags.SolidColor;
        minimapCam.backgroundColor = new Color(0.05f, 0.05f, 0.1f, 1.0f); // Dark Blue for contrast
        
        // Position: Fixed Side View
        // Center of field (Z=150), Height (Y=50), Side (X=100)
        // Shift Z to 850 so Z=0 is on the left side (with Size 360)
        camObj.transform.position = new Vector3(100f, 50f, 850f); 
        camObj.transform.rotation = Quaternion.Euler(0, -90, 0); // Look Left (towards -X)

        // Viewport Rect: Top Right Corner
        // Widen to 40% width (0.4) and shift left to 0.6
        minimapCam.rect = new Rect(0.6f, 0.7f, 0.4f, 0.3f);
        
        // Ensure it renders after main camera
        minimapCam.depth = 10;
        
        // Culling Mask: Show everything (or specific layers if needed)
        // For now, default is fine.
    }

    // No Update/LateUpdate needed for static camera
}
