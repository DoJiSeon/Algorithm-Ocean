using System.Collections.Generic;
using AlgorithmOcean.ShortsPlayer;
using UnityEngine;

namespace AlgorithmOcean.Dohyeon
{
    public sealed class ShortsContentRepository : MonoBehaviour
    {
        [SerializeField] private global::GameManager gameManager;
        [SerializeField] private FirebaseRestManager firebaseRestManager;
        [SerializeField] private bool fetchFromFirebaseOnStart = true;
        [SerializeField] private List<SubmitData> fallbackContents = new();

        private readonly List<SubmitData> firebaseContents = new();
        private readonly HashSet<string> viewedShortsKeys = new();

        public IReadOnlyList<SubmitData> FallbackContents => fallbackContents;
        public IReadOnlyList<SubmitData> FirebaseContents => firebaseContents;
        public bool IsLoaded { get; private set; }

        private void Start()
        {
            if (fetchFromFirebaseOnStart)
            {
                RefreshFromFirebase();
            }
            else
            {
                IsLoaded = true;
            }
        }

        public void RefreshFromFirebase()
        {
            if (firebaseRestManager == null)
            {
                Debug.LogWarning("[ShortsContentRepository] FirebaseRestManager is missing. Fallback Contents will be used.", this);
                IsLoaded = true;
                return;
            }

            IsLoaded = false;
            StartCoroutine(firebaseRestManager.FetchSubmissionsCoroutine(OnFirebaseContentsLoaded));
        }

        public List<SubmitData> GetFilteredContents()
        {
            List<SubmitData> sourceContents = firebaseContents.Count > 0
                ? firebaseContents
                : fallbackContents;

            var filteredContents = new List<SubmitData>();

            if (sourceContents.Count == 0)
            {
                Debug.LogWarning("[ShortsContentRepository] No SubmitData contents found. Check Firebase submissions or Fallback Contents.", this);
                return filteredContents;
            }

            foreach (SubmitData content in sourceContents)
            {
                if (content == null || string.IsNullOrWhiteSpace(content.youtube))
                {
                    continue;
                }

                if (IsViewedContent(content))
                {
                    continue;
                }

                if (IsPreferredCategory(content))
                {
                    continue;
                }

                filteredContents.Add(content);
            }

            if (filteredContents.Count == 0)
            {
                Debug.LogWarning("[ShortsContentRepository] Every SubmitData entry was removed by the preferred genre filter or has an empty youtube value.", this);
            }

            return filteredContents;
        }

        public SubmitData GetRandomFilteredContent()
        {
            List<SubmitData> filteredContents = GetFilteredContents();
            if (filteredContents.Count == 0)
            {
                Debug.LogWarning("[ShortsContentRepository] No shorts URL remains after genre filtering.", this);
                return null;
            }

            return filteredContents[Random.Range(0, filteredContents.Count)];
        }

        public void MarkViewed(string shortsUrl)
        {
            string key = GetShortsKey(shortsUrl);
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            viewedShortsKeys.Add(key);
        }

        public bool IsViewed(string shortsUrl)
        {
            string key = GetShortsKey(shortsUrl);
            return !string.IsNullOrEmpty(key) && viewedShortsKeys.Contains(key);
        }

        public void ClearViewed()
        {
            viewedShortsKeys.Clear();
        }

        private void OnFirebaseContentsLoaded(List<SubmitData> loadedContents)
        {
            firebaseContents.Clear();
            if (loadedContents != null)
            {
                firebaseContents.AddRange(loadedContents);
            }

            IsLoaded = true;
            Debug.Log($"[ShortsContentRepository] Loaded {firebaseContents.Count} SubmitData entries from Firebase.", this);
        }

        private bool IsPreferredCategory(SubmitData content)
        {
            global::GameManager preferenceSource = gameManager != null
                ? gameManager
                : global::GameManager.Instance;

            if (preferenceSource == null || content.categories == null)
            {
                return false;
            }

            foreach (string category in content.categories)
            {
                if (preferenceSource.ContainsSelectedPreferenceCategory(category))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsViewedContent(SubmitData content)
        {
            return content != null && IsViewed(content.youtube);
        }

        private static string GetShortsKey(string shortsUrl)
        {
            string videoId = YouTubeShortsPlayer.ExtractVideoId(shortsUrl);
            return string.IsNullOrEmpty(videoId) ? shortsUrl?.Trim() : videoId;
        }
    }
}
