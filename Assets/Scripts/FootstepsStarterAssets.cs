using UnityEngine;
using StarterAssets;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FootstepsStarterAssets : MonoBehaviour
{
    [Header("Walking Sounds")]
    public AudioClip DefaultWalkClip;
    public AudioClip SandWalkClip;
    public AudioClip WoodWalkClip;
    public AudioClip ConcreteWalkClip;
    public AudioClip GravelWalkClip;
    public AudioClip SnowWalkClip;
    public AudioClip MetalWalkClip;

    [Header("Runing Sounds")]
    public AudioClip DefaultRunClip;
    public AudioClip SandRunClip;
    public AudioClip WoodRunClip;
    public AudioClip ConcreteRunClip;
    public AudioClip GravelRunClip;
    public AudioClip SnowRunClip;
    public AudioClip MetalRunClip;

    [Header("Jump/Land Sounds")]
    public AudioClip DefaultJumpClip;
    public AudioClip DefaultLandClip;

    public AudioClip SandJumpClip;
    public AudioClip SandLandClip;

    public AudioClip WoodJumpClip;
    public AudioClip WoodLandClip;

    public AudioClip ConcreteJumpClip;
    public AudioClip ConcreteLandClip;

    public AudioClip GravelJumpClip;
    public AudioClip GravelLandClip;

    public AudioClip SnowJumpClip;
    public AudioClip SnowLandClip;

    public AudioClip MetalJumpClip;
    public AudioClip MetalLandClip;

    [Header("Step timing (seconds)")]
    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.35f;

    [Header("Thresholds")]
    public float minMoveSpeed = 0.15f;      // ignore tiny motion
    public float minLandSpeed = 1.0f;       // how hard you must be falling to play land sound

    [Header("Pitch variation")]
    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;

    private CharacterController _cc;
    private AudioSource _audio;
    private StarterAssetsInputs _inputs;

    private float _stepTimer;
    private bool _wasGrounded;
    private float _lastYVelocity;

    [SerializeField] private LayerMask groundLayers = ~0; // everything by default
    [SerializeField] private float groundCheckDistance = 1.5f;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _audio = GetComponent<AudioSource>();
        _inputs = GetComponent<StarterAssetsInputs>();
        _wasGrounded = _cc.isGrounded;
    }

    void Update()
    {
        bool grounded = _cc.isGrounded;

        // Detect surface once per frame (cheap enough) so all sounds this frame match.
        SurfaceKind surface = GetCurrentSurface();

        // Jump sound: when jump pressed while grounded
        if (_inputs != null && _inputs.jump && grounded)
        {
            AudioClip jump = GetJumpClip(surface);
            PlayOne(jump);
            // Don't clear jump flag here; Starter Assets handles it.
        }

        // Landing sound: when we were in air and just became grounded,
        // and we were falling at a meaningful speed.
        if (!_wasGrounded && grounded)
        {
            if (_lastYVelocity <= -minLandSpeed)
            {
                AudioClip land = GetLandClip(surface);
                PlayOne(land);
            }
        }

        // Footsteps (only while grounded and moving)
        Vector3 v = _cc.velocity;
        float horizontalSpeed = new Vector3(v.x, 0f, v.z).magnitude;

        if (!grounded || horizontalSpeed < minMoveSpeed)
        {
            _stepTimer = 0f;
        }
        else
        {
            bool sprinting = _inputs != null && _inputs.sprint;
            float interval = sprinting ? runStepInterval : walkStepInterval;

            _stepTimer += Time.deltaTime;
            if (_stepTimer >= interval)
            {
                _stepTimer = 0f;

                AudioClip step = sprinting ? GetRunClip(surface) : GetWalkClip(surface);
                PlayOne(step);
            }
        }

        _lastYVelocity = v.y;
        _wasGrounded = grounded;
    }

    void PlayRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;

        _audio.pitch = Random.Range(pitchMin, pitchMax);
        _audio.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    void PlayOne(AudioClip clip)
    {
        if (clip == null) return;

        _audio.pitch = Random.Range(pitchMin, pitchMax);
        _audio.PlayOneShot(clip);
    }

    // OPTIONAL: If your TerrainLayer names contain these keywords, it will map automatically.
    private static SurfaceKind SurfaceFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return SurfaceKind.Default;

        name = name.ToLowerInvariant();

        if (name.Contains("sand")) return SurfaceKind.Sand;
        if (name.Contains("wood") || name.Contains("plank")) return SurfaceKind.Wood;
        if (name.Contains("concrete") || name.Contains("cement") || name.Contains("asphalt")) return SurfaceKind.Concrete;
        if (name.Contains("gravel") || name.Contains("rock") || name.Contains("pebble")) return SurfaceKind.Gravel;
        if (name.Contains("snow")) return SurfaceKind.Snow;
        if (name.Contains("metal")) return SurfaceKind.Metal;

        return SurfaceKind.Default;
    }

    private SurfaceKind GetCurrentSurface()
    {
        // Raycast down from a bit above feet
        Vector3 origin = transform.position + Vector3.up * 0.25f;

        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore))
            return SurfaceKind.Default;

        // 1) If the thing we hit has a SurfaceType component, that wins (most reliable for meshes)
        if (hit.collider.TryGetComponent<SurfaceType>(out var surfaceType))
            return surfaceType.surface;

        // 2) Terrain: determine dominant TerrainLayer under hit point
        Terrain terrain = hit.collider.GetComponent<Terrain>();
        if (terrain != null)
        {
            var kind = GetTerrainSurfaceKind(terrain, hit.point);
            return kind;
        }

        // 3) Tag fallback (optional) - set tags like "Sand", "Wood", etc.
        string tag = hit.collider.tag;
        if (!string.IsNullOrEmpty(tag))
            return SurfaceFromName(tag);

        // 4) PhysicMaterial name fallback (optional)
        var pm = hit.collider.sharedMaterial;
        if (pm != null)
            return SurfaceFromName(pm.name);

        return SurfaceKind.Default;
    }

    private SurfaceKind GetTerrainSurfaceKind(Terrain terrain, Vector3 worldPos)
    {
        TerrainData data = terrain.terrainData;
        Vector3 terrainPos = worldPos - terrain.transform.position;

        int mapX = Mathf.FloorToInt((terrainPos.x / data.size.x) * data.alphamapWidth);
        int mapZ = Mathf.FloorToInt((terrainPos.z / data.size.z) * data.alphamapHeight);

        mapX = Mathf.Clamp(mapX, 0, data.alphamapWidth - 1);
        mapZ = Mathf.Clamp(mapZ, 0, data.alphamapHeight - 1);

        float[,,] alphas = data.GetAlphamaps(mapX, mapZ, 1, 1);

        int bestIndex = 0;
        float bestWeight = 0f;

        int layerCount = alphas.GetLength(2);
        for (int i = 0; i < layerCount; i++)
        {
            float w = alphas[0, 0, i];
            if (w > bestWeight)
            {
                bestWeight = w;
                bestIndex = i;
            }
        }

        TerrainLayer[] layers = data.terrainLayers;
        if (layers != null && bestIndex >= 0 && bestIndex < layers.Length && layers[bestIndex] != null)
            return SurfaceFromName(layers[bestIndex].name);

        return SurfaceKind.Default;
    }

    private AudioClip GetWalkClip(SurfaceKind surface)
    {
        return surface switch
        {
            SurfaceKind.Sand => SandWalkClip ? SandWalkClip : DefaultWalkClip,
            SurfaceKind.Wood => WoodWalkClip ? WoodWalkClip : DefaultWalkClip,
            SurfaceKind.Concrete => ConcreteWalkClip ? ConcreteWalkClip : DefaultWalkClip,
            SurfaceKind.Gravel => GravelWalkClip ? GravelWalkClip : DefaultWalkClip,
            SurfaceKind.Snow => SnowWalkClip ? SnowWalkClip : DefaultWalkClip,
            SurfaceKind.Metal => MetalWalkClip ? MetalWalkClip : DefaultWalkClip,
            _ => DefaultWalkClip
        };
    }

    private AudioClip GetRunClip(SurfaceKind surface)
    {
        return surface switch
        {
            SurfaceKind.Sand => SandRunClip ? SandRunClip : DefaultRunClip,
            SurfaceKind.Wood => WoodRunClip ? WoodRunClip : DefaultRunClip,
            SurfaceKind.Concrete => ConcreteRunClip ? ConcreteRunClip : DefaultRunClip,
            SurfaceKind.Gravel => GravelRunClip ? GravelRunClip : DefaultRunClip,
            SurfaceKind.Snow => SnowRunClip ? SnowRunClip : DefaultRunClip,
            SurfaceKind.Metal => MetalRunClip ? MetalRunClip : DefaultRunClip,
            _ => DefaultRunClip
        };
    }

    private AudioClip GetJumpClip(SurfaceKind surface)
    {
        return surface switch
        {
            SurfaceKind.Sand => SandJumpClip ? SandJumpClip : DefaultJumpClip,
            SurfaceKind.Wood => WoodJumpClip ? WoodJumpClip : DefaultJumpClip,
            SurfaceKind.Concrete => ConcreteJumpClip ? ConcreteJumpClip : DefaultJumpClip,
            SurfaceKind.Gravel => GravelJumpClip ? GravelJumpClip : DefaultJumpClip,
            SurfaceKind.Snow => SnowJumpClip ? SnowJumpClip : DefaultJumpClip,
            SurfaceKind.Metal => MetalJumpClip ? MetalJumpClip : DefaultJumpClip,
            _ => DefaultJumpClip
        };
    }

    private AudioClip GetLandClip(SurfaceKind surface)
    {
        return surface switch
        {
            SurfaceKind.Sand => SandLandClip ? SandLandClip : DefaultLandClip,
            SurfaceKind.Wood => WoodLandClip ? WoodLandClip : DefaultLandClip,
            SurfaceKind.Concrete => ConcreteLandClip ? ConcreteLandClip : DefaultLandClip,
            SurfaceKind.Gravel => GravelLandClip ? GravelLandClip : DefaultLandClip,
            SurfaceKind.Snow => SnowLandClip ? SnowLandClip : DefaultLandClip,
            SurfaceKind.Metal => MetalLandClip ? MetalLandClip : DefaultLandClip,
            _ => DefaultLandClip
        };
    }
}
