using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script สำหรับสร้าง Swing UI แบบ Pangya Style อัตโนมัติ
/// Auto-generate Pangya-style Swing UI in Unity Editor
/// วิธีใช้: ใน Unity Editor ไปที่ Menu → GRAND DRIVE → Create Swing UI (Pangya Style)
/// </summary>
public class SwingUIGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GRAND DRIVE/Create Swing UI (Pangya Style)")]
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
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // 2. สร้าง SwingUI Container
        GameObject swingUIObj = new GameObject("SwingUI_Pangya");
        swingUIObj.transform.SetParent(canvas.transform, false);
        RectTransform swingUIRect = swingUIObj.AddComponent<RectTransform>();
        swingUIRect.anchorMin = Vector2.zero;
        swingUIRect.anchorMax = Vector2.one;
        swingUIRect.offsetMin = Vector2.zero;
        swingUIRect.offsetMax = Vector2.zero;

        // 3. สร้าง Impact Circle (ซ้ายล่าง)
        GameObject impactCircle = CreateImpactCircle(swingUIObj.transform);

        // 4. สร้าง Power Bar (Pangya Style - ล่างกลาง)
        GameObject powerBar = CreatePangyaPowerBar(swingUIObj.transform);

        // 5. สร้าง Accuracy Bar (อยู่เหนือ Power Bar)
        GameObject accuracyBar = CreatePangyaAccuracyBar(swingUIObj.transform);

        // 6. สร้าง Club Info (ซ้ายบน)
        GameObject clubInfo = CreateClubInfo(swingUIObj.transform);

        // 7. สร้าง State/Hint Text
        GameObject stateText = CreateStateText(swingUIObj.transform);
        GameObject hintText = CreateHintText(swingUIObj.transform);

        // 8. สร้าง Result Panel
        GameObject resultPanel = CreateResultPanel(swingUIObj.transform);

        // 9. เพิ่ม SwingUI Component และเชื่อมต่อ
        SwingUI swingUI = swingUIObj.AddComponent<SwingUI>();
        
        // เชื่อมต่อ Impact Circle
        swingUI.impactCircleContainer = impactCircle.GetComponent<RectTransform>();
        swingUI.impactCircleBackground = impactCircle.transform.Find("CircleBG").GetComponent<Image>();
        swingUI.impactMarker = impactCircle.transform.Find("Marker").GetComponent<RectTransform>();
        swingUI.impactText = impactCircle.transform.Find("ImpactText").GetComponent<TextMeshProUGUI>();
        
        // เชื่อมต่อ Power Bar
        swingUI.powerBarContainer = powerBar.GetComponent<RectTransform>();
        swingUI.powerBarBackground = powerBar.transform.Find("Background").GetComponent<Image>();
        swingUI.powerBarFill = powerBar.transform.Find("Fill").GetComponent<Image>();
        swingUI.powerMarker = powerBar.transform.Find("Marker").GetComponent<RectTransform>();
        swingUI.currentDistanceText = powerBar.transform.Find("CurrentDistance").GetComponent<TextMeshProUGUI>();
        swingUI.halfDistanceText = powerBar.transform.Find("HalfDistance").GetComponent<TextMeshProUGUI>();
        swingUI.maxDistanceText = powerBar.transform.Find("MaxDistance").GetComponent<TextMeshProUGUI>();
        
        // เชื่อมต่อ Accuracy Bar
        swingUI.accuracyBarContainer = accuracyBar.GetComponent<RectTransform>();
        swingUI.accuracyBarBackground = accuracyBar.transform.Find("Background").GetComponent<Image>();
        swingUI.perfectZone = accuracyBar.transform.Find("PerfectZone").GetComponent<RectTransform>();
        swingUI.accuracyIndicator = accuracyBar.transform.Find("Indicator").GetComponent<RectTransform>();
        
        // เชื่อมต่อ Club Info
        swingUI.clubNameText = clubInfo.transform.Find("ClubName").GetComponent<TextMeshProUGUI>();
        
        // เชื่อมต่อ State/Hint
        swingUI.stateText = stateText.GetComponent<TextMeshProUGUI>();
        swingUI.hintText = hintText.GetComponent<TextMeshProUGUI>();
        
        // เชื่อมต่อ Result
        swingUI.resultPanel = resultPanel;
        swingUI.resultText = resultPanel.transform.Find("ResultText").GetComponent<TextMeshProUGUI>();

        // 10. สร้าง SwingSystem (ถ้ายังไม่มี)
        SwingSystem swingSystem = FindFirstObjectByType<SwingSystem>();
        if (swingSystem == null)
        {
            GameObject swingSystemObj = new GameObject("SwingSystem");
            swingSystem = swingSystemObj.AddComponent<SwingSystem>();
            swingSystem.maxDistance = 230f;
            swingSystem.powerBarSpeed = 1.2f;
            swingSystem.accuracyBarSpeed = 2.5f;
            swingSystem.perfectZoneSize = 0.08f;
        }
        swingUI.swingSystem = swingSystem;

        // 11. เชื่อมต่อ GolfBallController (ถ้ามี)
        GolfBallController golfBall = FindFirstObjectByType<GolfBallController>();
        if (golfBall != null)
        {
            golfBall.swingSystem = swingSystem;
            golfBall.useSwingSystem = true;
            EditorUtility.SetDirty(golfBall);
        }

        EditorUtility.SetDirty(swingUIObj);
        Selection.activeGameObject = swingUIObj;
        
        Debug.Log("✅ Pangya-Style Swing UI Created! / สร้าง Swing UI แบบ Pangya สำเร็จ!");
        Debug.Log("📝 กด Play แล้วกด Space เพื่อทดสอบ");
    }

    /// <summary>
    /// สร้าง Impact Circle (วงกลมซ้ายล่าง สำหรับเลือก Spin)
    /// </summary>
    static GameObject CreateImpactCircle(Transform parent)
    {
        GameObject container = new GameObject("ImpactCircle");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(0, 0);
        containerRect.pivot = new Vector2(0, 0);
        containerRect.anchoredPosition = new Vector2(30, 80);
        containerRect.sizeDelta = new Vector2(150, 150);

        // Circle Background (วงกลมเขียวเข้ม)
        GameObject circleBG = new GameObject("CircleBG");
        circleBG.transform.SetParent(container.transform, false);
        Image circleImg = circleBG.AddComponent<Image>();
        circleImg.color = new Color(0.1f, 0.3f, 0.2f, 0.9f);
        // ต้องใช้ Sprite วงกลม ถ้าไม่มีจะเป็นสี่เหลี่ยม
        RectTransform circleRect = circleBG.GetComponent<RectTransform>();
        circleRect.anchorMin = Vector2.zero;
        circleRect.anchorMax = Vector2.one;
        circleRect.offsetMin = Vector2.zero;
        circleRect.offsetMax = Vector2.zero;

        // Crosshair Lines (เส้นกากบาท)
        CreateCrosshairLine(container.transform, true);  // Horizontal
        CreateCrosshairLine(container.transform, false); // Vertical

        // Impact Marker (จุดแสดงตำแหน่ง)
        GameObject marker = new GameObject("Marker");
        marker.transform.SetParent(container.transform, false);
        Image markerImg = marker.AddComponent<Image>();
        markerImg.color = new Color(1f, 0.8f, 0.2f, 1f); // สีเหลือง
        RectTransform markerRect = marker.GetComponent<RectTransform>();
        markerRect.anchorMin = new Vector2(0.5f, 0.5f);
        markerRect.anchorMax = new Vector2(0.5f, 0.5f);
        markerRect.pivot = new Vector2(0.5f, 0.5f);
        markerRect.anchoredPosition = Vector2.zero;
        markerRect.sizeDelta = new Vector2(20, 20);

        // Impact Text
        GameObject impactText = new GameObject("ImpactText");
        impactText.transform.SetParent(container.transform, false);
        TextMeshProUGUI tmp = impactText.AddComponent<TextMeshProUGUI>();
        tmp.text = "Impact";
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform textRect = impactText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0);
        textRect.anchorMax = new Vector2(0.5f, 0);
        textRect.pivot = new Vector2(0.5f, 1);
        textRect.anchoredPosition = new Vector2(0, -5);
        textRect.sizeDelta = new Vector2(100, 20);

        return container;
    }

    static void CreateCrosshairLine(Transform parent, bool horizontal)
    {
        GameObject line = new GameObject(horizontal ? "HLine" : "VLine");
        line.transform.SetParent(parent, false);
        Image lineImg = line.AddComponent<Image>();
        lineImg.color = new Color(0.5f, 0.8f, 0.6f, 0.5f);
        RectTransform lineRect = line.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.sizeDelta = horizontal ? new Vector2(140, 2) : new Vector2(2, 140);
    }

    /// <summary>
    /// สร้าง Power Bar แบบ Pangya (แถบยาวด้านล่าง)
    /// </summary>
    static GameObject CreatePangyaPowerBar(Transform parent)
    {
        GameObject container = new GameObject("PowerBar");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0);
        containerRect.anchorMax = new Vector2(0.5f, 0);
        containerRect.pivot = new Vector2(0.5f, 0);
        containerRect.anchoredPosition = new Vector2(0, 30);
        containerRect.sizeDelta = new Vector2(700, 35);

        // Background (สีน้ำเงินเข้ม)
        GameObject background = new GameObject("Background");
        background.transform.SetParent(container.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.1f, 0.3f, 0.95f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Fill (สีฟ้า gradient effect)
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(container.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 0;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);

        // Tick Marks (เส้นแบ่ง)
        for (int i = 1; i < 10; i++)
        {
            CreateTickMark(container.transform, i / 10f, i == 5);
        }

        // Marker (ลูกศรชี้ตำแหน่งปัจจุบัน)
        GameObject marker = new GameObject("Marker");
        marker.transform.SetParent(container.transform, false);
        Image markerImg = marker.AddComponent<Image>();
        markerImg.color = Color.white;
        RectTransform markerRect = marker.GetComponent<RectTransform>();
        markerRect.anchorMin = new Vector2(0, 0.5f);
        markerRect.anchorMax = new Vector2(0, 0.5f);
        markerRect.pivot = new Vector2(0.5f, 0);
        markerRect.anchoredPosition = new Vector2(0, 20);
        markerRect.sizeDelta = new Vector2(8, 15);

        // Current Distance Text (ด้านซ้าย)
        GameObject currentDist = new GameObject("CurrentDistance");
        currentDist.transform.SetParent(container.transform, false);
        TextMeshProUGUI currentTmp = currentDist.AddComponent<TextMeshProUGUI>();
        currentTmp.text = "0y";
        currentTmp.fontSize = 24;
        currentTmp.fontStyle = FontStyles.Bold;
        currentTmp.alignment = TextAlignmentOptions.Left;
        currentTmp.color = Color.white;
        RectTransform currentRect = currentDist.GetComponent<RectTransform>();
        currentRect.anchorMin = new Vector2(0, 0.5f);
        currentRect.anchorMax = new Vector2(0, 0.5f);
        currentRect.pivot = new Vector2(1, 0.5f);
        currentRect.anchoredPosition = new Vector2(-10, 0);
        currentRect.sizeDelta = new Vector2(80, 30);

        // Half Distance Text (ตรงกลาง)
        GameObject halfDist = new GameObject("HalfDistance");
        halfDist.transform.SetParent(container.transform, false);
        TextMeshProUGUI halfTmp = halfDist.AddComponent<TextMeshProUGUI>();
        halfTmp.text = "115y";
        halfTmp.fontSize = 16;
        halfTmp.alignment = TextAlignmentOptions.Center;
        halfTmp.color = new Color(0.7f, 0.7f, 0.7f);
        RectTransform halfRect = halfDist.GetComponent<RectTransform>();
        halfRect.anchorMin = new Vector2(0.5f, 1);
        halfRect.anchorMax = new Vector2(0.5f, 1);
        halfRect.pivot = new Vector2(0.5f, 0);
        halfRect.anchoredPosition = new Vector2(0, 5);
        halfRect.sizeDelta = new Vector2(60, 20);

        // Max Distance Text (ด้านขวา)
        GameObject maxDist = new GameObject("MaxDistance");
        maxDist.transform.SetParent(container.transform, false);
        TextMeshProUGUI maxTmp = maxDist.AddComponent<TextMeshProUGUI>();
        maxTmp.text = "230y";
        maxTmp.fontSize = 20;
        maxTmp.fontStyle = FontStyles.Bold;
        maxTmp.alignment = TextAlignmentOptions.Right;
        maxTmp.color = Color.white;
        RectTransform maxRect = maxDist.GetComponent<RectTransform>();
        maxRect.anchorMin = new Vector2(1, 0.5f);
        maxRect.anchorMax = new Vector2(1, 0.5f);
        maxRect.pivot = new Vector2(0, 0.5f);
        maxRect.anchoredPosition = new Vector2(10, 0);
        maxRect.sizeDelta = new Vector2(80, 30);

        return container;
    }

    static void CreateTickMark(Transform parent, float position, bool isMajor)
    {
        GameObject tick = new GameObject($"Tick_{position:P0}");
        tick.transform.SetParent(parent, false);
        Image tickImg = tick.AddComponent<Image>();
        tickImg.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        RectTransform tickRect = tick.GetComponent<RectTransform>();
        tickRect.anchorMin = new Vector2(position, 0);
        tickRect.anchorMax = new Vector2(position, 1);
        tickRect.pivot = new Vector2(0.5f, 0.5f);
        tickRect.anchoredPosition = Vector2.zero;
        tickRect.sizeDelta = new Vector2(isMajor ? 3 : 1, 0);
    }

    /// <summary>
    /// สร้าง Accuracy Bar แบบ Pangya (แถบด้านบน Power Bar)
    /// </summary>
    static GameObject CreatePangyaAccuracyBar(Transform parent)
    {
        GameObject container = new GameObject("AccuracyBar");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0);
        containerRect.anchorMax = new Vector2(0.5f, 0);
        containerRect.pivot = new Vector2(0.5f, 0);
        containerRect.anchoredPosition = new Vector2(0, 75);
        containerRect.sizeDelta = new Vector2(700, 25);

        // Background (สีน้ำเงินเข้ม)
        GameObject background = new GameObject("Background");
        background.transform.SetParent(container.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.02f, 0.05f, 0.15f, 0.95f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Tick marks (จุดเล็กๆ)
        for (int i = 0; i <= 20; i++)
        {
            float pos = i / 20f;
            bool isMajor = i % 5 == 0;
            
            GameObject dot = new GameObject($"Dot_{i}");
            dot.transform.SetParent(container.transform, false);
            Image dotImg = dot.AddComponent<Image>();
            dotImg.color = new Color(0.3f, 0.5f, 0.3f, 0.8f);
            RectTransform dotRect = dot.GetComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(pos, 0.5f);
            dotRect.anchorMax = new Vector2(pos, 0.5f);
            dotRect.pivot = new Vector2(0.5f, 0.5f);
            dotRect.anchoredPosition = Vector2.zero;
            dotRect.sizeDelta = new Vector2(isMajor ? 6 : 4, isMajor ? 6 : 4);
        }

        // Perfect Zone (สีขาว ตรงกลาง)
        GameObject perfectZone = new GameObject("PerfectZone");
        perfectZone.transform.SetParent(container.transform, false);
        Image pzImage = perfectZone.AddComponent<Image>();
        pzImage.color = new Color(1f, 1f, 1f, 0.9f);
        RectTransform pzRect = perfectZone.GetComponent<RectTransform>();
        pzRect.anchorMin = new Vector2(0, 0);
        pzRect.anchorMax = new Vector2(0, 1);
        pzRect.pivot = new Vector2(0.5f, 0.5f);
        pzRect.anchoredPosition = new Vector2(350, 0); // กลาง
        pzRect.sizeDelta = new Vector2(56, 0); // 8% of 700

        // Indicator (ลูกศรสีเหลือง)
        GameObject indicator = new GameObject("Indicator");
        indicator.transform.SetParent(container.transform, false);
        
        // Arrow shape using multiple images
        Image indImg = indicator.AddComponent<Image>();
        indImg.color = new Color(1f, 0.9f, 0.2f, 1f);
        RectTransform indRect = indicator.GetComponent<RectTransform>();
        indRect.anchorMin = new Vector2(0, 0);
        indRect.anchorMax = new Vector2(0, 1);
        indRect.pivot = new Vector2(0.5f, 0.5f);
        indRect.anchoredPosition = new Vector2(0, 0);
        indRect.sizeDelta = new Vector2(6, 0);

        // ซ่อน Accuracy Bar ตอนเริ่ม
        container.SetActive(false);

        return container;
    }

    /// <summary>
    /// สร้าง Club Info (ซ้ายบน)
    /// </summary>
    static GameObject CreateClubInfo(Transform parent)
    {
        GameObject container = new GameObject("ClubInfo");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(0, 0);
        containerRect.pivot = new Vector2(0, 0);
        containerRect.anchoredPosition = new Vector2(30, 240);
        containerRect.sizeDelta = new Vector2(80, 50);

        // Background
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(container.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.2f, 0.1f, 0.8f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Club Name
        GameObject clubName = new GameObject("ClubName");
        clubName.transform.SetParent(container.transform, false);
        TextMeshProUGUI tmp = clubName.AddComponent<TextMeshProUGUI>();
        tmp.text = "1W";
        tmp.fontSize = 28;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform textRect = clubName.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return container;
    }

    static GameObject CreateStateText(Transform parent)
    {
        GameObject obj = new GameObject("StateText");
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = "READY";
        tmp.fontSize = 36;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2(0, 120);
        rect.sizeDelta = new Vector2(300, 50);

        return obj;
    }

    static GameObject CreateHintText(Transform parent)
    {
        GameObject obj = new GameObject("HintText");
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Press SPACE to start\nกด SPACE เพื่อเริ่ม";
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.8f, 0.8f, 0.8f);
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2(0, 170);
        rect.sizeDelta = new Vector2(400, 50);

        return obj;
    }

    static GameObject CreateResultPanel(Transform parent)
    {
        GameObject panel = new GameObject("ResultPanel");
        panel.transform.SetParent(parent, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
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
        SwingUI swingUI = FindFirstObjectByType<SwingUI>();
        if (swingUI != null)
        {
            DestroyImmediate(swingUI.gameObject);
            Debug.Log("🗑️ Swing UI Removed / ลบ Swing UI แล้ว");
        }
        
        SwingSystem swingSystem = FindFirstObjectByType<SwingSystem>();
        if (swingSystem != null)
        {
            DestroyImmediate(swingSystem.gameObject);
            Debug.Log("🗑️ SwingSystem Removed / ลบ SwingSystem แล้ว");
        }
    }
#endif
}
