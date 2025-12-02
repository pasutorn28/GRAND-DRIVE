using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI สำหรับแสดง Swing System แบบ Pangya Style - 3 Click System
/// แสดง Power Bar แนวนอนพร้อม Perfect Zone
/// </summary>
public class SwingUI : MonoBehaviour
{
    [Header("--- References ---")]
    public SwingSystem swingSystem;

    [Header("--- Main Bar UI ---")]
    [Tooltip("Background ของ Bar ทั้งหมด")]
    public RectTransform barBackground;
    
    [Tooltip("ขีดสีขาวที่เคลื่อนที่")]
    public RectTransform marker;
    
    [Tooltip("เส้นแบ่งตรงกลาง (จุด 0)")]
    public RectTransform centerLine;
    
    [Tooltip("Perfect Zone (ด้านซ้ายของ 0)")]
    public RectTransform perfectZone;

    [Header("--- Power Indicator ---")]
    [Tooltip("แสดงระยะที่เลือก (Fill)")]
    public Image powerFill;
    
    [Tooltip("เส้นแสดงระยะที่เลือกแล้ว")]
    public RectTransform powerSelectedLine;

    [Header("--- Distance Display ---")]
    public TextMeshProUGUI currentDistanceText;
    public TextMeshProUGUI maxDistanceText;

    [Header("--- State UI ---")]
    public TextMeshProUGUI stateText;
    public TextMeshProUGUI hintText;

    [Header("--- Result UI ---")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    [Header("--- Colors ---")]
    public Color markerColor = Color.white;
    public Color perfectZoneColor = new Color(1f, 1f, 0f, 0.5f);
    public Color powerFillColor = new Color(0.2f, 0.6f, 1f, 0.8f);
    public Color goodColor = new Color(0.3f, 1f, 0.3f);
    public Color missColor = new Color(1f, 0.3f, 0.3f);

    // Private
    private float barWidth;
    private float barHalfWidth;

    void Start()
    {
        if (swingSystem == null)
            swingSystem = FindFirstObjectByType<SwingSystem>();

        if (barBackground != null)
        {
            barWidth = barBackground.rect.width;
            barHalfWidth = barWidth / 2f;
        }

        if (swingSystem != null)
        {
            swingSystem.OnStateChanged.AddListener(OnStateChanged);
            swingSystem.OnSwingComplete.AddListener(OnSwingComplete);
        }

        if (resultPanel != null)
            resultPanel.SetActive(false);

        UpdateUI();
        OnStateChanged(SwingSystem.SwingState.Ready);
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (swingSystem == null) return;

        UpdateMarker();
        UpdatePerfectZone();
        UpdatePowerFill();
        UpdateDistanceText();
        UpdateHintText();
    }

    /// <summary>
    /// อัปเดตตำแหน่งขีด (Marker)
    /// MarkerPosition: -1 (ซ้ายสุด) ถึง 1 (ขวาสุด)
    /// </summary>
    void UpdateMarker()
    {
        if (marker == null || barBackground == null) return;

        // markerPosition: -1 = ซ้ายสุด, 0 = กลาง, 1 = ขวาสุด
        float pos = swingSystem.MarkerPosition;
        float xPos = pos * barHalfWidth;

        marker.anchoredPosition = new Vector2(xPos, marker.anchoredPosition.y);
    }

    /// <summary>
    /// อัปเดต Perfect Zone (ด้านซ้ายของ 0)
    /// </summary>
    void UpdatePerfectZone()
    {
        if (perfectZone == null || barBackground == null) return;

        float zoneCenter = swingSystem.PerfectZoneCenter; // ค่าติดลบ
        float zoneSize = swingSystem.PerfectZoneSizeValue;

        // แปลงเป็น pixel position
        float xPos = zoneCenter * barHalfWidth;
        float width = zoneSize * barHalfWidth;

        perfectZone.anchoredPosition = new Vector2(xPos, perfectZone.anchoredPosition.y);
        perfectZone.sizeDelta = new Vector2(width, perfectZone.sizeDelta.y);
    }

    /// <summary>
    /// อัปเดต Power Fill
    /// </summary>
    void UpdatePowerFill()
    {
        if (powerFill != null)
        {
            powerFill.fillAmount = swingSystem.SelectedPower * 0.5f; // 0.5 = ครึ่งบาร์ (ด้านขวา)
        }

        if (powerSelectedLine != null)
        {
            if (swingSystem.SelectedPower > 0)
            {
                powerSelectedLine.gameObject.SetActive(true);
                float xPos = swingSystem.SelectedPower * barHalfWidth;
                powerSelectedLine.anchoredPosition = new Vector2(xPos, powerSelectedLine.anchoredPosition.y);
            }
            else
            {
                powerSelectedLine.gameObject.SetActive(false);
            }
        }
    }

    void UpdateDistanceText()
    {
        if (currentDistanceText != null)
        {
            float dist = swingSystem.CurrentDistance;
            currentDistanceText.text = $"{dist:F0}y";
        }

        if (maxDistanceText != null)
        {
            maxDistanceText.text = $"{swingSystem.MaxDistance:F0}y";
        }
    }

    void UpdateHintText()
    {
        if (hintText == null) return;

        switch (swingSystem.CurrentState)
        {
            case SwingSystem.SwingState.Ready:
                hintText.text = "Press SPACE to start";
                break;
            case SwingSystem.SwingState.PowerPhase:
                hintText.text = "Press SPACE to set distance!";
                break;
            case SwingSystem.SwingState.AccuracyPhase:
                hintText.text = "Press SPACE in the Perfect Zone!";
                break;
            case SwingSystem.SwingState.Hitting:
            case SwingSystem.SwingState.Cooldown:
                hintText.text = "";
                break;
        }
    }

    void OnStateChanged(SwingSystem.SwingState newState)
    {
        if (stateText != null)
        {
            switch (newState)
            {
                case SwingSystem.SwingState.Ready:
                    stateText.text = "READY";
                    stateText.color = Color.white;
                    break;
                case SwingSystem.SwingState.PowerPhase:
                    stateText.text = "POWER";
                    stateText.color = powerFillColor;
                    break;
                case SwingSystem.SwingState.AccuracyPhase:
                    stateText.text = "ACCURACY";
                    stateText.color = perfectZoneColor;
                    break;
                case SwingSystem.SwingState.Hitting:
                    stateText.text = "SWING!";
                    stateText.color = goodColor;
                    break;
                case SwingSystem.SwingState.Cooldown:
                    stateText.text = "";
                    break;
            }
        }

        if (newState == SwingSystem.SwingState.Ready && resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    void OnSwingComplete(float power, float accuracy, bool isPerfect)
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            float distance = power * swingSystem.MaxDistance;

            if (isPerfect)
            {
                // SCH-WING! ไม่ใช่ PANGYA!
                resultText.text = $"SCH-WING!\nPERFECT!\n{distance:F0}y";
                resultText.color = Color.yellow;
            }
            else if (accuracy >= 0.8f)
            {
                resultText.text = $"Good!\n{distance:F0}y";
                resultText.color = goodColor;
            }
            else if (accuracy >= 0.5f)
            {
                resultText.text = $"OK\n{distance:F0}y";
                resultText.color = Color.white;
            }
            else
            {
                resultText.text = $"Miss...\n{distance:F0}y";
                resultText.color = missColor;
            }
        }
    }

    /// <summary>
    /// อัปเดต Impact Marker (Legacy - for compatibility)
    /// </summary>
    public void UpdateImpactMarker(float horizontal, float vertical)
    {
        // Kept for compatibility with ImpactPointController
    }

    public void SetClubInfo(string clubName, float maxDist, Sprite icon = null)
    {
        if (swingSystem != null)
            swingSystem.SetMaxDistance(maxDist);
    }

    void OnDestroy()
    {
        if (swingSystem != null)
        {
            swingSystem.OnStateChanged.RemoveListener(OnStateChanged);
            swingSystem.OnSwingComplete.RemoveListener(OnSwingComplete);
        }
    }
}
