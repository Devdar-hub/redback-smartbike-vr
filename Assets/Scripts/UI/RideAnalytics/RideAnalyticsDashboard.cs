using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RideAnalyticsDashboard : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private RideAnalyticsManager analyticsManager;

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

    private void Awake()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;
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
}
