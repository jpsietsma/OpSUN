using UnityEngine;

public class ResourceInteractive : MonoBehaviour
{
    [Header("Resource Stats")]
    public int hitsPerDrop;
    public int hitsMax;
    public int hitsLeft;

    [Header("Impact/Breaking Sounds")]
    public AudioClip extractSound;
    public AudioClip breakSound;

    [Header("List of items/amount that are dropped")]
    public ResourceDefinition itemDrop1;
    public int itemDrop1Amt;
    public ResourceDefinition itemDrop2;
    public int itemDrop2Amt;
    public ResourceDefinition itemDrop3;
    public int itemDrop3Amt;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
