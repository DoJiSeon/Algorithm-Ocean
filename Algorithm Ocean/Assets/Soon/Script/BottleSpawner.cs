using System.Collections.Generic;
using UnityEngine;

public class BottleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private Transform target;          // 추가: Boat
    [SerializeField] private int spawnCount = 8;
    [SerializeField] private float spawnRadiusMin = 8f; // 추가: 너무 가깝지 않게
    [SerializeField] private float spawnRadiusMax = 20f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float despawnDistance = 35f; // 추가: 멀어진 병 정리

    // TODO: 나중에 ShortsData 풀로 교체
    [SerializeField]
    private string[] dummyPool = {
        "sample1", "sample2", "sample3", "sample4", "sample5"
    };

    private List<BottleInteractable> activeBottles = new();

    private void Start()
    {
        for (int i = 0; i < spawnCount; i++) SpawnOne();
    }

    private void Update()
    {
        // 배에서 멀어진 병 정리 + 재스폰
        if (target == null) return;

        for (int i = activeBottles.Count - 1; i >= 0; i--)
        {
            var b = activeBottles[i];
            if (b == null) { activeBottles.RemoveAt(i); SpawnOne(); continue; }

            float dist = Vector3.Distance(b.transform.position, target.position);
            if (dist > despawnDistance)
            {
                Destroy(b.gameObject);
                activeBottles.RemoveAt(i);
                SpawnOne();
            }
        }
    }

    private void SpawnOne()
    {
        if (target == null || dummyPool.Length == 0) return;

        Vector3 pos = FindSpawnPosition();
        if (pos == Vector3.zero) return;

        var data = dummyPool[Random.Range(0, dummyPool.Length)];
        var bottle = Instantiate(bottlePrefab, pos, Quaternion.identity, transform);
        var interactable = bottle.GetComponent<BottleInteractable>();
        interactable.Initialize(data);
        interactable.OnPicked.AddListener(OnBottlePicked);
        activeBottles.Add(interactable);
    }

    private Vector3 FindSpawnPosition()
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            // 배 주변 링 형태로 스폰 (배 바로 옆은 X)
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(spawnRadiusMin, spawnRadiusMax);
            Vector3 candidate = target.position + new Vector3(
                Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius
            );

            if (!IsTooClose(candidate)) return candidate;
        }
        return Vector3.zero;
    }

    private bool IsTooClose(Vector3 p)
    {
        foreach (var b in activeBottles)
        {
            if (b == null) continue;
            if (Vector3.Distance(p, b.transform.position) < minDistance) return true;
        }
        return false;
    }

    private void OnBottlePicked(string data)
    {
        // TODO: ShortsPlayer.Open(data)로 교체
        Debug.Log($"[Spawner] Picked: {data}");
        // 잡힌 병은 Update에서 자동 정리됨 (gameObject.SetActive(false) 됨)
    }
}