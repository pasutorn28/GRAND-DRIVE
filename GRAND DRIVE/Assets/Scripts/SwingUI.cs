using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI สำหรับแสดง Swing System
/// UI for displaying the Swing System (Power Bar, Accuracy Bar)
/// </summary>
public class SwingUI : MonoBehaviour
{
    [Header("--- References ---")]
    [Tooltip("อ้างอิง SwingSystem")]
    public SwingSystem swingSystem;

    [Header("--- Power Bar UI ---")]
    [Tooltip("Background ของ Power Bar")]
    public Image powerBarBackground;
    
    [Tooltip("Fill ของ Power Bar")]
    public Image powerBarFill;
    
    [Tooltip("Text แสดงค่า Power")]
    public TextMeshProUGUI powerText;

    [Header("--- Accuracy Bar UI ---")]
    [Tooltip("Background ของ Accuracy Bar")]
    public Image accuracyBarBackground;
    
    [Tooltip("Indicator ที่เคลื่อนที่")]
    public RectTransform accuracyIndicator;
    
    [Tooltip("Perfect Zone")]
    public RectTransform perfectZone;
    
    [Tooltip("Text แสดงค่า Accuracy")]
    public TextMeshProUGUI accuracyText;

    [Header("--- State UI ---")]
    [Tooltip("Text แสดงสถานะปัจจุบัน")]
    public TextMeshProUGUI stateText;
    
    [Tooltip("Text แสดงคำแนะนำ")]
    public TextMeshProUGUI hintText;

    [Header("--- Result UI ---")]
    [Tooltip("Panel แสดงผลลัพธ์")]
    public GameObject resultPanel;
    
    [Tooltip("Text แสดงผลลัพธ์")]
    public TextMeshProUGUI resultText;

    [Header("--- Colors ---")]
    public Color normalColor = new Color(0.3f, 0.7f, 1f);      // ฟ้า
    public Color perfectColor = new Color(1f, 0.85f, 0f);       // ทอง
    public Color goodColor = new Color(0.3f, 1f, 0.3f);         // เขียว
    public Color missColor = new Color(1f, 0.3f, 0.3f);         // แดง

    // Private
    private float accuracyBarWidth;

    void Start()
    {
        // หา SwingSystem อัตโนมัติถ้าไม่ได้กำหนด
        if (swingSystem == null)
        {
            swingSystem = FindFirstObjectByType<SwingSystem>();
        }

        // คำนวณความกว้างของ Accuracy Bar
        if (accuracyBarBackground != null)
        {
            accuracyBarWidth = accuracyBarBackground.rectTransform.rect.width;
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
    }

    void Update()
    {
        UpdateUI();
    }

    /// <summary>
    /// อัปเดต UI ทุกเฟรม
    /// Update UI every frame
    /// </summary>
    void UpdateUI()
    {
        if (swingSystem == null) return;

        // อัปเดต Power Bar
        UpdatePowerBar();

        // อัปเดต Accuracy Bar
        UpdateAccuracyBar();

        // อัปเดต Hint Text
        UpdateHintText();
    }

    /// <summary>
    /// อัปเดต Power Bar UI
    /// </summary>
    void UpdatePowerBar()
    {
        if (powerBarFill != null)
        {
            powerBarFill.fillAmount = swingSystem.CurrentPower;
            
            // เปลี่ยนสีตามพลัง
            if (swingSystem.CurrentPower >= 0.9f)
                powerBarFill.color = perfectColor;
            else if (swingSystem.CurrentPower >= 0.5f)
                powerBarFill.color = goodColor;
            else
                powerBarFill.color = normalColor;
        }

        if (powerText != null)
        {
            powerText.text = $"{swingSystem.CurrentPower:P0}";
        }
    }

    /// <summary>
    /// อัปเดต Accuracy Bar UI
    /// </summary>
    void UpdateAccuracyBar()
    {
        // อัปเดต Indicator position
        if (accuracyIndicator != null && accuracyBarBackground != null)
        {
            float xPos = (swingSystem.CurrentAccuracy - 0.5f) * accuracyBarWidth;
            accuracyIndicator.anchoredPosition = new Vector2(xPos, accuracyIndicator.anchoredPosition.y);
        }

        // อัปเดต Perfect Zone position และขนาด
        if (perfectZone != null && accuracyBarBackground != null)
        {
            float zoneWidth = swingSystem.PerfectZoneSize * accuracyBarWidth;
            float xPos = (swingSystem.PerfectZoneCenter - 0.5f) * accuracyBarWidth;
            
            perfectZone.sizeDelta = new Vector2(zoneWidth, perfectZone.sizeDelta.y);
            perfectZone.anchoredPosition = new Vector2(xPos, perfectZone.anchoredPosition.y);
        }

        if (accuracyText != null)
        {
            accuracyText.text = $"{swingSystem.CurrentAccuracy:P0}";
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
                hintText.text = "Press SPACE to start swing\nกด SPACE เพื่อเริ่มตี";
                break;
            case SwingSystem.SwingState.PowerPhase:
                hintText.text = "Press SPACE to set power!\nกด SPACE เพื่อกำหนดพลัง!";
                break;
            case SwingSystem.SwingState.AccuracyPhase:
                hintText.text = "Press SPACE in the GOLD zone!\nกด SPACE ในโซนสีทอง!";
                break;
            case SwingSystem.SwingState.Hitting:
            case SwingSystem.SwingState.Cooldown:
                hintText.text = "Wait for ball to stop...\nรอลูกหยุด...";
                break;
        }
    }

    /// <summary>
    /// เรียกเมื่อ State เปลี่ยน
    /// Called when swing state changes
    /// </summary>
    void OnStateChanged(SwingSystem.SwingState newState)
    {
        if (stateText != null)
        {
            switch (newState)
            {
                case SwingSystem.SwingState.Ready:
                    stateText.text = "READY";
                    stateText.color = normalColor;
                    break;
                case SwingSystem.SwingState.PowerPhase:
                    stateText.text = "POWER";
                    stateText.color = goodColor;
                    break;
                case SwingSystem.SwingState.AccuracyPhase:
                    stateText.text = "ACCURACY";
                    stateText.color = perfectColor;
                    break;
                case SwingSystem.SwingState.Hitting:
                    stateText.text = "SWING!";
                    stateText.color = perfectColor;
                    break;
                case SwingSystem.SwingState.Cooldown:
                    stateText.text = "WAIT...";
                    stateText.color = Color.gray;
                    break;
            }
        }

        // ซ่อน Result Panel เมื่อเริ่มใหม่
        if (newState == SwingSystem.SwingState.Ready && resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        // แสดง/ซ่อน Accuracy Bar ตาม Phase
        if (accuracyBarBackground != null)
        {
            accuracyBarBackground.gameObject.SetActive(
                newState == SwingSystem.SwingState.AccuracyPhase ||
                newState == SwingSystem.SwingState.Hitting
            );
        }
    }

    /// <summary>
    /// เรียกเมื่อตีลูกเสร็จ
    /// Called when swing is complete
    /// </summary>
    void OnSwingComplete(float power, float accuracy, bool isPerfect)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (resultText != null)
        {
            if (isPerfect)
            {
                resultText.text = $"✨ SCH-WING! ✨\nPERFECT IMPACT!\n\nPower: {power:P0}\nAccuracy: {accuracy:P0}";
                resultText.color = perfectColor;
            }
            else if (accuracy >= 0.8f)
            {
                resultText.text = $"Good Shot!\n\nPower: {power:P0}\nAccuracy: {accuracy:P0}";
                resultText.color = goodColor;
            }
            else if (accuracy >= 0.5f)
            {
                resultText.text = $"OK Shot\n\nPower: {power:P0}\nAccuracy: {accuracy:P0}";
                resultText.color = normalColor;
            }
            else
            {
                resultText.text = $"Miss...\n\nPower: {power:P0}\nAccuracy: {accuracy:P0}";
                resultText.color = missColor;
            }
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (swingSystem != null)
        {
            swingSystem.OnStateChanged.RemoveListener(OnStateChanged);
            swingSystem.OnSwingComplete.RemoveListener(OnSwingComplete);
        }
    }
}
