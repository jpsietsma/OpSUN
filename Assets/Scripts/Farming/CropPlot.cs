using UnityEngine;

public class CropPlot : MonoBehaviour
{
    [Header("Where the plant stages will spawn")]
    [SerializeField] private Transform plantSpawnPoint;

    [Header("State")]
    [SerializeField] private bool isPlanted;
    [SerializeField] private bool isReadyToHarvest;
    [SerializeField] private int currentStageIndex; // 0..4

    private CropDefinition cropDef;
    private GameObject currentStageInstance;

    private float plantedTime;
    private float nextStageTime;

    public bool CanPlant => !isPlanted;
    public bool ReadyToHarvest => isReadyToHarvest;

    private void Awake()
    {
        if (plantSpawnPoint == null)
            plantSpawnPoint = transform;
    }

    private void Update()
    {
        if (!isPlanted || isReadyToHarvest || cropDef == null)
            return;

        if (Time.time >= nextStageTime)
        {
            AdvanceStage();
        }
    }

    public bool TryPlant(CropDefinition def)
    {
        if (def == null) return false;
        if (isPlanted) return false;
        if (def.stagePrefabs == null || def.stagePrefabs.Length < 5) return false;

        cropDef = def;

        var harvestInfo = GetComponent<CropPlotHarvestInfo>();
        if (harvestInfo == null) harvestInfo = gameObject.AddComponent<CropPlotHarvestInfo>();

        isPlanted = true;
        isReadyToHarvest = false;

        currentStageIndex = 0;
        plantedTime = Time.time;

        SpawnStage(currentStageIndex);

        nextStageTime = Time.time + cropDef.SecondsPerStage;
        return true;
    }

    private void AdvanceStage()
    {
        currentStageIndex = Mathf.Clamp(currentStageIndex + 1, 0, 4);
        SpawnStage(currentStageIndex);

        if (currentStageIndex >= 4)
        {
            isReadyToHarvest = true;
        }
        else
        {
            nextStageTime = Time.time + cropDef.SecondsPerStage;
        }
    }

    private void SpawnStage(int stageIndex)
    {
        if (currentStageInstance != null)
            Destroy(currentStageInstance);

        var prefab = cropDef.stagePrefabs[stageIndex];
        if (prefab == null)
        {
            Debug.LogWarning($"[CropPlot] Missing stage prefab index {stageIndex} on {cropDef.name}");
            return;
        }

        currentStageInstance = Instantiate(prefab, plantSpawnPoint.position, plantSpawnPoint.rotation, plantSpawnPoint);
    }

    public void ClearPlot()
    {
        isPlanted = false;
        isReadyToHarvest = false;
        currentStageIndex = 0;
        cropDef = null;

        if (currentStageInstance != null)
            Destroy(currentStageInstance);
    }
}
