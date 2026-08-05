# Ride Analytics HUD

This module creates the Smart Ride Analytics dashboard shown in the Figma mockup.

## Files

- `RideAnalyticsManager.cs` collects speed, cadence, heart rate, power, ride time, distance, calories, mission, checkpoint, personal-best, and gear values.
- `RideAnalyticsDashboard.cs` formats analytics values into TextMeshPro UI labels.
- `RideAnalyticsHudBuilder.cs` builds the screen-space dashboard UI at runtime.
- `Assets/Editor/RideAnalytics/RideAnalyticsHudPrefabCreator.cs` adds Unity menu tools for creating the HUD prefab or adding it to the current scene.

## Local Test

1. Open the project in Unity `2022.3.22f1`.
2. Wait for Unity to compile the scripts.
3. Open a playable scene such as `Assets/Scenes/CityScene.unity`.
4. Use `SmartBike > Ride Analytics > Add HUD To Current Scene`.
5. Press Play.

The HUD uses mock ride data when MQTT is not connected, so the numbers should update immediately.

If the HUD already exists in the scene and looks wrong after script changes:

1. Select `RideAnalyticsHUD` in the Hierarchy.
2. Use `SmartBike > Ride Analytics > Rebuild Selected HUD`.
3. Press Play again.

For the closest match to the Figma image, use the Game view or Simulator at a 16:9 resolution such as `1920x1080`.

## Create a Reusable Prefab

Use `SmartBike > Ride Analytics > Create HUD Prefab`.

Unity will create:

`Assets/Prefabs/UI/RideAnalyticsHUD.prefab`

Drop that prefab into any scene where the analytics dashboard should appear.

## Data Integration

The manager already reads these MQTT topics when `Mqtt.Instance` is connected:

- `bike/{DeviceId}/speed`
- `bike/{DeviceId}/cadence`
- `bike/{DeviceId}/heartrate`
- `bike/{DeviceId}/power`

For REST API integration, call:

```csharp
RideAnalyticsManager.Instance.SetApiValues(speedKmh, cadenceRpm, heartRateBpm, powerWatts, gear);
```

When the real API details are available, add a small API client that fetches values and passes them into this method.
