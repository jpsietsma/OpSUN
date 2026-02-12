using UnityEngine;

[CreateAssetMenu(menuName = "Game/Farming/Crop Definition")]
public class CropDefinition : ScriptableObject
{
    [Header("Stages (5 total)")]
    [Tooltip("Stage 0 = seed, Stage 4 = fully grown")]
    public GameObject[] stagePrefabs = new GameObject[5];

    [Header("Timing")]
    [Tooltip("Total real-time seconds from stage 0 to stage 4")]
    public float totalGrowSeconds = 120f;

    public float SecondsPerStage
    {
        get
        {
            // 5 stages => transitions 0->1->2->3->4 = 4 transitions
            return Mathf.Max(1f, totalGrowSeconds / 4f);
        }
    }
}