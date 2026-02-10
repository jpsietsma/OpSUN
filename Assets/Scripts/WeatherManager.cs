using System;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public enum WeatherType { Calm, Rain, Storm, Blizzard }
    public enum ZoneType { Default, Beach, Mountaintop }

    [Header("References")]
    public DayNightCycle dayNight;
    public Transform player;                 // optional (only needed if you want manager to know player tag usage elsewhere)

    [Header("Per-day changes")]
    [Tooltip("Max number of weather changes per in-game day (0-2 recommended).")]
    [Range(0, 2)] public int maxChangesPerDay = 2;

    [Tooltip("Minimum hours between weather changes.")]
    [Range(0.5f, 12f)] public float minGapHours = 4f;

    [Tooltip("Earliest hour a change can happen.")]
    [Range(0f, 23.5f)] public float earliestChangeHour = 2f;

    [Tooltip("Latest hour a change can happen.")]
    [Range(0.5f, 24f)] public float latestChangeHour = 22f;

    [Header("Base weights (Default zone)")]
    public float calmWeight = 70f;
    public float rainWeight = 20f;
    public float stormWeight = 10f;

    [Header("Zone modifiers")]
    [Tooltip("Extra weight added to Storm in the Beach zone.")]
    public float beachStormBonus = 15f;

    [Tooltip("Extra weight added to Blizzard on the Mountaintop (Blizzard only exists there).")]
    public float mountainBlizzardWeight = 35f;

    [Tooltip("Optional: reduce Calm slightly on Mountaintop so Blizzard/Rain show more.")]
    public float mountainCalmPenalty = 10f;

    [Header("Effects (drag GameObjects to enable/disable)")]
    public GameObject calmFX;
    public GameObject rainFX;
    public GameObject stormFX;
    public GameObject blizzardFX;

    public WeatherType CurrentWeather { get; private set; } = WeatherType.Calm;
    public ZoneType CurrentZone { get; private set; } = ZoneType.Default;

    public event Action<WeatherType> OnWeatherChanged;

    int _lastDay = -1;

    // Scheduled change times in day fraction (0..1)
    float[] _changeTimes01 = new float[2] { -1f, -1f };
    int _nextChangeIndex = 0;
    int _changesToday = 0;

    void Start()
    {
        if (dayNight == null)
        {
            Debug.LogError("WeatherManager: DayNightCycle reference is missing.");
            enabled = false;
            return;
        }

        // Initialize
        _lastDay = dayNight.currentDay;
        SetWeather(WeatherType.Calm);
        ScheduleNewDay();
    }

    void Update()
    {
        // New day?
        if (dayNight.currentDay != _lastDay)
        {
            _lastDay = dayNight.currentDay;
            ScheduleNewDay();
        }

        // No more changes scheduled today
        if (_nextChangeIndex >= _changesToday) return;

        float now = dayNight.time01;
        float target = _changeTimes01[_nextChangeIndex];

        // We scheduled within the same day window, so simple compare works
        if (now >= target)
        {
            RollAndApplyWeather();
            _nextChangeIndex++;
        }
    }

    public void SetZone(ZoneType zone)
    {
        CurrentZone = zone;
        // We do NOT force an immediate reroll. Zone affects the next scheduled change.
    }

    void ScheduleNewDay()
    {
        _nextChangeIndex = 0;

        // Decide 0..maxChangesPerDay changes for today (biased toward fewer changes)
        // (You can change these odds)
        int changes = maxChangesPerDay;
        if (maxChangesPerDay == 2)
        {
            // 20% chance 0 changes, 50% chance 1, 30% chance 2
            float r = UnityEngine.Random.value;
            changes = (r < 0.20f) ? 0 : (r < 0.70f ? 1 : 2);
        }
        else if (maxChangesPerDay == 1)
        {
            // 30% chance 0 changes, 70% chance 1
            changes = (UnityEngine.Random.value < 0.30f) ? 0 : 1;
        }

        _changesToday = changes;
        _changeTimes01[0] = -1f;
        _changeTimes01[1] = -1f;

        if (_changesToday == 0) return;

        // Convert hour constraints to 0..1
        float min01 = Mathf.Clamp01(earliestChangeHour / 24f);
        float max01 = Mathf.Clamp01(latestChangeHour / 24f);
        if (max01 <= min01) { min01 = 0.05f; max01 = 0.95f; }

        // Pick times with minimum separation
        float gap01 = Mathf.Clamp01(minGapHours / 24f);

        if (_changesToday == 1)
        {
            _changeTimes01[0] = UnityEngine.Random.Range(min01, max01);
        }
        else // 2 changes
        {
            // Try a few times to find two that aren't too close
            for (int attempt = 0; attempt < 30; attempt++)
            {
                float a = UnityEngine.Random.Range(min01, max01);
                float b = UnityEngine.Random.Range(min01, max01);

                if (Mathf.Abs(a - b) >= gap01)
                {
                    if (a < b)
                    {
                        _changeTimes01[0] = a;
                        _changeTimes01[1] = b;
                    }
                    else
                    {
                        _changeTimes01[0] = b;
                        _changeTimes01[1] = a;
                    }
                    return;
                }
            }

            // Fallback if we couldn't find a valid pair
            float mid = (min01 + max01) * 0.5f;
            _changeTimes01[0] = Mathf.Clamp(mid - gap01 * 0.5f, min01, max01);
            _changeTimes01[1] = Mathf.Clamp(mid + gap01 * 0.5f, min01, max01);
        }
    }

    void RollAndApplyWeather()
    {
        // Base weights (everywhere)
        float calm = calmWeight;
        float rain = rainWeight;
        float storm = stormWeight;
        float blizzard = 0f;

        // Zone modifiers
        if (CurrentZone == ZoneType.Beach)
        {
            storm += beachStormBonus;
        }
        else if (CurrentZone == ZoneType.Mountaintop)
        {
            blizzard = mountainBlizzardWeight;
            calm = Mathf.Max(0f, calm - mountainCalmPenalty);
        }

        // Roll weighted
        WeatherType rolled = RollWeighted(calm, rain, storm, blizzard);

        // If blizzard rolled but not in mountaintop, fall back (shouldn't happen because blizzard weight=0 elsewhere)
        if (rolled == WeatherType.Blizzard && CurrentZone != ZoneType.Mountaintop)
            rolled = WeatherType.Storm;

        SetWeather(rolled);
    }

    WeatherType RollWeighted(float calm, float rain, float storm, float blizzard)
    {
        float total = Mathf.Max(0f, calm) + Mathf.Max(0f, rain) + Mathf.Max(0f, storm) + Mathf.Max(0f, blizzard);
        if (total <= 0.0001f) return WeatherType.Calm;

        float r = UnityEngine.Random.value * total;

        r -= Mathf.Max(0f, calm);
        if (r <= 0f) return WeatherType.Calm;

        r -= Mathf.Max(0f, rain);
        if (r <= 0f) return WeatherType.Rain;

        r -= Mathf.Max(0f, storm);
        if (r <= 0f) return WeatherType.Storm;

        return WeatherType.Blizzard;
    }

    void SetWeather(WeatherType w)
    {
        CurrentWeather = w;

        // Toggle effects
        if (calmFX) calmFX.SetActive(w == WeatherType.Calm);
        if (rainFX) rainFX.SetActive(w == WeatherType.Rain);
        if (stormFX) stormFX.SetActive(w == WeatherType.Storm);
        if (blizzardFX) blizzardFX.SetActive(w == WeatherType.Blizzard);

        OnWeatherChanged?.Invoke(w);
        // Debug.Log($"Weather: {w} (Zone: {CurrentZone}) Day {dayNight.currentDay}");
    }
}
