using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Day Counter")]
    public int currentDay = 1;     // Day 1 starts at game start

    [Header("Time")]
    [Tooltip("Real-time minutes for a full 24h cycle")]
    public float dayLengthMinutes = 10f;
    [Range(0f, 24f)] public float startTimeHours = 12f;

    [Header("Lights")]
    public Light sun;
    public Light moon;

    [Header("Intensity")]
    public float sunMaxIntensity = 1.2f;
    public float moonMaxIntensity = 0.25f;

    [Header("Colors")]
    public Gradient sunColorOverDay = new Gradient();
    public Gradient ambientColorOverDay = new Gradient();

    [Header("Ambient")]
    public bool controlAmbient = true;

    [Header("Skybox (Day/Night)")]
    public Material daySkybox;
    public Material nightSkybox;

    [Tooltip("When time01 is between these values, we consider it NIGHT (wraps past 1.0).")]
    [Range(0f, 1f)] public float nightStartsAt01 = 0.75f; // ~6pm
    [Range(0f, 1f)] public float dayStartsAt01 = 0.25f;   // ~6am

    [Tooltip("Optional: drive skybox exposure if the shader supports _Exposure.")]
    public bool controlSkyboxExposure = true;
    public AnimationCurve skyboxExposureOverDay = AnimationCurve.Linear(0f, 0.6f, 1f, 0.6f);

    [Tooltip("Update GI environment every N seconds (0 = only when skybox swaps).")]
    public float giUpdateInterval = 5f;

    // 0..1 where 0 = midnight, 0.5 = noon
    [Range(0f, 1f)] public float time01;

    float _lastTime01;
    Material _activeSkybox;
    float _giTimer;

    void Start()
    {
        time01 = Mathf.Repeat(startTimeHours / 24f, 1f);

        // If you didn't set gradients, provide decent defaults
        if (sunColorOverDay.colorKeys.Length == 0)
        {
            sunColorOverDay = DefaultSunGradient();
            ambientColorOverDay = DefaultAmbientGradient();
        }

        // If user didn't set curve, provide a simple default:
        if (skyboxExposureOverDay == null || skyboxExposureOverDay.keys.Length == 0)
        {
            // Dark at midnight, bright at noon, dark at next midnight
            skyboxExposureOverDay = new AnimationCurve(
                new Keyframe(0.00f, 0.55f),
                new Keyframe(0.23f, 0.90f),
                new Keyframe(0.50f, 1.20f),
                new Keyframe(0.77f, 0.90f),
                new Keyframe(1.00f, 0.55f)
            );
        }

        ApplySkyboxForTime(time01, force: true);
        UpdateLighting(time01);
        DynamicGI.UpdateEnvironment();
    }

    void Update()
    {
        float daySeconds = Mathf.Max(10f, dayLengthMinutes * 60f);

        _lastTime01 = time01;
        time01 = Mathf.Repeat(time01 + Time.deltaTime / daySeconds, 1f);

        // Detect wrap-around (new day)
        if (_lastTime01 > time01)
            currentDay++;

        UpdateLighting(time01);
        ApplySkyboxForTime(time01, force: false);

        // Optional periodic GI refresh
        if (giUpdateInterval > 0f)
        {
            _giTimer += Time.deltaTime;
            if (_giTimer >= giUpdateInterval)
            {
                _giTimer = 0f;
                DynamicGI.UpdateEnvironment();
            }
        }
    }

    void ApplySkyboxForTime(float t, bool force)
    {
        if (daySkybox == null || nightSkybox == null)
            return;

        bool isNight = IsNight(t);
        Material target = isNight ? nightSkybox : daySkybox;

        if (force || RenderSettings.skybox != target)
        {
            RenderSettings.skybox = target;
            _activeSkybox = target;

            // When swapping skybox, refresh environment immediately
            _giTimer = 0f;
            DynamicGI.UpdateEnvironment();
        }

        // Optional exposure control (only if shader supports it)
        if (controlSkyboxExposure && _activeSkybox != null && _activeSkybox.HasProperty("_Exposure"))
        {
            float exposure = skyboxExposureOverDay.Evaluate(t);
            _activeSkybox.SetFloat("_Exposure", exposure);
        }
    }

    bool IsNight(float t)
    {
        // Night if time >= nightStartsAt OR time < dayStartsAt
        return (t >= nightStartsAt01 || t < dayStartsAt01);
    }

    void UpdateLighting(float t)
    {
        // Sun angle: -90 at midnight, +90 at noon, back to 270 at next midnight
        float sunAngle = (t * 360f) - 90f;

        if (sun != null)
        {
            sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

            // daylight factor: 0 at night, 1 at noon
            float daylight = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI)); // 0..1..0

            sun.intensity = daylight * sunMaxIntensity;
            sun.color = sunColorOverDay.Evaluate(t);
        }

        if (moon != null)
        {
            // Opposite side of the sun
            moon.transform.rotation = Quaternion.Euler(sunAngle + 180f, 170f, 0f);

            float daylight = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI));
            float night = 1f - daylight;

            moon.intensity = night * moonMaxIntensity;
        }

        if (controlAmbient)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColorOverDay.Evaluate(t);
        }
    }

    Gradient DefaultSunGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.05f,0.05f,0.1f), 0.00f), // midnight
                new GradientColorKey(new Color(1.0f,0.55f,0.25f), 0.23f), // sunrise
                new GradientColorKey(new Color(1.0f,0.98f,0.92f), 0.50f), // noon
                new GradientColorKey(new Color(1.0f,0.45f,0.20f), 0.77f), // sunset
                new GradientColorKey(new Color(0.05f,0.05f,0.1f), 1.00f)  // midnight
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );
        return g;
    }

    Gradient DefaultAmbientGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.02f,0.02f,0.05f), 0.00f),
                new GradientColorKey(new Color(0.35f,0.25f,0.20f), 0.23f),
                new GradientColorKey(new Color(0.60f,0.60f,0.65f), 0.50f),
                new GradientColorKey(new Color(0.35f,0.22f,0.18f), 0.77f),
                new GradientColorKey(new Color(0.02f,0.02f,0.05f), 1.00f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );
        return g;
    }
}
