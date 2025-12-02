using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script สำหรับสร้าง Swing UI แบบ Pangya Style 3-Click อัตโนมัติ
/// วิธีใช้: ใน Unity Editor ไปที่ Menu → GRAND DRIVE → Create Swing UI (Pangya 3-Click)
/// </summary>
public class SwingUIGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GRAND DRIVE/Create Swing UI (Pangya 3-Click)")]
    public static void CreateSwingUI()
    {
        // ลบ UI เก่าก่อน
        RemoveSwingUI();

        // 1. สร้าง Canvas (ถ้ายังไม่มี)
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // 2. สร้าง SwingUI Container
        GameObject swingUIObj = new GameObject("SwingUI_Pangya3Click");
        swingUIObj.transform.SetParent(canvas.transform, false);
        RectTransform swingUIRect = swingUIObj.AddComponent<RectTransform>();
        swingUIRect.anchorMin = Vector2.zero;
        swingUIRect.anchorMax = Vector2.one;
        swingUIRect.offsetMin = Vector2.zero;
        swingUIRect.offsetMax = Vector2.zero;

        // 3. สร้าง Main Power Bar (แถบหลัก มี 0 ตรงกลาง)
        GameObject mainBar = CreateMainBar(swingUIObj.transform);

        // 4. สร้าง State/Hint Text
        GameObject stateText = CreateStateText(swingUIObj.transform);
        GameObject hintText = CreateHintText(swingUIObj.transform);

        // 5. สร้าง Result Panel
        GameObject resultPanel = CreateResultPanel(swingUIObj.transform);

        // 6. สร้าง Distance Display
        GameObject distanceDisplay = CreateDistanceDisplay(swingUIObj.transform);

        // 7. เพิ่ม SwingUI Component และเชื่อมต่อ
        SwingUI swingUI = swingUIObj.AddComponent<SwingUI>();
        
        // เชื่อมต่อ Main Bar
        swingUI.barBackground = mainBar.transform.Find("Background").GetComponent<RectTransform>();
        swingUI.marker = mainBar.transform.Find("Marker").GetComponent<RectTransform>();
        swingUI.centerLine = mainBar.transform.Find("CenterLine").GetComponent<RectTransform>();
        swingUI.perfectZone = mainBar.transform.Find("PerfectZone").GetComponent<RectTransform>();
        swingUI.powerFill = mainBar.transform.Find("PowerFill").GetComponent<Image>();
        swingUI.powerSelectedLine = mainBar.transform.Find("PowerSelectedLine").GetComponent<RectTransform>();
        
        // เชื่อมต่อ Text
        swingUI.stateText = stateText.GetComponent<TextMeshProUGUI>();
        swingUI.hintText = hintText.GetComponent<TextMeshProUGUI>();
        swingUI.currentDistanceText = distanceDisplay.transform.Find("CurrentDistance").GetComponent<TextMeshProUGUI>();
        swingUI.maxDistanceText = distanceDisplay.transform.Find("MaxDistance").GetComponent<TextMeshProUGUI>();
        
        // เชื่อมต่อ Result
        swingUI.resultPanel = resultPanel;
        swingUI.resultText = resultPanel.transform.Find("ResultText").GetComponent<TextMeshProUGUI>();

        // 8. สร้าง SwingSystem (ถ้ายังไม่มี)
        SwingSystem swingSystem = FindFirstObjectByType<SwingSystem>();
        if (swingSystem == null)
        {
            GameObject swingSystemObj = new GameObject("SwingSystem");
            swingSystem = swingSystemObj.AddComponent<SwingSystem>();
        }
        swingUI.swingSystem = swingSystem;

        // 9. เชื่อมต่อ GolfBallController (ถ้ามี)
        GolfBallController golfBall = FindFirstObjectByType<GolfBallController>();
        if (golfBall != null)
        {
            golfBall.swingSystem = swingSystem;
            golfBall.useSwingSystem = true;
            EditorUtility.SetDirty(golfBall);
        }

        EditorUtility.SetDirty(swingUIObj);
        Selection.activeGameObject = swingUIObj;
        
        Debug.Log("✅ Pangya 3-Click Swing UI Created!");
        Debug.Log("📝 วิธีเล่น: กด SPACE 3 ครั้ง (เริ่ม → เลือกระยะ → ตีใน Perfect Zone)");
    }

    /// <summary>
    /// สร้าง Main Bar - มี 0 ตรงกลาง, ขวา = Power, ซ้าย = Accuracy Zone
    /// </summary>
    static GameObject CreateMainBar(Transform parent)
    {
        GameObject container = new GameObject("MainBar");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0);
        containerRect.anchorMax = new Vector2(0.5f, 0);
        containerRect.pivot = new Vector2(0.5f, 0);
        containerRect.anchoredPosition = new Vector2(0, 50);
        containerRect.sizeDelta = new Vector2(800, 40);

        // Background (สีน้ำเงินเข้ม)
        GameObject background = new GameObject("Background");
        background.transform.SetParent(container.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.08f, 0.15f, 0.35f, 0.95f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Power Fill (ด้านขวาของ 0) - สีฟ้า
        GameObject powerFill = new GameObject("PowerFill");
        powerFill.transform.SetParent(container.transform, false);
        Image fillImage = powerFill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.6f, 1f, 0.8f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0; // จากซ้าย
        fillImage.fillAmount = 0;
        RectTransform fillRect = powerFill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0.5f, 0);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(0, 3);
        fillRect.offsetMax = new Vector2(-3, -3);

        // Center Line (เส้น 0 ตรงกลาง) - สีขาว
        GameObject centerLine = new GameObject("CenterLine");
        centerLine.transform.SetParent(container.transform, false);
        Image centerImg = centerLine.AddComponent<Image>();
        centerImg.color = Color.white;
        RectTransform centerRect = centerLine.GetComponent<RectTransform>();
        centerRect.anchorMin = new Vector2(0.5f, 0);
        centerRect.anchorMax = new Vector2(0.5f, 1);
        centerRect.pivot = new Vector2(0.5f, 0.5f);
        centerRect.anchoredPosition = Vector2.zero;
        centerRect.sizeDelta = new Vector2(4, 0);

        // Perfect Zone (ด้านซ้ายของ 0) - สีเหลือง
        GameObject perfectZone = new GameObject("PerfectZone");
        perfectZone.transform.SetParent(container.transform, false);
        Image pzImage = perfectZone.AddComponent<Image>();
        pzImage.color = new Color(1f, 0.9f, 0.2f, 0.7f);
        RectTransform pzRect = perfectZone.GetComponent<RectTransform>();
        pzRect.anchorMin = new Vector2(0.5f, 0);
        pzRect.anchorMax = new Vector2(0.5f, 1);
        pzRect.pivot = new Vector2(0.5f, 0.5f);
        // Perfect Zone อยู่ที่ -0.5 (25% จากซ้าย) ขนาด 15%
        float zoneCenter = -0.5f;
        float zoneSize = 0.15f;
        float halfBarWidth = 400f;
        pzRect.anchoredPosition = new Vector2(zoneCenter * halfBarWidth, 0);
        pzRect.sizeDelta = new Vector2(zoneSize * halfBarWidth, -6);

        // Power Selected Line (เส้นแสดงระยะที่เลือก)
        GameObject powerLine = new GameObject("PowerSelectedLine");
        powerLine.transform.SetParent(container.transform, false);
        Image lineImg = powerLine.AddComponent<Image>();
        lineImg.color = new Color(0f, 1f, 0.5f, 1f); // สีเขียว
        RectTransform lineRect = powerLine.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0);
        lineRect.anchorMax = new Vector2(0.5f, 1);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.sizeDelta = new Vector2(3, 0);
        powerLine.SetActive(false);

        // Tick Marks (เส้นแบ่งระยะ)
        for (int i = -4; i <= 4; i++)
        {
            if (i == 0) continue; // ข้าม 0 เพราะมี CenterLine แล้ว
            
            float pos = 0.5f + (i * 0.1f); // 0.1, 0.2, 0.3... หรือ 0.4, 0.3, 0.2...
            bool isMajor = i % 2 == 0;
            
            GameObject tick = new GameObject($"Tick_{i}");
            tick.transform.SetParent(container.transform, false);
            Image tickImg = tick.AddComponent<Image>();
            tickImg.color = new Color(0.5f, 0.6f, 0.7f, 0.6f);
            RectTransform tickRect = tick.GetComponent<RectTransform>();
            tickRect.anchorMin = new Vector2(pos, 0.2f);
            tickRect.anchorMax = new Vector2(pos, 0.8f);
            tickRect.pivot = new Vector2(0.5f, 0.5f);
            tickRect.anchoredPosition = Vector2.zero;
            tickRect.sizeDelta = new Vector2(isMajor ? 2 : 1, 0);
        }

        // Marker (ขีดสีขาวที่เคลื่อนที่)
        GameObject marker = new GameObject("Marker");
        marker.transform.SetParent(container.transform, false);
        Image markerImg = marker.AddComponent<Image>();
        markerImg.color = Color.white;
        RectTransform markerRect = marker.GetComponent<RectTransform>();
        markerRect.anchorMin = new Vector2(0.5f, 0);
        markerRect.anchorMax = new Vector2(0.5f, 1);
        markerRect.pivot = new Vector2(0.5f, 0.5f);
        markerRect.anchoredPosition = Vector2.zero;
        markerRect.sizeDelta = new Vector2(6, -4);

        // Distance Labels
        // 0y (กลาง)
        CreateDistanceLabel(container.transform, "0y", 0.5f);
        // Max (ขวาสุด)
        CreateDistanceLabel(container.transform, "230y", 0.95f);

        return container;
    }

    static void CreateDistanceLabel(Transform parent, string text, float anchorX)
    {
        GameObject label = new GameObject($"Label_{text}");
        label.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.7f, 0.8f, 0.9f);
        RectTransform rect = label.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(anchorX, 1);
        rect.anchorMax = new Vector2(anchorX, 1);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2(0, 5);
        rect.sizeDelta = new Vector2(60, 20);
    }

    static GameObject CreateDistanceDisplay(Transform parent)
    {
        GameObject container = new GameObject("DistanceDisplay");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0);
        containerRect.anchorMax = new Vector2(0.5f, 0);
        containerRect.pivot = new Vector2(0.5f, 0);
        containerRect.anchoredPosition = new Vector2(0, 100);
        containerRect.sizeDelta = new Vector2(300, 50);

        // Current Distance (ใหญ่ตรงกลาง)
        GameObject currentDist = new GameObject("CurrentDistance");
        currentDist.transform.SetParent(container.transform, false);
        TextMeshProUGUI currentTmp = currentDist.AddComponent<TextMeshProUGUI>();
        currentTmp.text = "0y";
        currentTmp.fontSize = 42;
        currentTmp.fontStyle = FontStyles.Bold;
        currentTmp.alignment = TextAlignmentOptions.Center;
        currentTmp.color = new Color(0.3f, 0.9f, 1f);
        RectTransform currentRect = currentDist.GetComponent<RectTransform>();
        currentRect.anchorMin = new Vector2(0, 0);
        currentRect.anchorMax = new Vector2(0.6f, 1);
        currentRect.offsetMin = Vector2.zero;
        currentRect.offsetMax = Vector2.zero;

        // Max Distance (เล็กด้านขวา)
        GameObject maxDist = new GameObject("MaxDistance");
        maxDist.transform.SetParent(container.transform, false);
        TextMeshProUGUI maxTmp = maxDist.AddComponent<TextMeshProUGUI>();
        maxTmp.text = "/ 230y";
        maxTmp.fontSize = 20;
        maxTmp.alignment = TextAlignmentOptions.Left;
        maxTmp.color = new Color(0.6f, 0.6f, 0.6f);
        RectTransform maxRect = maxDist.GetComponent<RectTransform>();
        maxRect.anchorMin = new Vector2(0.6f, 0.2f);
        maxRect.anchorMax = new Vector2(1, 0.8f);
        maxRect.offsetMin = Vector2.zero;
        maxRect.offsetMax = Vector2.zero;

        return container;
    }

    static GameObject CreateStateText(Transform parent)
    {
        GameObject obj = new GameObject("StateText");
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = "READY";
        tmp.fontSize = 28;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2(0, 160);
        rect.sizeDelta = new Vector2(300, 40);

        return obj;
    }

    static GameObject CreateHintText(Transform parent)
    {
        GameObject obj = new GameObject("HintText");
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Press SPACE to start";
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.7f, 0.7f, 0.7f);
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2(0, 200);
        rect.sizeDelta = new Vector2(400, 30);

        return obj;
    }

    static GameObject CreateResultPanel(Transform parent)
    {
        GameObject panel = new GameObject("ResultPanel");
        panel.transform.SetParent(parent, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.85f);
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(350, 180);

        GameObject resultText = new GameObject("ResultText");
        resultText.transform.SetParent(panel.transform, false);
        TextMeshProUGUI tmp = resultText.AddComponent<TextMeshProUGUI>();
        tmp.text = "RESULT";
        tmp.fontSize = 36;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        RectTransform textRect = resultText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -20);

        panel.SetActive(false);

        return panel;
    }

    [MenuItem("GRAND DRIVE/Remove Swing UI")]
    public static void RemoveSwingUI()
    {
        // ลบ SwingUI ทั้งหมด
        SwingUI[] swingUIs = FindObjectsByType<SwingUI>(FindObjectsSortMode.None);
        foreach (var ui in swingUIs)
        {
            DestroyImmediate(ui.gameObject);
        }
        
        // ไม่ลบ SwingSystem เพราะอาจต้องใช้
        Debug.Log("🗑️ Swing UI Removed!");
    }
#endif
}
