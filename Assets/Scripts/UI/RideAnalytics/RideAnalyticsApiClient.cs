using System;
using System.Collections;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class RideAnalyticsApiClient : MonoBehaviour
{
    [Header("Backend")]
    [SerializeField] private string apiBaseUrl = "http://localhost:5000";
    [SerializeField] private string userId;
    [SerializeField] private string currentRideId;

    [Header("Polling")]
    [SerializeField] private bool pollDashboardHud;
    [SerializeField] private bool useBackendMockHud;
    [SerializeField] private bool applyBackendMockHudToRideValues;
    [SerializeField] private float pollIntervalSeconds = 2f;
    [SerializeField] private float targetDistanceKm = 30f;
    [SerializeField] private int fallbackGear = 6;

    [Header("References")]
    [SerializeField] private RideAnalyticsManager analyticsManager;

    private Coroutine pollingRoutine;

    public string CurrentRideId => currentRideId;

    private void Awake()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;
    }

    private void OnEnable()
    {
        if (pollDashboardHud)
            StartHudPolling();
    }

    private void OnDisable()
    {
        StopHudPolling();
    }

    [ContextMenu("Start Backend Ride")]
    public void StartBackendRide()
    {
        StartCoroutine(StartRideRoutine());
    }

    [ContextMenu("End Backend Ride")]
    public void EndBackendRide()
    {
        StartCoroutine(EndRideRoutine());
    }

    [ContextMenu("Fetch HUD Once")]
    public void FetchHudOnce()
    {
        StartCoroutine(FetchHudRoutine());
    }

    public void StartHudPolling()
    {
        pollDashboardHud = true;

        if (pollingRoutine == null)
            pollingRoutine = StartCoroutine(PollHudRoutine());
    }

    public void StopHudPolling()
    {
        pollDashboardHud = false;

        if (pollingRoutine != null)
        {
            StopCoroutine(pollingRoutine);
            pollingRoutine = null;
        }
    }

    public void SetCurrentRideId(string rideId)
    {
        currentRideId = rideId;
    }

    private IEnumerator StartRideRoutine()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;

        if (string.IsNullOrWhiteSpace(userId))
        {
            Debug.LogWarning("RideAnalyticsApiClient needs a Supabase profile userId before starting a backend ride.");
            analyticsManager?.BeginRide();
            yield break;
        }

        string body = "{\"user_id\":\"" + EscapeJson(userId) + "\"}";
        using (UnityWebRequest request = CreateJsonRequest("POST", "/api/rides/start", body))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Failed to start backend ride: " + request.error + " " + request.downloadHandler.text);
                analyticsManager?.BeginRide();
                yield break;
            }

            StartRideResponse response = JsonUtility.FromJson<StartRideResponse>(request.downloadHandler.text);
            currentRideId = response != null ? response.ride_id : null;
            analyticsManager?.BeginRide();
            Debug.Log("Backend ride started: " + currentRideId);
        }
    }

    private IEnumerator EndRideRoutine()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;

        RideAnalyticsSnapshot snapshot = analyticsManager != null ? analyticsManager.EndRide() : null;

        if (string.IsNullOrWhiteSpace(currentRideId) || snapshot == null)
        {
            Debug.LogWarning("No backend ride_id available. Ended local ride only.");
            yield break;
        }

        string body = string.Format(
            CultureInfo.InvariantCulture,
            "{{\"duration\":{0},\"distance\":{1},\"avg_speed\":{2},\"calories\":{3}}}",
            Mathf.RoundToInt(snapshot.rideTimeSeconds),
            snapshot.distanceKm,
            snapshot.averageSpeedKmh,
            snapshot.caloriesKcal);

        using (UnityWebRequest request = CreateJsonRequest("POST", "/api/rides/" + UnityWebRequest.EscapeURL(currentRideId) + "/end", body))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Failed to end backend ride: " + request.error + " " + request.downloadHandler.text);
                yield break;
            }

            Debug.Log("Backend ride ended: " + request.downloadHandler.text);
        }
    }

    private IEnumerator PollHudRoutine()
    {
        while (pollDashboardHud)
        {
            yield return FetchHudRoutine();
            yield return new WaitForSeconds(Mathf.Max(0.5f, pollIntervalSeconds));
        }

        pollingRoutine = null;
    }

    private IEnumerator FetchHudRoutine()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;

        string query = "/api/dashboard/hud?gear=" + fallbackGear.ToString(CultureInfo.InvariantCulture)
            + "&target_distance_km=" + targetDistanceKm.ToString(CultureInfo.InvariantCulture);

        if (useBackendMockHud)
            query += "&mock=true";

        if (!string.IsNullOrWhiteSpace(currentRideId))
            query += "&ride_id=" + UnityWebRequest.EscapeURL(currentRideId);

        using (UnityWebRequest request = UnityWebRequest.Get(BuildUrl(query)))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Failed to fetch dashboard HUD data: " + request.error + " " + request.downloadHandler.text);
                yield break;
            }

            DashboardHudResponse response = JsonUtility.FromJson<DashboardHudResponse>(request.downloadHandler.text);
            if (response == null)
                yield break;

            currentRideId = string.IsNullOrWhiteSpace(currentRideId) ? response.rideId : currentRideId;

            if (useBackendMockHud && analyticsManager != null && analyticsManager.IsUsingMovementDrivenValues)
            {
                Debug.Log("Dashboard mock API response ignored because the HUD is using movement-driven ride values.");
                yield break;
            }

            if (useBackendMockHud && !applyBackendMockHudToRideValues)
            {
                Debug.Log("Dashboard mock API response received. Enable Apply Backend Mock Hud To Ride Values to use mock API values in the HUD.");
                yield break;
            }

            analyticsManager?.SetApiValues(
                response.currentSpeedKmh,
                response.cadenceRpm,
                response.heartRateBpm,
                response.powerWatts,
                response.currentGear > 0 ? response.currentGear : fallbackGear);
        }
    }

    private UnityWebRequest CreateJsonRequest(string method, string path, string body)
    {
        UnityWebRequest request = new UnityWebRequest(BuildUrl(path), method);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    private string BuildUrl(string path)
    {
        return apiBaseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
    }

    private static string EscapeJson(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    [Serializable]
    private class StartRideResponse
    {
        public string ride_id;
    }

    [Serializable]
    private class DashboardHudResponse
    {
        public string rideId;
        public float currentSpeedKmh;
        public float cadenceRpm;
        public float heartRateBpm;
        public float powerWatts;
        public int currentGear;
    }
}
