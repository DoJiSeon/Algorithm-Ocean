using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlgorithmOcean.Dohyeon;

public class BottleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private Transform target;
    [SerializeField] private ShortsContentRepository contentRepository;
    [SerializeField] private ShortsPlaybackUI playbackUI;
    [SerializeField] private int spawnCount = 8;
    [SerializeField] private float spawnRadiusMin = 8f;
    [SerializeField] private float spawnRadiusMax = 20f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float despawnDistance = 35f;

    private readonly List<BottleInteractable> activeBottles = new();

    private IEnumerator Start()
    {
        if (!CanSpawn())
        {
            yield break;
        }

        while (!contentRepository.IsLoaded)
        {
            yield return null;
        }

        RefillBottles();
    }

    private void Update()
    {
        if (target == null) return;

        for (int i = activeBottles.Count - 1; i >= 0; i--)
        {
            var bottle = activeBottles[i];
            if (bottle == null || !bottle.gameObject.activeSelf)
            {
                activeBottles.RemoveAt(i);
                continue;
            }

            float distance = Vector3.Distance(bottle.transform.position, target.position);
            if (distance > despawnDistance)
            {
                Destroy(bottle.gameObject);
                activeBottles.RemoveAt(i);
            }
        }

        RefillBottles();
    }

    private void RefillBottles()
    {
        List<SubmitData> filteredContents = GetSpawnableContents();
        if (filteredContents.Count == 0)
        {
            return;
        }

        int targetBottleCount = Mathf.Min(spawnCount, filteredContents.Count);
        while (activeBottles.Count < targetBottleCount)
        {
            SpawnOne(filteredContents);
        }
    }

    private void SpawnOne(List<SubmitData> filteredContents)
    {
        if (!TryFindSpawnPosition(out Vector3 position))
        {
            Debug.LogWarning("[Spawner] Could not find a valid bottle spawn position.", this);
            return;
        }

        SubmitData content = PickUnusedContent(filteredContents);
        if (content == null)
        {
            return;
        }

        var bottle = Instantiate(bottlePrefab, position, Quaternion.identity, transform);
        var interactable = bottle.GetComponent<BottleInteractable>();

        if (interactable == null)
        {
            Debug.LogWarning("[Spawner] Bottle prefab has no BottleInteractable component.", bottle);
            Destroy(bottle);
            return;
        }

        interactable.Initialize(content, playbackUI, target);
        interactable.OnPicked.AddListener(OnBottlePicked);
        activeBottles.Add(interactable);
    }

    private List<SubmitData> GetSpawnableContents()
    {
        if (!CanSpawn())
        {
            return new List<SubmitData>();
        }

        List<SubmitData> filteredContents = contentRepository.GetFilteredContents();
        if (filteredContents.Count == 0)
        {
            Debug.LogWarning("[Spawner] No shorts content available after preferred genre filtering.", this);
        }

        return filteredContents;
    }

    private SubmitData PickUnusedContent(List<SubmitData> filteredContents)
    {
        var candidates = new List<SubmitData>(filteredContents);
        foreach (BottleInteractable bottle in activeBottles)
        {
            if (bottle == null) continue;
            candidates.Remove(bottle.ContentData);
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    private bool TryFindSpawnPosition(out Vector3 position)
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(spawnRadiusMin, spawnRadiusMax);
            Vector3 candidate = target.position + new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            if (!IsTooClose(candidate))
            {
                position = candidate;
                return true;
            }
        }

        position = Vector3.zero;
        return false;
    }

    private bool IsTooClose(Vector3 position)
    {
        foreach (var bottle in activeBottles)
        {
            if (bottle == null) continue;
            if (Vector3.Distance(position, bottle.transform.position) < minDistance) return true;
        }

        return false;
    }

    private void OnBottlePicked(string shortsUrl)
    {
        Debug.Log($"[Spawner] Picked shorts URL: {shortsUrl}");
    }

    private bool CanSpawn()
    {
        if (target == null)
        {
            Debug.LogWarning("[Spawner] Target is missing. Assign the Boat transform to BottleSpawner.target.", this);
            return false;
        }

        if (bottlePrefab == null)
        {
            Debug.LogWarning("[Spawner] Bottle prefab is missing. Assign the existing bottle prefab to BottleSpawner.bottlePrefab.", this);
            return false;
        }

        if (contentRepository == null)
        {
            Debug.LogWarning("[Spawner] Content repository is missing. Assign a ShortsContentRepository object to BottleSpawner.contentRepository.", this);
            return false;
        }

        return true;
    }
}
