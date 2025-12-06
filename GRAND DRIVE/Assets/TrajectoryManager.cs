using UnityEngine;
using System.Collections.Generic;

public class TrajectoryManager : MonoBehaviour
{
    [Header("--- Settings ---")]
    public Material lineMaterial;
    public float lineWidth = 0.5f; // Increased width for better visibility
    public Color normalColor = Color.green;
    public Color spikeColor = Color.yellow;
    public Color tomahawkColor = Color.red;
    public Color cobraColor = Color.cyan;

    private List<LineRenderer> activeLines = new List<LineRenderer>();
    private LineRenderer currentLine;
    private GolfBallController ballController;
    private int pointCount = 0;

    void Start()
    {
        ballController = FindFirstObjectByType<GolfBallController>();
        
        // Create a default material if none provided
        if (lineMaterial == null)
        {
            // Force Sprites/Default for maximum visibility (ignores lighting, always on top)
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Hidden/Internal-Colored");
            
            lineMaterial = new Material(shader);
            Debug.Log($"TrajectoryManager: Created material with shader {shader.name}");
        }
    }

    void Update()
    {
        // Clear lines command
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllTrajectories();
        }

        // If ball is in air, update the current line
        if (ballController != null && ballController.IsInAir)
        {
            if (currentLine == null)
            {
                StartNewTrajectory();
            }
            
            UpdateTrajectory();
        }
        else
        {
            // Ball stopped, finish this line
            currentLine = null;
        }
    }

    void StartNewTrajectory()
    {
        GameObject lineObj = new GameObject($"Trajectory_{activeLines.Count}");
        lineObj.transform.SetParent(transform);
        
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        
        // Use Hidden/Internal-Colored for debug visibility
        Material mat = new Material(Shader.Find("Hidden/Internal-Colored"));
        lr.material = mat;
        
        lr.startWidth = 2.0f; // HUGE width for testing
        lr.endWidth = 2.0f;
        lr.positionCount = 0;
        lr.useWorldSpace = true;
        lr.sortingOrder = 100; 
        
        // Set color
        Color c = normalColor;
        if (ballController != null)
        {
            switch (ballController.currentShotType)
            {
                case SpecialShotType.Spike: c = spikeColor; break;
                case SpecialShotType.Tomahawk: c = tomahawkColor; break;
                case SpecialShotType.Cobra: c = cobraColor; break;
            }
        }
        
        lr.startColor = c;
        lr.endColor = c;
        lr.material.color = c; 

        currentLine = lr;
        activeLines.Add(lr);
        pointCount = 0;
        
        // Add start point immediately
        if (ballController != null)
        {
            AddPoint(ballController.transform.position);
        }
        
        Debug.Log($"Started new trajectory line. Color: {c}");
    }

    void UpdateTrajectory()
    {
        if (currentLine == null || ballController == null) return;

        // Only add point if moved enough (0.1m) to avoid clutter
        Vector3 currentPos = ballController.transform.position;
        if (pointCount == 0 || Vector3.Distance(currentLine.GetPosition(pointCount - 1), currentPos) > 0.1f)
        {
            AddPoint(currentPos);
        }
    }

    void AddPoint(Vector3 pos)
    {
        pointCount++;
        currentLine.positionCount = pointCount;
        currentLine.SetPosition(pointCount - 1, pos);
    }

    void OnDrawGizmos()
    {
        // Debug visualization in Scene view
        if (activeLines == null) return;
        
        foreach (var lr in activeLines)
        {
            if (lr == null || lr.positionCount < 2) continue;
            
            Gizmos.color = lr.startColor;
            for (int i = 0; i < lr.positionCount - 1; i++)
            {
                Gizmos.DrawLine(lr.GetPosition(i), lr.GetPosition(i + 1));
            }
        }
    }



    public void ClearAllTrajectories()
    {
        foreach (var lr in activeLines)
        {
            if (lr != null) Destroy(lr.gameObject);
        }
        activeLines.Clear();
        currentLine = null;
        Debug.Log("Trajectory Lines Cleared!");
    }
}
