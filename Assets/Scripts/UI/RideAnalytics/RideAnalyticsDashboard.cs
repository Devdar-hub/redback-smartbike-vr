using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Redback.UI;

public class RideAnalyticsDashboard : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private RideAnalyticsManager analyticsManager;
    [SerializeField] private RideAnalyticsApiClient apiClient;

    [Header("Metric Labels")]
    [SerializeField] private TMP_Text currentSpeedText;
    [SerializeField] private TMP_Text averageSpeedText;
    [SerializeField] private TMP_Text maxSpeedText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text rideDurationText;
    [SerializeField] private TMP_Text caloriesText;
    [SerializeField] private TMP_Text heartRateText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text gearText;
    [SerializeField] private TMP_Text missionText;
    [SerializeField] private TMP_Text checkpointsText;
    [SerializeField] private TMP_Text personalBestText;

    [Header("Progress")]
    [SerializeField] private Image distanceProgressFill;

    [Header("HUD Pages")]
    [SerializeField] private GameObject mapPage;
    [SerializeField] private GameObject analyticsPage;
    [SerializeField] private GameObject endRidePage;
    [SerializeField] private TMP_Text pauseButtonText;
    [SerializeField] private TMP_Text analyticsSummaryText;
    [SerializeField] private TMP_Text endRideSummaryText;

    private bool pausedByHud;

    private void Awake()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;
        if (apiClient == null)
            apiClient = GetComponentInParent<RideAnalyticsApiClient>();
    }

    private void Update()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;

        if (analyticsManager == null || analyticsManager.Snapshot == null)
            return;

        Render(analyticsManager.Snapshot);
    }

    public void Bind(
        RideAnalyticsManager manager,
        TMP_Text currentSpeed,
        TMP_Text averageSpeed,
        TMP_Text maxSpeed,
        TMP_Text distance,
        TMP_Text rideDuration,
        TMP_Text calories,
        TMP_Text heartRate,
        TMP_Text progress,
        TMP_Text gear,
        TMP_Text mission,
        TMP_Text checkpoints,
        TMP_Text personalBest,
        Image progressFill)
    {
        analyticsManager = manager;
        currentSpeedText = currentSpeed;
        averageSpeedText = averageSpeed;
        maxSpeedText = maxSpeed;
        distanceText = distance;
        rideDurationText = rideDuration;
        caloriesText = calories;
        heartRateText = heartRate;
        progressText = progress;
        gearText = gear;
        missionText = mission;
        checkpointsText = checkpoints;
        personalBestText = personalBest;
        distanceProgressFill = progressFill;
    }

    public void BindPages(
        GameObject map,
        GameObject analytics,
        GameObject endRide,
        TMP_Text pauseLabel,
        TMP_Text analyticsSummary,
        TMP_Text endRideSummary)
    {
        mapPage = map;
        analyticsPage = analytics;
        endRidePage = endRide;
        pauseButtonText = pauseLabel;
        analyticsSummaryText = analyticsSummary;
        endRideSummaryText = endRideSummary;
        HidePages();
    }

    private void Render(RideAnalyticsSnapshot snapshot)
    {
        SetText(currentSpeedText, string.Format("{0:0.0} km/h", snapshot.currentSpeedKmh));
        SetText(averageSpeedText, string.Format("{0:0.0} km/h", snapshot.averageSpeedKmh));
        SetText(maxSpeedText, string.Format("{0:0.0} km/h", snapshot.maxSpeedKmh));
        SetText(distanceText, string.Format("{0:0.00} km", snapshot.distanceKm));
        SetText(rideDurationText, FormatTime(snapshot.rideTimeSeconds));
        SetText(caloriesText, string.Format("{0:0} k cal", snapshot.caloriesKcal));
        SetText(heartRateText, string.Format("{0:0} bpm", snapshot.heartRateBpm));
        SetText(progressText, string.Format("{0:0}%", snapshot.DistanceProgress * 100f));
        SetText(gearText, snapshot.currentGear > 0 ? snapshot.currentGear.ToString() : "-");
        SetText(missionText, snapshot.currentMission);
        SetText(checkpointsText, string.Format("{0}/{1}", snapshot.checkpointsCompleted, snapshot.checkpointsTotal));
        SetText(personalBestText, string.Format("{0:0.0} km/h", snapshot.personalBestKmh));

        if (distanceProgressFill != null)
            distanceProgressFill.fillAmount = snapshot.DistanceProgress;

        SetText(analyticsSummaryText, BuildAnalyticsSummary(snapshot));
    }

    private void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }

    private string FormatTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
        return string.Format("{0:00}:{1:00}:{2:00}", (int)time.TotalHours, time.Minutes, time.Seconds);
    }

    public void TogglePause()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;
        if (analyticsManager == null)
            return;

        if (pausedByHud)
        {
            analyticsManager.ResumeRide();
            Time.timeScale = 1f;
        }
        else
        {
            analyticsManager.PauseRide();
            Time.timeScale = 0f;
        }

        pausedByHud = !pausedByHud;
        SetText(pauseButtonText, pausedByHud ? "Resume" : "Pause");
        Debug.Log(pausedByHud ? "Ride Analytics HUD paused ride stats." : "Ride Analytics HUD resumed ride stats.");
    }

    public void ShowMap()
    {
        HidePages();
        if (mapPage != null)
        {
            mapPage.SetActive(true);
            return;
        }

        DashboardController controller = FindObjectOfType<DashboardController>();
        if (controller != null)
        {
            controller.ShowMapDashboardPanel();
            return;
        }

        Debug.Log("Ride Analytics HUD Map button clicked. No DashboardController map panel was found in this scene.");
    }

    public void ShowAnalytics()
    {
        HidePages();
        if (analyticsPage != null)
        {
            analyticsPage.SetActive(true);
            return;
        }

        DashboardController controller = FindObjectOfType<DashboardController>();
        if (controller != null)
        {
            controller.ShowTripDetailsPanel();
            return;
        }

        Debug.Log("Ride Analytics HUD Analytics button clicked. No DashboardController analytics panel was found in this scene.");
    }

    public void EndRide()
    {
        if (pausedByHud)
        {
            pausedByHud = false;
            Time.timeScale = 1f;
            SetText(pauseButtonText, "Pause");
        }

        RideAnalyticsSnapshot snapshot = analyticsManager != null ? analyticsManager.EndRide() : null;

        if (apiClient == null)
            apiClient = GetComponentInParent<RideAnalyticsApiClient>();

        if (apiClient != null)
            apiClient.EndBackendRide();

        HidePages();
        if (endRidePage != null)
            endRidePage.SetActive(true);
        if (snapshot != null)
            SetText(endRideSummaryText, BuildEndRideSummary(snapshot));
        Time.timeScale = 0f;

        Debug.Log("Ride Analytics HUD ended ride.");
    }

    public void ReturnToGarage()
    {
        Time.timeScale = 1f;
        MapLoader.LoadScene("GarageScene");
    }

    public void ClosePages()
    {
        HidePages();
    }

    private void HidePages()
    {
        if (mapPage != null)
            mapPage.SetActive(false);
        if (analyticsPage != null)
            analyticsPage.SetActive(false);
        if (endRidePage != null)
            endRidePage.SetActive(false);
    }

    private string BuildAnalyticsSummary(RideAnalyticsSnapshot snapshot)
    {
        return "Ride analytics\n\n"
            + "Speed: " + snapshot.currentSpeedKmh.ToString("0.0") + " km/h\n"
            + "Average: " + snapshot.averageSpeedKmh.ToString("0.0") + " km/h\n"
            + "Max: " + snapshot.maxSpeedKmh.ToString("0.0") + " km/h\n"
            + "Distance: " + snapshot.distanceKm.ToString("0.00") + " km\n"
            + "Calories: " + snapshot.caloriesKcal.ToString("0") + " kcal\n"
            + "Gear: " + (snapshot.currentGear > 0 ? snapshot.currentGear.ToString() : "0");
    }

    private string BuildEndRideSummary(RideAnalyticsSnapshot snapshot)
    {
        return "Ride complete\n\n"
            + "Time: " + FormatTime(snapshot.rideTimeSeconds) + "\n"
            + "Distance: " + snapshot.distanceKm.ToString("0.00") + " km\n"
            + "Average speed: " + snapshot.averageSpeedKmh.ToString("0.0") + " km/h\n"
            + "Max speed: " + snapshot.maxSpeedKmh.ToString("0.0") + " km/h\n"
            + "Calories: " + snapshot.caloriesKcal.ToString("0") + " kcal";
    }
}
