using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI สำหรับแสดง Swing System แบบ Pangya Style
/// Pangya-style Swing UI with Power Bar, Accuracy Indicator, Impact Circle
/// </summary>
public class SwingUI : MonoBehaviour
{
    [Header("--- References ---")]
    [Tooltip("อ้างอิง SwingSystem")]
    public SwingSystem swingSystem;

    [Header("--- Power Bar UI (Pangya Style) ---")]
    [Tooltip("Container ของ Power Bar ทั้งหมด")]
    public RectTransform powerBarContainer;
    
    [Tooltip("Background ของ Power Bar")]
    public Image powerBarBackground;
    
    [Tooltip("Fill ของ Power Bar (สีฟ้า)")]
    public Image powerBarFill;
    
    [Tooltip("Marker แสดงตำแหน่ง Power ที่เลือก")]
    public RectTransform powerMarker;

    [Header("--- Accuracy Bar UI ---")]
    [Tooltip("Container ของ Accuracy Bar")]
    public RectTransform accuracyBarContainer;
    
    [Tooltip("Background ของ Accuracy Bar")]
    public Image accuracyBarBackground;
    
    [Tooltip("Perfect Zone (สีขาว/เหลือง ตรงกลาง)")]
    public RectTransform perfectZone;
    
    [Tooltip("Indicator ที่เคลื่อนที่ (ลูกศร/เส้น)")]
    public RectTransform accuracyIndicator;

    [Header("--- Distance Display ---")]
    [Tooltip("Text แสดงระยะปัจจุบัน")]
    public TextMeshProUGUI currentDistanceText;
    
    [Tooltip("Text แสดงระยะสูงสุด")]
    public TextMeshProUGUI maxDistanceText;
    
    [Tooltip("Text แสดงระยะครึ่งหนึ่ง")]
    public TextMeshProUGUI halfDistanceText;

    [Header("--- Club Info ---")]
    [Tooltip("Text แสดงชื่อไม้")]
    public TextMeshProUGUI clubNameText;
    
    [Tooltip("Icon ของไม้")]
    public Image clubIcon;

    [Header("--- Impact Circle (Spin Control) ---")]
    [Tooltip("Container ของ Impact Circle")]
    public RectTransform impactCircleContainer;
    
    [Tooltip("Background วงกลม")]
    public Image impactCircleBackground;
    
    [Tooltip("Crosshair/Marker แสดงจุด Impact")]
    public RectTransform impactMarker;
    
    [Tooltip("Text แสดง Impact")]
    public TextMeshProUGUI impactText;

    [Header("--- State UI ---")]
    [Tooltip("Text แสดงสถานะ")]
    public TextMeshProUGUI stateText;
    
    [Tooltip("Text แสดงคำแนะนำ")]
    public TextMeshProUGUI hintText;

    [Header("--- Result UI ---")]
    [Tooltip("Panel แสดงผลลัพธ์")]
    public GameObject resultPanel;
    
    [Tooltip("Text แสดงผลลัพธ์")]
    public TextMeshProUGUI resultText;

    [Header("--- Colors ---")]
    public Color powerBarColor = new Color(0.2f, 0.6f, 1f);      // ฟ้า
    public Color perfectZoneColor = new Color(1f, 1f, 1f, 0.9f); // ขาว
    public Color indicatorColor = Color.yellow;                    // เหลือง
    public Color goodColor = new Color(0.3f, 1f, 0.3f);           // เขียว
    public Color missColor = new Color(1f, 0.3f, 0.3f);           // แดง

    // Private
    private float powerBarWidth;
    private float accuracyBarWidth;
    private float impactCircleRadius;

    void Start()
    {
        // หา SwingSystem อัตโนมัติถ้าไม่ได้กำหนด
        if (swingSystem == null)
        {
            swingSystem = FindFirstObjectByType<SwingSystem>();
        }

        // คำนวณขนาด
        if (powerBarBackground != null)
        {
            powerBarWidth = powerBarBackground.rectTransform.rect.width;
        }
        if (accuracyBarBackground != null)
        {
            accuracyBarWidth = accuracyBarBackground.rectTransform.rect.width;
        }
        if (impactCircleBackground != null)
        {
            impactCircleRadius = impactCircleBackground.rectTransform.rect.width / 2f;
        }

        // Subscribe to events
        if (swingSystem != null)
        {
            swingSystem.OnStateChanged.AddListener(OnStateChanged);
            swingSystem.OnSwingComplete.AddListener(OnSwingComplete);
        }

        // ซ่อน Result Panel
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        // Initial state
        UpdateUI();
        OnStateChanged(SwingSystem.SwingState.Ready);
    }

    void Update()
    {
        UpdateUI();
    }

    /// <summary>
    /// อัปเดต UI ทุกเฟรม
    /// </summary>
    void UpdateUI()
    {
        if (swingSystem == null) return;

        UpdatePowerBar();
        UpdateAccuracyBar();
        UpdateDistanceText();
        UpdateHintText();
    }

    /// <summary>
    /// อัปเดต Power Bar แบบ Pangya
    /// </summary>
    void UpdatePowerBar()
    {
        if (powerBarFill != null)
        {
            powerBarFill.fillAmount = swingSystem.CurrentPower;
        }

        // อัปเดต Power Marker
        if (powerMarker != null && powerBarBackground != null)
        {
            float xPos = swingSystem.CurrentPower * powerBarWidth;
            powerMarker.anchoredPosition = new Vector2(xPos, powerMarker.anchoredPosition.y);
        }
    }

    /// <summary>
    /// อัปเดต Accuracy Bar
    /// </summary>
    void UpdateAccuracyBar()
    {
        if (swingSystem.CurrentState != SwingSystem.SwingState.AccuracyPhase &&
            swingSystem.CurrentState != SwingSystem.SwingState.Hitting)
        {
            return;
        }

        // อัปเดต Indicator position
        if (accuracyIndicator != null && accuracyBarBackground != null)
        {
            float xPos = swingSystem.CurrentAccuracy * accuracyBarWidth;
            accuracyIndicator.anchoredPosition = new Vector2(xPos, accuracyIndicator.anchoredPosition.y);
        }

        // อัปเดต Perfect Zone position (อยู่กลางเสมอในแบบ Pangya)
        if (perfectZone != null && accuracyBarBackground != null)
        {
            float zoneWidth = swingSystem.PerfectZoneSizeValue * accuracyBarWidth;
            float xPos = swingSystem.PerfectZoneCenter * accuracyBarWidth;
            
            perfectZone.sizeDelta = new Vector2(zoneWidth, perfectZone.sizeDelta.y);
            perfectZone.anchoredPosition = new Vector2(xPos, perfectZone.anchoredPosition.y);
        }
    }

    /// <summary>
    /// อัปเดต Distance Text
    /// </summary>
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

        if (halfDistanceText != null)
        {
            halfDistanceText.text = $"{swingSystem.MaxDistance / 2f:F0}y";
        }
    }

    /// <summary>
    /// อัปเดต Hint Text ตาม State
    /// </summary>
    void UpdateHintText()
    {
        if (hintText == null) return;

        switch (swingSystem.CurrentState)
        {
            case SwingSystem.SwingState.Ready:
                hintText.text = "Press SPACE to start\nกด SPACE เพื่อเริ่ม";
                break;
            case SwingSystem.SwingState.PowerPhase:
                hintText.text = "Press SPACE to set power!\nกด SPACE เพื่อกำหนดพลัง!";
                break;
            case SwingSystem.SwingState.AccuracyPhase:
                hintText.text = "Press SPACE in the white zone!\nกด SPACE ในโซนสีขาว!";
                break;
            case SwingSystem.SwingState.Hitting:
            case SwingSystem.SwingState.Cooldown:
                hintText.text = "";
                break;
        }
    }

    /// <summary>
    /// เรียกเมื่อ State เปลี่ยน
    /// </summary>
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
                    stateText.color = powerBarColor;
                    break;
                case SwingSystem.SwingState.AccuracyPhase:
                    stateText.text = "ACCURACY";
                    stateText.color = indicatorColor;
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

        // ซ่อน Result Panel เมื่อเริ่มใหม่
        if (newState == SwingSystem.SwingState.Ready && resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        // แสดง/ซ่อน Accuracy Bar
        if (accuracyBarContainer != null)
        {
            bool showAccuracy = newState == SwingSystem.SwingState.AccuracyPhase ||
                               newState == SwingSystem.SwingState.Hitting;
            accuracyBarContainer.gameObject.SetActive(showAccuracy);
        }

        // แสดง/ซ่อน Power Bar Container
        if (powerBarContainer != null)
        {
            bool showPower = newState != SwingSystem.SwingState.Cooldown;
            powerBarContainer.gameObject.SetActive(showPower);
        }
    }

    /// <summary>
    /// เรียกเมื่อตีลูกเสร็จ
    /// </summary>
    void OnSwingComplete(float power, float accuracy, bool isPerfect)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (resultText != null)
        {
            float distance = power * swingSystem.MaxDistance;
            
            if (isPerfect)
            {
                resultText.text = $"✨ SCH-WING! ✨\nPERFECT!\n{distance:F0}y";
                resultText.color = indicatorColor;
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
    /// อัปเดต Impact Circle (สำหรับ Spin)
    /// เรียกจากภายนอกเมื่อผู้เล่นเลื่อนจุด Impact
    /// </summary>
    public void UpdateImpactMarker(float horizontal, float vertical)
    {
        if (impactMarker != null && impactCircleBackground != null)
        {
            float x = horizontal * impactCircleRadius;
            float y = vertical * impactCircleRadius;
            impactMarker.anchoredPosition = new Vector2(x, y);
        }

        if (impactText != null)
        {
            string hText = horizontal > 0.1f ? "Slice" : horizontal < -0.1f ? "Hook" : "";
            string vText = vertical > 0.1f ? "Top" : vertical < -0.1f ? "Back" : "";
            impactText.text = $"{vText}{(string.IsNullOrEmpty(vText) || string.IsNullOrEmpty(hText) ? "" : " ")}{hText}";
        }
    }

    /// <summary>
    /// ตั้งค่าข้อมูลไม้กอล์ฟ
    /// </summary>
    public void SetClubInfo(string clubName, float maxDist, Sprite icon = null)
    {
        if (clubNameText != null)
        {
            clubNameText.text = clubName;
        }
        
        if (clubIcon != null && icon != null)
        {
            clubIcon.sprite = icon;
        }
        
        if (swingSystem != null)
        {
            swingSystem.SetMaxDistance(maxDist);
        }
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
