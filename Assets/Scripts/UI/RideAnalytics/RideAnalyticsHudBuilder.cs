using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RideAnalyticsHudBuilder : MonoBehaviour
{
    [SerializeField] private bool buildOnAwake = true;
    [SerializeField] private RideAnalyticsManager analyticsManager;
    [SerializeField] private bool showAdvancedCenterStats = false;

    private static readonly Color White = Color.white;
    private static readonly Color Pink = new Color(0.92f, 0.02f, 0.28f);
    private static readonly Color Blue = new Color(0.09f, 0.27f, 0.66f);
    private static readonly Color Yellow = new Color(1f, 0.92f, 0.02f);
    private static readonly Color MenuBlue = new Color(0.22f, 0.55f, 0.82f, 0.95f);
    private static readonly Color ProgressTrack = new Color(1f, 1f, 1f, 0.25f);
    private static readonly Color ProgressFill = new Color(0.95f, 0.95f, 0.12f, 0.95f);
    private static Sprite circleSprite;

    private void Awake()
    {
        if (buildOnAwake && transform.childCount == 0)
            Build(gameObject, analyticsManager);
    }

    [ContextMenu("Rebuild Ride Analytics HUD")]
    public void Rebuild()
    {
        ClearChildren(transform);
        Build(gameObject, analyticsManager);
    }

    public static RideAnalyticsDashboard Build(GameObject root, RideAnalyticsManager manager)
    {
        if (manager == null)
            manager = root.GetComponent<RideAnalyticsManager>();
        if (manager == null)
            manager = root.AddComponent<RideAnalyticsManager>();
        if (root.GetComponent<RideAnalyticsApiClient>() == null)
            root.AddComponent<RideAnalyticsApiClient>();

        Canvas canvas = CreateCanvas(root.transform);
        GameObject dashboardObject = new GameObject("RideAnalyticsDashboard");
        dashboardObject.transform.SetParent(canvas.transform, false);
        Stretch(dashboardObject.AddComponent<RectTransform>());

        RideAnalyticsDashboard dashboard = dashboardObject.AddComponent<RideAnalyticsDashboard>();

        TMP_Text currentSpeed = CreateMetric(dashboardObject.transform, "Current Speed", "39.4 km/h", new Vector2(20, -12), TextAnchor.UpperLeft);
        TMP_Text averageSpeed = CreateMetric(dashboardObject.transform, "Average Speed", "28.4 km/h", new Vector2(20, -250), TextAnchor.UpperLeft);
        TMP_Text maxSpeed = CreateMetric(dashboardObject.transform, "Max Speed", "45.4 km/h", new Vector2(20, -488), TextAnchor.UpperLeft);
        TMP_Text distance = CreateMetric(dashboardObject.transform, "Distance", "12.95 km", new Vector2(20, -724), TextAnchor.UpperLeft);

        TMP_Text duration = CreateMetric(dashboardObject.transform, "Ride duration", "00:30:49", new Vector2(-20, -12), TextAnchor.UpperRight);
        TMP_Text calories = CreateMetric(dashboardObject.transform, "Calories burned", "220 k cal", new Vector2(-20, -250), TextAnchor.UpperRight);
        TMP_Text heartRate = CreateMetric(dashboardObject.transform, "Heart Rate", "132 bpm", new Vector2(-20, -488), TextAnchor.UpperRight);
        TMP_Text progress = CreateMetric(dashboardObject.transform, "Distance Progress", "62%", new Vector2(-20, -724), TextAnchor.UpperRight);

        TMP_Text mission = null;
        TMP_Text checkpoints = null;
        TMP_Text personalBest = null;

        if (root.GetComponent<RideAnalyticsHudBuilder>() != null && root.GetComponent<RideAnalyticsHudBuilder>().showAdvancedCenterStats)
        {
            mission = CreateSmallMetric(dashboardObject.transform, "Mission", "Free Ride", new Vector2(0, -82), TextAnchor.UpperCenter);
            checkpoints = CreateSmallMetric(dashboardObject.transform, "Checkpoints", "0/8", new Vector2(0, -172), TextAnchor.UpperCenter);
            personalBest = CreateSmallMetric(dashboardObject.transform, "Personal Best", "45.4 km/h", new Vector2(0, -262), TextAnchor.UpperCenter);
        }

        Image fill = CreateProgressBar(dashboardObject.transform);
        TMP_Text gear = CreateGearDisplay(dashboardObject.transform);

        CreateSpeedIcon(dashboardObject.transform);
        CreateMenuButton(dashboardObject.transform);
        TMP_Text pauseNav = CreateBottomNav(dashboardObject.transform, dashboard);
        GameObject mapPage = CreateMapPage(dashboardObject.transform, dashboard);
        GameObject analyticsPage = CreateAnalyticsPage(dashboardObject.transform, dashboard, out TMP_Text analyticsSummary);
        GameObject endRidePage = CreateEndRidePage(dashboardObject.transform, dashboard, out TMP_Text endRideSummary);

        dashboard.Bind(manager, currentSpeed, averageSpeed, maxSpeed, distance, duration, calories, heartRate, progress, gear, mission, checkpoints, personalBest, fill);
        dashboard.BindPages(mapPage, analyticsPage, endRidePage, pauseNav, analyticsSummary, endRideSummary);
        return dashboard;
    }

    private static Canvas CreateCanvas(Transform parent)
    {
        GameObject canvasObject = new GameObject("RideAnalyticsCanvas");
        canvasObject.transform.SetParent(parent, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        Stretch(canvasObject.GetComponent<RectTransform>());
        return canvas;
    }

    private static TMP_Text CreateMetric(Transform parent, string title, string value, Vector2 offset, TextAnchor alignment)
    {
        TMP_Text titleText = CreateText(parent, title + "Title", title, 58, FontStyles.Bold, alignment, White);
        TMP_Text valueText = CreateText(parent, title + "Value", value, 56, FontStyles.Bold, alignment, White);

        ConfigureAnchored(titleText.rectTransform, offset, alignment, new Vector2(620, 76));
        ConfigureAnchored(valueText.rectTransform, offset + new Vector2(0, -68), alignment, new Vector2(620, 76));
        return valueText;
    }

    private static TMP_Text CreateSmallMetric(Transform parent, string title, string value, Vector2 offset, TextAnchor alignment)
    {
        TMP_Text titleText = CreateText(parent, title + "Title", title, 30, FontStyles.Bold, alignment, White);
        TMP_Text valueText = CreateText(parent, title + "Value", value, 34, FontStyles.Bold, alignment, White);

        ConfigureAnchored(titleText.rectTransform, offset, alignment, new Vector2(360, 44));
        ConfigureAnchored(valueText.rectTransform, offset + new Vector2(0, -40), alignment, new Vector2(360, 50));
        return valueText;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, float size, FontStyles style, TextAnchor anchor, Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        TMP_Text label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.color = color;
        label.alignment = ToTextAlignment(anchor);
        label.enableWordWrapping = false;

        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return label;
    }

    private static TextAlignmentOptions ToTextAlignment(TextAnchor anchor)
    {
        if (anchor == TextAnchor.UpperRight)
            return TextAlignmentOptions.TopRight;
        if (anchor == TextAnchor.UpperCenter)
            return TextAlignmentOptions.Top;
        return TextAlignmentOptions.TopLeft;
    }

    private static void ConfigureAnchored(RectTransform rect, Vector2 offset, TextAnchor alignment, Vector2 size)
    {
        if (alignment == TextAnchor.UpperRight)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
        }
        else if (alignment == TextAnchor.UpperCenter)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
        }
        else
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
        }

        rect.anchoredPosition = offset;
        rect.sizeDelta = size;
    }

    private static Image CreateProgressBar(Transform parent)
    {
        GameObject trackObject = new GameObject("DistanceProgressTrack");
        trackObject.transform.SetParent(parent, false);
        Image track = trackObject.AddComponent<Image>();
        track.color = ProgressTrack;
        RectTransform trackRect = trackObject.GetComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(1f, 1f);
        trackRect.anchorMax = new Vector2(1f, 1f);
        trackRect.pivot = new Vector2(1f, 1f);
        trackRect.anchoredPosition = new Vector2(-20, -842);
        trackRect.sizeDelta = new Vector2(520, 18);

        GameObject fillObject = new GameObject("DistanceProgressFill");
        fillObject.transform.SetParent(trackObject.transform, false);
        Image fill = fillObject.AddComponent<Image>();
        fill.color = ProgressFill;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 0.62f;
        Stretch(fillObject.GetComponent<RectTransform>());
        return fill;
    }

    private static TMP_Text CreateGearDisplay(Transform parent)
    {
        GameObject panelObject = new GameObject("GearPanel");
        panelObject.transform.SetParent(parent, false);
        Image panel = panelObject.AddComponent<Image>();
        panel.color = new Color(0.02f, 0.02f, 0.025f, 0.92f);
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 102f);
        rect.sizeDelta = new Vector2(96f, 140f);

        TMP_Text label = CreateText(panelObject.transform, "GearLabel", "GEAR", 18, FontStyles.Normal, TextAnchor.UpperCenter, new Color(0.75f, 0.75f, 0.75f));
        ConfigureAnchored(label.rectTransform, new Vector2(0, -10), TextAnchor.UpperCenter, new Vector2(90, 28));

        TMP_Text value = CreateText(panelObject.transform, "GearValue", "6", 82, FontStyles.Bold, TextAnchor.UpperCenter, new Color(0.1f, 0.16f, 1f));
        ConfigureAnchored(value.rectTransform, new Vector2(0, -42), TextAnchor.UpperCenter, new Vector2(90, 92));
        return value;
    }

    private static void CreateSpeedIcon(Transform parent)
    {
        GameObject iconRoot = new GameObject("SpeedIcon");
        iconRoot.transform.SetParent(parent, false);
        RectTransform rect = iconRoot.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(515, -33);
        rect.sizeDelta = new Vector2(108, 108);

        GameObject circle = new GameObject("Dial");
        circle.transform.SetParent(iconRoot.transform, false);
        Image circleImage = circle.AddComponent<Image>();
        circleImage.sprite = GetCircleSprite();
        circleImage.color = new Color(0f, 0.48f, 0.68f, 0.92f);
        RectTransform circleRect = circle.GetComponent<RectTransform>();
        circleRect.anchorMin = new Vector2(0.5f, 0.5f);
        circleRect.anchorMax = new Vector2(0.5f, 0.5f);
        circleRect.pivot = new Vector2(0.5f, 0.5f);
        circleRect.sizeDelta = new Vector2(60, 60);

        CreateLine(iconRoot.transform, "Needle", new Vector2(54, 54), new Vector2(8, 44), -42f, White);
        CreateLine(iconRoot.transform, "SpeedLine1", new Vector2(8, 48), new Vector2(28, 5), 0f, new Color(0f, 0.54f, 0.45f));
        CreateLine(iconRoot.transform, "SpeedLine2", new Vector2(12, 66), new Vector2(24, 5), 0f, new Color(0f, 0.54f, 0.45f));
        CreateLine(iconRoot.transform, "SpeedLine3", new Vector2(18, 84), new Vector2(18, 5), 0f, new Color(0f, 0.54f, 0.45f));
    }

    private static void CreateMenuButton(Transform parent)
    {
        GameObject button = new GameObject("MenuButton");
        button.transform.SetParent(parent, false);
        Image image = button.AddComponent<Image>();
        image.sprite = GetCircleSprite();
        image.color = MenuBlue;
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-36, -34);
        rect.sizeDelta = new Vector2(100, 100);

        CreateLine(button.transform, "Bar1", new Vector2(50, 70), new Vector2(58, 12), 0f, White);
        CreateLine(button.transform, "Bar2", new Vector2(50, 50), new Vector2(58, 12), 0f, White);
        CreateLine(button.transform, "Bar3", new Vector2(50, 30), new Vector2(58, 12), 0f, White);
    }

    private static TMP_Text CreateBottomNav(Transform parent, RideAnalyticsDashboard dashboard)
    {
        TMP_Text pause = CreateNavText(parent, "Pause", new Vector2(-470, 0), Pink, dashboard.TogglePause);
        CreateNavText(parent, "Map", new Vector2(-230, 0), Blue, dashboard.ShowMap);
        CreateNavText(parent, "Analytics", new Vector2(95, 0), Yellow, dashboard.ShowAnalytics);
        CreateNavText(parent, "End Ride", new Vector2(430, 0), Color.black, dashboard.EndRide);
        return pause;
    }

    private static TMP_Text CreateNavText(Transform parent, string text, Vector2 offset, Color color, UnityEngine.Events.UnityAction onClick)
    {
        TMP_Text label = CreateText(parent, "Nav" + text.Replace(" ", ""), text, 62, FontStyles.Bold, TextAnchor.UpperCenter, color);
        Button button = label.gameObject.AddComponent<Button>();
        button.targetGraphic = label;
        button.onClick.AddListener(onClick);

        RectTransform rect = label.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = offset + new Vector2(0, 12);
        rect.sizeDelta = new Vector2(300, 84);
        return label;
    }

    private static GameObject CreateMapPage(Transform parent, RideAnalyticsDashboard dashboard)
    {
        GameObject page = CreatePage(parent, "MapPage");
        CreateText(page.transform, "MapTitle", "Route Map", 58, FontStyles.Bold, TextAnchor.UpperCenter, White);
        ConfigureAnchored(page.transform.Find("MapTitle").GetComponent<RectTransform>(), new Vector2(0, -82), TextAnchor.UpperCenter, new Vector2(700, 80));

        GameObject mapPanel = CreatePanel(page.transform, "MapPanel", new Vector2(0, -190), new Vector2(920, 560), new Color(0.88f, 0.92f, 0.86f, 0.96f), TextAnchor.UpperCenter);
        CreateLine(mapPanel.transform, "MapRoad1", new Vector2(165, 120), new Vector2(460, 12), 23f, new Color(0.72f, 0.72f, 0.68f));
        CreateLine(mapPanel.transform, "MapRoad2", new Vector2(430, 285), new Vector2(600, 12), -18f, new Color(0.72f, 0.72f, 0.68f));
        CreateLine(mapPanel.transform, "MapRoute1", new Vector2(230, 155), new Vector2(340, 16), 28f, Blue);
        CreateLine(mapPanel.transform, "MapRoute2", new Vector2(455, 300), new Vector2(360, 16), -15f, Blue);
        CreateMapMarker(mapPanel.transform, new Vector2(86, 82), Color.red);
        CreateMapMarker(mapPanel.transform, new Vector2(790, 365), new Color(0.1f, 0.75f, 0.24f));

        TMP_Text details = CreateText(page.transform, "MapDetails", "Current route\nCity ride loop\nCheckpoints update from distance progress", 34, FontStyles.Bold, TextAnchor.UpperCenter, White);
        ConfigureAnchored(details.rectTransform, new Vector2(0, -775), TextAnchor.UpperCenter, new Vector2(920, 130));
        CreatePageButton(page.transform, "Back to Ride", new Vector2(0, 70), new Vector2(360, 74), dashboard.ClosePages);
        page.SetActive(false);
        return page;
    }

    private static GameObject CreateAnalyticsPage(Transform parent, RideAnalyticsDashboard dashboard, out TMP_Text summary)
    {
        GameObject page = CreatePage(parent, "AnalyticsPage");
        summary = CreateText(page.transform, "AnalyticsSummary", "Ride analytics", 44, FontStyles.Bold, TextAnchor.UpperCenter, White);
        ConfigureAnchored(summary.rectTransform, new Vector2(0, -135), TextAnchor.UpperCenter, new Vector2(860, 480));

        CreatePanel(page.transform, "SpeedBar", new Vector2(-330, -650), new Vector2(120, 230), Pink, TextAnchor.UpperCenter);
        CreatePanel(page.transform, "DistanceBar", new Vector2(-110, -590), new Vector2(120, 290), Blue, TextAnchor.UpperCenter);
        CreatePanel(page.transform, "CaloriesBar", new Vector2(110, -680), new Vector2(120, 200), Yellow, TextAnchor.UpperCenter);
        CreatePanel(page.transform, "GearBar", new Vector2(330, -620), new Vector2(120, 260), MenuBlue, TextAnchor.UpperCenter);

        CreatePageButton(page.transform, "Back to Ride", new Vector2(0, 70), new Vector2(360, 74), dashboard.ClosePages);
        page.SetActive(false);
        return page;
    }

    private static GameObject CreateEndRidePage(Transform parent, RideAnalyticsDashboard dashboard, out TMP_Text summary)
    {
        GameObject page = CreatePage(parent, "EndRidePage");
        summary = CreateText(page.transform, "EndRideSummary", "Ride complete", 46, FontStyles.Bold, TextAnchor.UpperCenter, White);
        ConfigureAnchored(summary.rectTransform, new Vector2(0, -150), TextAnchor.UpperCenter, new Vector2(920, 520));
        CreatePageButton(page.transform, "View Ride", new Vector2(-210, 70), new Vector2(320, 74), dashboard.ClosePages);
        CreatePageButton(page.transform, "End Screen", new Vector2(210, 70), new Vector2(320, 74), dashboard.ReturnToGarage);
        page.SetActive(false);
        return page;
    }

    private static GameObject CreatePage(Transform parent, string name)
    {
        GameObject page = new GameObject(name);
        page.transform.SetParent(parent, false);
        Image background = page.AddComponent<Image>();
        background.color = new Color(0.02f, 0.06f, 0.09f, 0.92f);
        Stretch(page.GetComponent<RectTransform>());
        return page;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Color color, TextAnchor anchor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        ConfigureAnchored(panel.GetComponent<RectTransform>(), position, anchor, size);
        return panel;
    }

    private static void CreatePageButton(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CreatePanel(parent, label.Replace(" ", "") + "Button", position, size, MenuBlue, TextAnchor.UpperCenter);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        button.onClick.AddListener(onClick);

        TMP_Text text = CreateText(buttonObject.transform, "Label", label, 32, FontStyles.Bold, TextAnchor.UpperCenter, White);
        Stretch(text.rectTransform);
    }

    private static void CreateMapMarker(Transform parent, Vector2 position, Color color)
    {
        GameObject marker = new GameObject("MapMarker");
        marker.transform.SetParent(parent, false);
        Image image = marker.AddComponent<Image>();
        image.sprite = GetCircleSprite();
        image.color = color;
        RectTransform rect = marker.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(34, 34);
    }

    private static void CreateLine(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, float angle, Color color)
    {
        GameObject line = new GameObject(name);
        line.transform.SetParent(parent, false);
        Image image = line.AddComponent<Image>();
        image.color = color;
        RectTransform rect = line.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        rect.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null)
            return circleSprite;

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.name = "RideAnalyticsCircle";

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.48f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius - distance + 1f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return circleSprite;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
}
