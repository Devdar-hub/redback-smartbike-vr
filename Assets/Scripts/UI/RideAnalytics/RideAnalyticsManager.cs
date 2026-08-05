using System;
using System.Globalization;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

public class RideAnalyticsManager : MonoBehaviour
{
    public static RideAnalyticsManager Instance { get; private set; }

    [Header("Data Source")]
    [SerializeField] private bool useMockDataWhenMqttUnavailable = true;
    [SerializeField] private bool subscribeToMqtt = true;

    [Header("Ride Settings")]
    [SerializeField] private float riderWeightKg = 75f;
    [SerializeField] private float defaultPersonalBestKmh = 45.4f;
    [SerializeField] private int checkpointsTotal = 8;
    [SerializeField] private int currentGear = 6;

    [Header("Mock Values")]
    [SerializeField] private float mockBaseSpeedKmh = 39.4f;
    [SerializeField] private float mockCadenceRpm = 82f;
    [SerializeField] private float mockHeartRateBpm = 132f;

    private readonly object mqttLock = new object();
    private bool mqttSubscribed;
    private float mqttSpeedKmh;
    private float mqttCadenceRpm;
    private float mqttHeartRateBpm;
    private float mqttPowerWatts;
    private float rideStartTime;
    private float distanceKm;
    private float maxSpeedKmh;
    private float speedSampleTotal;
    private int speedSampleCount;

    public RideAnalyticsSnapshot Snapshot { get; private set; } = new RideAnalyticsSnapshot();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        rideStartTime = Time.time;
        Snapshot.personalBestKmh = defaultPersonalBestKmh;
        Snapshot.currentMission = "Free Ride";
        Snapshot.checkpointsTotal = checkpointsTotal;
        Snapshot.currentGear = currentGear;
    }

    private void OnDestroy()
    {
        if (mqttSubscribed && Mqtt.Instance != null)
            Mqtt.Instance.Unsubscribe(OnMqttMessage);

        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        TrySubscribeToMqtt();

        float speed = GetCurrentSpeed();
        float cadence = GetCadence();
        float heartRate = GetHeartRate();
        float power = GetPowerWatts();

        distanceKm += speed * Time.deltaTime / 3600f;
        maxSpeedKmh = Mathf.Max(maxSpeedKmh, speed);

        speedSampleTotal += speed;
        speedSampleCount++;

        Snapshot.currentSpeedKmh = speed;
        Snapshot.averageSpeedKmh = speedSampleCount == 0 ? 0f : speedSampleTotal / speedSampleCount;
        Snapshot.maxSpeedKmh = maxSpeedKmh;
        Snapshot.cadenceRpm = cadence;
        Snapshot.distanceKm = distanceKm;
        Snapshot.caloriesKcal = EstimateCalories(power, speed);
        Snapshot.heartRateBpm = heartRate;
        Snapshot.rideTimeSeconds = Time.time - rideStartTime;
        Snapshot.currentMission = GetMissionName();
        Snapshot.checkpointsCompleted = EstimateCheckpointProgress();
        Snapshot.checkpointsTotal = checkpointsTotal;
        Snapshot.personalBestKmh = Mathf.Max(defaultPersonalBestKmh, maxSpeedKmh);
        Snapshot.currentGear = currentGear;
    }

    public void SetApiValues(float speedKmh, float cadenceRpm, float heartRateBpm, float powerWatts, int gear)
    {
        lock (mqttLock)
        {
            mqttSpeedKmh = Mathf.Max(0f, speedKmh);
            mqttCadenceRpm = Mathf.Max(0f, cadenceRpm);
            mqttHeartRateBpm = Mathf.Max(0f, heartRateBpm);
            mqttPowerWatts = Mathf.Max(0f, powerWatts);
            currentGear = Mathf.Max(0, gear);
        }
    }

    private void TrySubscribeToMqtt()
    {
        if (!subscribeToMqtt || mqttSubscribed || Mqtt.Instance == null || !Mqtt.Instance.IsConnected)
            return;

        Mqtt.Instance.Subscribe(OnMqttMessage, Mqtt.SpeedTopic, Mqtt.CadenceTopic, Mqtt.HeartRateTopic, Mqtt.PowerTopic);
        mqttSubscribed = true;
    }

    private void OnMqttMessage(object sender, MqttMsgPublishEventArgs e)
    {
        string message = System.Text.Encoding.UTF8.GetString(e.Message);
        float value = ReadFloatField(message, "value");

        if (value <= 0f)
        {
            value = ReadFloatField(message, "speed");
            if (value <= 0f)
                value = ReadFloatField(message, "cadence");
            if (value <= 0f)
                value = ReadFloatField(message, "heartrate");
            if (value <= 0f)
                value = ReadFloatField(message, "heartRate");
            if (value <= 0f)
                value = ReadFloatField(message, "power");
        }

        lock (mqttLock)
        {
            if (e.Topic == Mqtt.SpeedTopic)
                mqttSpeedKmh = Mathf.Max(0f, value);
            else if (e.Topic == Mqtt.CadenceTopic)
                mqttCadenceRpm = Mathf.Max(0f, value);
            else if (e.Topic == Mqtt.HeartRateTopic)
                mqttHeartRateBpm = Mathf.Max(0f, value);
            else if (e.Topic == Mqtt.PowerTopic)
                mqttPowerWatts = Mathf.Max(0f, value);
        }
    }

    private float GetCurrentSpeed()
    {
        lock (mqttLock)
        {
            if (mqttSpeedKmh > 0f)
                return mqttSpeedKmh;
        }

        if (!useMockDataWhenMqttUnavailable)
            return 0f;

        return mockBaseSpeedKmh + Mathf.Sin(Time.time * 0.8f) * 2.2f;
    }

    private float GetCadence()
    {
        lock (mqttLock)
        {
            if (mqttCadenceRpm > 0f)
                return mqttCadenceRpm;
        }

        return useMockDataWhenMqttUnavailable ? mockCadenceRpm + Mathf.Sin(Time.time * 0.7f) * 4f : 0f;
    }

    private float GetHeartRate()
    {
        lock (mqttLock)
        {
            if (mqttHeartRateBpm > 0f)
                return mqttHeartRateBpm;
        }

        return useMockDataWhenMqttUnavailable ? mockHeartRateBpm + Mathf.Sin(Time.time * 0.45f) * 5f : 0f;
    }

    private float GetPowerWatts()
    {
        lock (mqttLock)
        {
            if (mqttPowerWatts > 0f)
                return mqttPowerWatts;
        }

        return Mathf.Max(80f, GetCurrentSpeed() * 5.2f);
    }

    private float EstimateCalories(float powerWatts, float speedKmh)
    {
        float elapsedHours = Mathf.Max(0f, Time.time - rideStartTime) / 3600f;
        float met = Mathf.Lerp(6f, 12f, Mathf.InverseLerp(12f, 36f, speedKmh));
        float caloriesFromMet = met * riderWeightKg * elapsedHours;
        float caloriesFromPower = powerWatts * Mathf.Max(0f, Time.time - rideStartTime) / 4184f;
        return Mathf.Max(caloriesFromMet, caloriesFromPower);
    }

    private string GetMissionName()
    {
        if (Mission_Activator.ActiveMission != null)
            return Mission_Activator.ActiveMission.MissionName;

        return "Free Ride";
    }

    private int EstimateCheckpointProgress()
    {
        if (checkpointsTotal <= 0)
            return 0;

        return Mathf.Clamp(Mathf.FloorToInt(distanceKm * 1.6f), 0, checkpointsTotal);
    }

    private float ReadFloatField(string message, string fieldName)
    {
        int keyIndex = message.IndexOf("\"" + fieldName + "\"", StringComparison.OrdinalIgnoreCase);
        if (keyIndex == -1)
            keyIndex = message.IndexOf("'" + fieldName + "'", StringComparison.OrdinalIgnoreCase);
        if (keyIndex == -1)
            return 0f;

        int colonIndex = message.IndexOf(':', keyIndex);
        if (colonIndex == -1)
            return 0f;

        int endIndex = message.IndexOfAny(new[] { ',', '}' }, colonIndex + 1);
        if (endIndex == -1)
            endIndex = message.Length;

        string rawValue = message.Substring(colonIndex + 1, endIndex - colonIndex - 1).Trim().Trim('\'', '"');
        float parsed;
        return float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) ? parsed : 0f;
    }
}
