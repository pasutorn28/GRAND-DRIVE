using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script สำหรับสร้าง Swing UI อัตโนมัติ
/// Auto-generate Swing UI in Unity Editor
/// วิธีใช้: ใน Unity Editor ไปที่ Menu → GRAND DRIVE → Create Swing UI
/// </summary>
public class SwingUIGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GRAND DRIVE/Create Swing UI")]
    public static void CreateSwingUI()
    {
        // 1. สร้าง Canvas (ถ้ายังไม่มี)
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // ตั้งค่า CanvasScaler สำหรับ Mobile
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // 2. สร้าง SwingUI Container
        GameObject swingUIObj = new GameObject("SwingUI");
        swingUIObj.transform.SetParent(canvas.transform, false);
        RectTransform swingUIRect = swingUIObj.AddComponent<RectTransform>();
        swingUIRect.anchorMin = Vector2.zero;
        swingUIRect.anchorMax = Vector2.one;
        swingUIRect.offsetMin = Vector2.zero;
        swingUIRect.offsetMax = Vector2.zero;

        // 3. สร้าง Power Bar
        GameObject powerBarContainer = CreatePowerBar(swingUIObj.transform);

        // 4. สร้าง Accuracy Bar
        GameObject accuracyBarContainer = CreateAccuracyBar(swingUIObj.transform);

        // 5. สร้าง State Text
        GameObject stateTextObj = CreateStateText(swingUIObj.transform);

        // 6. สร้าง Hint Text
        GameObject hintTextObj = CreateHintText(swingUIObj.transform);

        // 7. สร้าง Result Panel
        GameObject resultPanelObj = CreateResultPanel(swingUIObj.transform);

        // 8. เพิ่ม SwingUI Component และเชื่อมต่อ
        SwingUI swingUI = swingUIObj.AddComponent<SwingUI>();
        
        // เชื่อมต่อ Power Bar
        swingUI.powerBarBackground = powerBarContainer.transform.Find("Background").GetComponent<Image>();
        swingUI.powerBarFill = powerBarContainer.transform.Find("Fill").GetComponent<Image>();
        swingUI.powerText = powerBarContainer.transform.Find("PowerText").GetComponent<TextMeshProUGUI>();
        
        // เชื่อมต่อ Accuracy Bar
        swingUI.accuracyBarBackground = accuracyBarContainer.transform.Find("Background").GetComponent<Image>();
        swingUI.accuracyIndicator = accuracyBarContainer.transform.Find("Indicator").GetComponent<RectTransform>();
        swingUI.perfectZone = accuracyBarContainer.transform.Find("PerfectZone").GetComponent<RectTransform>();
        swingUI.accuracyText = accuracyBarContainer.transform.Find("AccuracyText").GetComponent<TextMeshProUGUI>();
        
        // เชื่อมต่อ State/Hint
        swingUI.stateText = stateTextObj.GetComponent<TextMeshProUGUI>();
        swingUI.hintText = hintTextObj.GetComponent<TextMeshProUGUI>();
        
        // เชื่อมต่อ Result
        swingUI.resultPanel = resultPanelObj;
        swingUI.resultText = resultPanelObj.transform.Find("ResultText").GetComponent<TextMeshProUGUI>();

        // 9. สร้าง SwingSystem (ถ้ายังไม่มี)
        SwingSystem swingSystem = FindFirstObjectByType<SwingSystem>();
        if (swingSystem == null)
        {
            GameObject swingSystemObj = new GameObject("SwingSystem");
            swingSystem = swingSystemObj.AddComponent<SwingSystem>();
        }
        swingUI.swingSystem = swingSystem;

        // 10. เชื่อมต่อ GolfBallController (ถ้ามี)
        GolfBallController golfBall = FindFirstObjectByType<GolfBallController>();
        if (golfBall != null)
        {
            golfBall.swingSystem = swingSystem;
            golfBall.useSwingSystem = true;
            EditorUtility.SetDirty(golfBall);
        }

        // Mark as dirty for save
        EditorUtility.SetDirty(swingUIObj);
        
        // Select the created object
        Selection.activeGameObject = swingUIObj;
        
        Debug.Log("✅ Swing UI Created Successfully! / สร้าง Swing UI สำเร็จ!");
        Debug.Log("📝 กด Play แล้วกด Space เพื่อทดสอบ");
    }

    static GameObject CreatePowerBar(Transform parent)
    {
        // Container
        GameObject container = new GameObject("PowerBar");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0);
        containerRect.anchorMax = new Vector2(0.5f, 0);
        containerRect.pivot = new Vector2(0.5f, 0);
        containerRect.anchoredPosition = new Vector2(0, 100);
        containerRect.sizeDelta = new Vector2(600, 40);

        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(container.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(container.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.7f, 1f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 0;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);

        // Power Text
        GameObject powerText = new GameObject("PowerText");
        powerText.transform.SetParent(container.transform, false);
        TextMeshProUGUI tmp = powerText.AddComponent<TextMeshProUGUI>();
        tmp.text = "0%";
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform textRect = powerText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(container.transform, false);
        TextMeshProUGUI labelTmp = label.AddComponent<TextMeshProUGUI>();
        labelTmp.text = "POWER";
        labelTmp.fontSize = 18;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.color = Color.white;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 1);
        labelRect.anchorMax = new Vector2(0.5f, 1);
        labelRect.pivot = new Vector2(0.5f, 0);
        labelRect.anchoredPosition = new Vector2(0, 5);
        labelRect.sizeDelta = new Vector2(200, 25);

        return container;
    }

    static GameObject CreateAccuracyBar(Transform parent)
    {
        // Container
        GameObject container = new GameObject("AccuracyBar");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0);
        containerRect.anchorMax = new Vector2(0.5f, 0);
        containerRect.pivot = new Vector2(0.5f, 0);
        containerRect.anchoredPosition = new Vector2(0, 180);
        containerRect.sizeDelta = new Vector2(600, 30);

        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(container.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Perfect Zone (Gold)
        GameObject perfectZone = new GameObject("PerfectZone");
        perfectZone.transform.SetParent(container.transform, false);
        Image pzImage = perfectZone.AddComponent<Image>();
        pzImage.color = new Color(1f, 0.85f, 0f, 0.8f); // Gold
        RectTransform pzRect = perfectZone.GetComponent<RectTransform>();
        pzRect.anchorMin = new Vector2(0.5f, 0);
        pzRect.anchorMax = new Vector2(0.5f, 1);
        pzRect.pivot = new Vector2(0.5f, 0.5f);
        pzRect.anchoredPosition = Vector2.zero;
        pzRect.sizeDelta = new Vector2(90, 0); // 15% of 600

        // Indicator (White line)
        GameObject indicator = new GameObject("Indicator");
        indicator.transform.SetParent(container.transform, false);
        Image indImage = indicator.AddComponent<Image>();
        indImage.color = Color.white;
        RectTransform indRect = indicator.GetComponent<RectTransform>();
        indRect.anchorMin = new Vector2(0.5f, 0);
        indRect.anchorMax = new Vector2(0.5f, 1);
        indRect.pivot = new Vector2(0.5f, 0.5f);
        indRect.anchoredPosition = new Vector2(-300, 0); // Start at left
        indRect.sizeDelta = new Vector2(4, 0);

        // Accuracy Text
        GameObject accText = new GameObject("AccuracyText");
        accText.transform.SetParent(container.transform, false);
        TextMeshProUGUI tmp = accText.AddComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform textRect = accText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(1, 0.5f);
        textRect.anchorMax = new Vector2(1, 0.5f);
        textRect.pivot = new Vector2(0, 0.5f);
        textRect.anchoredPosition = new Vector2(10, 0);
        textRect.sizeDelta = new Vector2(60, 30);

        // Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(container.transform, false);
        TextMeshProUGUI labelTmp = label.AddComponent<TextMeshProUGUI>();
        labelTmp.text = "ACCURACY";
        labelTmp.fontSize = 16;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.color = Color.white;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 1);
        labelRect.anchorMax = new Vector2(0.5f, 1);
        labelRect.pivot = new Vector2(0.5f, 0);
        labelRect.anchoredPosition = new Vector2(0, 5);
        labelRect.sizeDelta = new Vector2(200, 20);

        // ซ่อน Accuracy Bar ตอนเริ่ม
        container.SetActive(false);

        return container;
    }

    static GameObject CreateStateText(Transform parent)
    {
        GameObject obj = new GameObject("StateText");
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = "READY";
        tmp.fontSize = 48;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.3f, 0.7f, 1f);
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2(0, 240);
        rect.sizeDelta = new Vector2(400, 60);

        return obj;
    }

    static GameObject CreateHintText(Transform parent)
    {
        GameObject obj = new GameObject("HintText");
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Press SPACE to start swing\nกด SPACE เพื่อเริ่มตี";
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.8f, 0.8f, 0.8f);
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2(0, 40);
        rect.sizeDelta = new Vector2(500, 50);

        return obj;
    }

    static GameObject CreateResultPanel(Transform parent)
    {
        // Panel Background
        GameObject panel = new GameObject("ResultPanel");
        panel.transform.SetParent(parent, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(400, 200);

        // Result Text
        GameObject resultText = new GameObject("ResultText");
        resultText.transform.SetParent(panel.transform, false);
        TextMeshProUGUI tmp = resultText.AddComponent<TextMeshProUGUI>();
        tmp.text = "RESULT";
        tmp.fontSize = 32;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        RectTransform textRect = resultText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -20);

        // ซ่อน Panel ตอนเริ่ม
        panel.SetActive(false);

        return panel;
    }

    [MenuItem("GRAND DRIVE/Remove Swing UI")]
    public static void RemoveSwingUI()
    {
        // ลบ SwingUI
        SwingUI swingUI = FindFirstObjectByType<SwingUI>();
        if (swingUI != null)
        {
            DestroyImmediate(swingUI.gameObject);
            Debug.Log("🗑️ Swing UI Removed / ลบ Swing UI แล้ว");
        }
        
        // ลบ SwingSystem
        SwingSystem swingSystem = FindFirstObjectByType<SwingSystem>();
        if (swingSystem != null)
        {
            DestroyImmediate(swingSystem.gameObject);
            Debug.Log("🗑️ SwingSystem Removed / ลบ SwingSystem แล้ว");
        }
    }
#endif
}
