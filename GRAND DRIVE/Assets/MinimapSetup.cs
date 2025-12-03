using UnityEngine;
using System.Collections.Generic;

public class MinimapSetup : MonoBehaviour
{
    [Header("--- Cameras ---")]
    private Camera wideCamera;      // ซ้าย - มุมกว้าง เห็นวิถีเต็ม
    private Camera followCamera;    // ขวา - ติดตามลูก
    
    private GolfBallController ball;
    
    [Header("--- Trajectory ---")]
    private LineRenderer fairwayLine;
    private LineRenderer trajectoryLine;
    private List<Vector3> trajectoryPoints = new List<Vector3>();
    private bool isTracking = false;
    private float trackInterval = 0.02f;
    private float lastTrackTime = 0f;
    private Vector3 startPosition;

    void Start()
    {
        ball = FindFirstObjectByType<GolfBallController>();
        startPosition = ball != null ? ball.transform.position : Vector3.zero;
        
        SetupWideCamera();      // ซ้าย - มุมกว้าง
        SetupFollowCamera();    // ขวา - ติดตามลูก
        CreateFairwayLine();
        CreateTrajectoryLine();
    }

    /// <summary>
    /// กล้องซ้าย - มุมกว้าง เห็นวิถีทั้งหมด
    /// </summary>
    void SetupWideCamera()
    {
        GameObject camObj = new GameObject("WideViewCamera");
        wideCamera = camObj.AddComponent<Camera>();

        wideCamera.orthographic = true;
        wideCamera.orthographicSize = 120f;  // มุมกว้าง
        wideCamera.clearFlags = CameraClearFlags.SolidColor;
        wideCamera.backgroundColor = new Color(0.02f, 0.02f, 0.08f, 1.0f);
        
        // มองจากด้านข้าง - เริ่มจากจุดตีซ้ายสุด
        camObj.transform.position = new Vector3(-100f, 50f, 100f); 
        camObj.transform.rotation = Quaternion.Euler(0, 90, 0);

        // Viewport: ซ้ายบน
        wideCamera.rect = new Rect(0.0f, 0.65f, 0.35f, 0.35f);
        
        wideCamera.depth = 10;
        wideCamera.nearClipPlane = 0.1f;
        wideCamera.farClipPlane = 1000f;
    }

    /// <summary>
    /// กล้องขวา - ติดตามลูกใกล้ๆ
    /// </summary>
    void SetupFollowCamera()
    {
        GameObject camObj = new GameObject("FollowViewCamera");
        followCamera = camObj.AddComponent<Camera>();

        followCamera.orthographic = true;
        followCamera.orthographicSize = 30f;  // ซูมเข้าใกล้
        followCamera.clearFlags = CameraClearFlags.SolidColor;
        followCamera.backgroundColor = new Color(0.02f, 0.05f, 0.02f, 1.0f);
        
        // มองจากด้านข้าง
        camObj.transform.position = new Vector3(-50f, 30f, 0f); 
        camObj.transform.rotation = Quaternion.Euler(0, 90, 0);

        // Viewport: ขวาบน
        followCamera.rect = new Rect(0.65f, 0.65f, 0.35f, 0.35f);
        
        followCamera.depth = 11;
        followCamera.nearClipPlane = 0.1f;
        followCamera.farClipPlane = 1000f;
    }

    void CreateFairwayLine()
    {
        GameObject obj = new GameObject("FairwayLine");
        fairwayLine = obj.AddComponent<LineRenderer>();
        
        fairwayLine.positionCount = 2;
        fairwayLine.SetPosition(0, new Vector3(0, 0.2f, -20f));
        fairwayLine.SetPosition(1, new Vector3(0, 0.2f, 500f));
        
        fairwayLine.startColor = Color.white;
        fairwayLine.endColor = new Color(1f, 1f, 1f, 0.3f);
        fairwayLine.startWidth = 3f;
        fairwayLine.endWidth = 3f;
        
        fairwayLine.material = new Material(Shader.Find("Sprites/Default"));
        fairwayLine.useWorldSpace = true;
    }

    void CreateTrajectoryLine()
    {
        GameObject obj = new GameObject("TrajectoryLine");
        trajectoryLine = obj.AddComponent<LineRenderer>();
        
        trajectoryLine.positionCount = 0;
        trajectoryLine.startColor = Color.green;
        trajectoryLine.endColor = Color.yellow;
        trajectoryLine.startWidth = 2f;
        trajectoryLine.endWidth = 2f;
        
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.useWorldSpace = true;
    }

    void Update()
    {
        if (ball == null) return;
        
        // เริ่มติดตามเมื่อลูกถูกตี
        if (ball.IsInAir && !isTracking)
        {
            StartTracking();
        }
        
        // หยุดติดตามเมื่อลูกหยุด
        if (!ball.IsInAir && isTracking)
        {
            isTracking = false;
            Debug.Log($"📊 Trajectory: {trajectoryPoints.Count} points, Max height: {GetMaxHeight():F1}m");
        }
        
        // บันทึกตำแหน่งลูกระหว่างบิน
        if (isTracking && Time.time - lastTrackTime > trackInterval)
        {
            trajectoryPoints.Add(ball.transform.position);
            UpdateTrajectoryLine();
            lastTrackTime = Time.time;
        }
        
        // กด R = Reset trajectory
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearTrajectory();
            startPosition = ball.transform.position;
        }
    }

    void StartTracking()
    {
        isTracking = true;
        ClearTrajectory();
        startPosition = ball.transform.position;
        trajectoryPoints.Add(ball.transform.position);
        lastTrackTime = Time.time;
    }

    void ClearTrajectory()
    {
        trajectoryPoints.Clear();
        if (trajectoryLine != null)
        {
            trajectoryLine.positionCount = 0;
        }
    }

    void UpdateTrajectoryLine()
    {
        if (trajectoryLine == null || trajectoryPoints.Count < 2) return;
        
        trajectoryLine.positionCount = trajectoryPoints.Count;
        trajectoryLine.SetPositions(trajectoryPoints.ToArray());
    }

    float GetMaxHeight()
    {
        float max = 0f;
        foreach (var p in trajectoryPoints)
        {
            if (p.y > max) max = p.y;
        }
        return max;
    }

    void LateUpdate()
    {
        if (ball == null) return;
        
        // === กล้องซ้าย (Wide) - ปรับให้เห็นวิถีทั้งหมด ===
        if (wideCamera != null && trajectoryPoints.Count > 0)
        {
            Vector3 minPos = startPosition;
            Vector3 maxPos = startPosition;
            
            foreach (var point in trajectoryPoints)
            {
                minPos = Vector3.Min(minPos, point);
                maxPos = Vector3.Max(maxPos, point);
            }
            
            if (ball.IsInAir)
            {
                maxPos = Vector3.Max(maxPos, ball.transform.position);
            }
            
            // กล้องอยู่กลางระหว่าง start และ max
            float centerZ = (startPosition.z + maxPos.z) / 2f;
            float centerY = (minPos.y + maxPos.y) / 2f + 20f;
            
            // ขนาดต้องครอบคลุมทั้งหมด + padding
            float rangeZ = Mathf.Max(maxPos.z - startPosition.z + 80f, 150f);
            float rangeY = maxPos.y - minPos.y + 60f;
            float requiredSize = Mathf.Max(rangeZ / 2f, rangeY, 80f);
            
            Vector3 camPos = wideCamera.transform.position;
            camPos.z = centerZ;
            camPos.y = Mathf.Max(centerY, 50f);
            wideCamera.transform.position = camPos;
            wideCamera.orthographicSize = Mathf.Lerp(wideCamera.orthographicSize, requiredSize, Time.deltaTime * 2f);
        }
        
        // === กล้องขวา (Follow) - ติดตามลูกใกล้ๆ ===
        if (followCamera != null)
        {
            Vector3 camPos = followCamera.transform.position;
            camPos.z = ball.transform.position.z;
            camPos.y = ball.transform.position.y + 10f;
            followCamera.transform.position = Vector3.Lerp(followCamera.transform.position, camPos, Time.deltaTime * 8f);
        }
    }
}
