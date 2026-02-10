using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTreeChopSystem : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;
    public float range = 3f;
    public LayerMask hitMask = ~0;              // include Terrain + choppable tree colliders
    public LayerMask terrainOnlyMask = ~0;      // set to Terrain layer for best results

    [Header("Conversion")]
    public GameObject interactiveTreePrefab;    // Tree_Interactive (has ChoppableTree + collider)
    public float spawnYOffset = 0f;
    public float terrainTreePickRadius = 1.5f;  // how close hit point must be to a tree instance

    [Header("Cooldown")]
    public float hitCooldown = 0.25f;

    private float _nextHitTime;

    // Tracks converted trees by a stable key so we don't double-convert
    private readonly Dictionary<int, ChoppableTree> _spawnedByKey = new();

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    /// Call this from your existing tool swing system at the hit frame.
    public void TryChop(float damage)
    {
        if (Time.time < _nextHitTime) return;
        _nextHitTime = Time.time + hitCooldown;

        if (cam == null) return;

        // 1) If we hit an existing choppable GameObject, damage it immediately
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, range, hitMask))
        {
            var choppable = hit.collider.GetComponentInParent<ChoppableTree>();
            if (choppable != null)
            {
                choppable.ApplyChop(damage, hit.point, hit.normal);
                return;
            }
        }

        // 2) Otherwise: try terrain (painted trees live here)
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit thit, range, terrainOnlyMask))
            return;

        var terrain = thit.collider.GetComponent<Terrain>();
        if (terrain == null) return;
        if (interactiveTreePrefab == null) return;

        if (!TryFindNearestTreeInstance(terrain, thit.point, terrainTreePickRadius, out int treeIndex, out TreeInstance tree))
            return;

        // Defer the terrain modification + flush to next frame (fixes your error)
        StartCoroutine(DeferredConvertAndChop(terrain, treeIndex, tree, thit.point, thit.normal, damage));
    }

    private IEnumerator DeferredConvertAndChop(Terrain terrain, int treeIndex, TreeInstance tree, Vector3 hitPoint, Vector3 hitNormal, float damage)
    {
        // Wait one frame so we're out of the animation event callback stack
        yield return null;

        int key = MakeTreeKey(terrain, tree);

        if (!_spawnedByKey.TryGetValue(key, out ChoppableTree spawned) || spawned == null)
        {
            RemoveTreeInstance_NoFlush(terrain, treeIndex);

            // Spawn interactive prefab at the exact tree position
            Vector3 worldPos = TreeWorldPosition(terrain, tree) + Vector3.up * spawnYOffset;
            Quaternion worldRot = Quaternion.Euler(0f, tree.rotation * Mathf.Rad2Deg, 0f);
            Vector3 worldScale = new Vector3(tree.widthScale, tree.heightScale, tree.widthScale);

            GameObject go = Instantiate(interactiveTreePrefab, worldPos, worldRot);
            go.transform.localScale = Vector3.Scale(go.transform.localScale, worldScale);

            spawned = go.GetComponent<ChoppableTree>();
            _spawnedByKey[key] = spawned;

            // Now it's safe to flush/rebuild terrain after removing instance
            terrain.Flush();
        }

        if (spawned != null)
            spawned.ApplyChop(damage, hitPoint, hitNormal);
    }

    private static Vector3 TreeWorldPosition(Terrain terrain, TreeInstance tree)
    {
        Vector3 tpos = tree.position;
        Vector3 size = terrain.terrainData.size;
        Vector3 terrainPos = terrain.transform.position;
        return new Vector3(
            terrainPos.x + tpos.x * size.x,
            terrainPos.y + tpos.y * size.y,
            terrainPos.z + tpos.z * size.z
        );
    }

    private static int MakeTreeKey(Terrain terrain, TreeInstance tree)
    {
        Vector3 p = tree.position;
        int hx = Mathf.RoundToInt(p.x * 10000f);
        int hz = Mathf.RoundToInt(p.z * 10000f);
        int proto = tree.prototypeIndex;
        int tid = terrain.GetInstanceID();

        unchecked
        {
            int key = 17;
            key = key * 31 + tid;
            key = key * 31 + proto;
            key = key * 31 + hx;
            key = key * 31 + hz;
            return key;
        }
    }

    private static bool TryFindNearestTreeInstance(Terrain terrain, Vector3 worldPoint, float radius, out int bestIndex, out TreeInstance bestTree)
    {
        TerrainData td = terrain.terrainData;
        TreeInstance[] trees = td.treeInstances;

        bestIndex = -1;
        bestTree = default;

        if (trees == null || trees.Length == 0) return false;

        Vector3 terrainPos = terrain.transform.position;
        Vector3 size = td.size;

        float bestDistSqr = float.MaxValue;

        for (int i = 0; i < trees.Length; i++)
        {
            Vector3 wp = new Vector3(
                terrainPos.x + trees[i].position.x * size.x,
                terrainPos.y + trees[i].position.y * size.y,
                terrainPos.z + trees[i].position.z * size.z
            );

            float d = (wp - worldPoint).sqrMagnitude;
            if (d < bestDistSqr)
            {
                bestDistSqr = d;
                bestIndex = i;
                bestTree = trees[i];
            }
        }

        return bestIndex >= 0 && bestDistSqr <= radius * radius;
    }

    // IMPORTANT: no Flush() here — we do it AFTER yielding in the coroutine
    private static void RemoveTreeInstance_NoFlush(Terrain terrain, int index)
    {
        TerrainData td = terrain.terrainData;
        var list = new List<TreeInstance>(td.treeInstances);
        if (index < 0 || index >= list.Count) return;

        list.RemoveAt(index);
        td.treeInstances = list.ToArray();
    }
}
