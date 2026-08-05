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
        CreateBottomNav(dashboardObject.transform);

        dashboard.Bind(manager, currentSpeed, averageSpeed, maxSpeed, distance, duration, calories, heartRate, progress, gear, mission, checkpoints, personalBest, fill);
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

    private static void CreateBottomNav(Transform parent)
    {
        CreateNavText(parent, "Pause", new Vector2(-470, 0), Pink);
        CreateNavText(parent, "Map", new Vector2(-230, 0), Blue);
        CreateNavText(parent, "Analytics", new Vector2(95, 0), Yellow);
        CreateNavText(parent, "End Ride", new Vector2(430, 0), Color.black);
    }

    private static void CreateNavText(Transform parent, string text, Vector2 offset, Color color)
    {
        TMP_Text label = CreateText(parent, "Nav" + text.Replace(" ", ""), text, 62, FontStyles.Bold, TextAnchor.UpperCenter, color);
        RectTransform rect = label.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = offset + new Vector2(0, 12);
        rect.sizeDelta = new Vector2(300, 84);
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
