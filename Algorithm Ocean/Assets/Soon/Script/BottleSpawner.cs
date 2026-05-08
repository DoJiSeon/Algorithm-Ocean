using System.Collections.Generic;
using UnityEngine;

public class BottleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private int spawnCount = 8;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float minDistance = 3f;

    // TODO: GameManager.Instance.GetFilteredShorts()로 교체 예정
    [SerializeField]
    private string[] dummyPool = {
        "sample1",
        "sample2",
        "sample3",
        "sample4",
        "sample5"
    };

    private void Start()
    {
        SpawnBottles(dummyPool);
    }

    private void SpawnBottles(string[] pool)
    {
        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning("[Spawner] No data available");
            return;
        }

        var positions = new List<Vector3>();
        int attempts = 0;

        while (positions.Count < spawnCount && attempts < 100)
        {
            attempts++;
            Vector2 r = Random.insideUnitCircle * spawnRadius;
            Vector3 p = new Vector3(r.x, 0, r.y);

            if (IsTooClose(p, positions)) continue;
            positions.Add(p);

            var data = pool[Random.Range(0, pool.Length)];
            var bottle = Instantiate(bottlePrefab, p, Quaternion.identity, transform);
            var interactable = bottle.GetComponent<BottleInteractable>();
            interactable.Initialize(data);
            interactable.OnPicked.AddListener(OnBottlePicked);
        }
    }

    private bool IsTooClose(Vector3 p, List<Vector3> existing)
    {
        foreach (var pos in existing)
            if (Vector3.Distance(p, pos) < minDistance) return true;
        return false;
    }

    private void OnBottlePicked(string data)
    {
        Debug.Log($"[Spawner] Bottle picked with data: {data}");
        // TODO: FindObjectOfType<ShortsPlayer>()?.Open(data);
    }
}