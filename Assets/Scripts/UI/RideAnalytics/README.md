# Ride Analytics HUD

This module creates the Smart Ride Analytics dashboard shown in the Figma mockup.

## Files

- `RideAnalyticsManager.cs` collects speed, cadence, heart rate, power, ride time, distance, calories, mission, checkpoint, personal-best, and gear values.
- `RideAnalyticsDashboard.cs` formats analytics values into TextMeshPro UI labels.
- `RideAnalyticsHudBuilder.cs` builds the screen-space dashboard UI at runtime.
- `RideAnalyticsApiClient.cs` can start/end backend ride sessions and poll dashboard HUD values from the Node/Express API.
- `Assets/Editor/RideAnalytics/RideAnalyticsHudPrefabCreator.cs` adds Unity menu tools for creating the HUD prefab or adding it to the current scene.

## Local Test

1. Open the project in Unity `2022.3.22f1`.
2. Wait for Unity to compile the scripts.
3. Open a playable scene such as `Assets/Scenes/CityScene.unity`.
4. Use `SmartBike > Ride Analytics > Add HUD To Current Scene`.
5. Press Play.

The HUD starts at zero unless it receives movement, MQTT values, or backend database values. It no longer requests generated mock HUD data from the backend.

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

`RideAnalyticsApiClient` now calls `SetBackendHudValues` automatically when API polling is enabled. This applies the full backend HUD response, including speed, cadence, heart rate, power, distance, calories, ride time, average speed, max speed, gear, and progress.

## Backend API Test

1. Start the backend project locally on port `5000`.
2. In Unity, select the `RideAnalyticsHUD` object.
3. In `Ride Analytics Api Client`, keep `Api Base Url` as `http://localhost:5000`.
4. Add a valid database `ride_id` to `Current Ride Id` to display stored database demo values.
5. Add a valid Supabase `profiles.id` value to `User Id` if you want to test backend ride saving.
6. Use the component context menu:
   - `Start Backend Ride`
   - `Fetch HUD Once`
   - `End Backend Ride`
7. To keep polling live data, enable `Poll Dashboard Hud`.

If `User Id` is empty, the client still starts a local ride, but backend ride saving requires a valid Supabase profile UUID because the `rides.user_id` column is required.

Use backend database demo data for dashboard testing. Do not append `mock=true` to `/api/dashboard/hud`; that path generates temporary values and is not suitable for final dashboard integration.
