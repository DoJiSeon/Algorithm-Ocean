using System;
using UnityEngine;

namespace AlgorithmOcean.ShortsPlayer
{
    public sealed class YouTubeShortsJsonLoader : MonoBehaviour
    {
        [SerializeField] private YouTubeShortsPlayer player;
        [SerializeField] private TextAsset shortsJson;
        [SerializeField] private string resourcesPath = "shorts_links";
        [SerializeField] private int startIndex;
        [SerializeField] private bool autoplay = true;
        [SerializeField] private bool loadOnStart = true;

        private ShortsVideoList videoList;
        private int currentIndex;

        public int Count => videoList?.items?.Length ?? 0;

        private void Reset()
        {
            player = GetComponent<YouTubeShortsPlayer>();
        }

        private void Awake()
        {
            if (player == null)
            {
                player = GetComponent<YouTubeShortsPlayer>();
            }
        }

        private void Start()
        {
            if (loadOnStart)
            {
                LoadIndex(startIndex);
            }
        }

        public void Configure(YouTubeShortsPlayer targetPlayer, TextAsset targetJson = null)
        {
            player = targetPlayer;
            shortsJson = targetJson;
        }

        public void LoadIndex(int index)
        {
            EnsureLoaded();

            if (Count == 0)
            {
                Debug.LogWarning("No YouTube Shorts entries were found in the configured JSON.", this);
                return;
            }

            currentIndex = Mathf.Clamp(index, 0, Count - 1);
            string videoOrUrl = videoList.items[currentIndex].GetVideoOrUrl();

            if (string.IsNullOrEmpty(videoOrUrl))
            {
                Debug.LogWarning($"Shorts entry at index {currentIndex} has no url or videoId.", this);
                return;
            }

            player.Load(videoOrUrl, autoplay);
        }

        public void LoadNext()
        {
            if (Count == 0)
            {
                LoadIndex(0);
                return;
            }

            LoadIndex((currentIndex + 1) % Count);
        }

        public void LoadPrevious()
        {
            if (Count == 0)
            {
                LoadIndex(0);
                return;
            }

            LoadIndex((currentIndex - 1 + Count) % Count);
        }

        private void EnsureLoaded()
        {
            if (videoList != null)
            {
                return;
            }

            TextAsset jsonAsset = shortsJson;
            if (jsonAsset == null && !string.IsNullOrWhiteSpace(resourcesPath))
            {
                jsonAsset = Resources.Load<TextAsset>(resourcesPath);
            }

            if (jsonAsset == null)
            {
                videoList = new ShortsVideoList();
                Debug.LogWarning($"Shorts JSON was not found. Resources path: {resourcesPath}", this);
                return;
            }

            try
            {
                videoList = JsonUtility.FromJson<ShortsVideoList>(jsonAsset.text) ?? new ShortsVideoList();
            }
            catch (Exception exception)
            {
                videoList = new ShortsVideoList();
                Debug.LogWarning($"Failed to parse Shorts JSON: {exception.Message}", this);
            }
        }

        [Serializable]
        private sealed class ShortsVideoList
        {
            public ShortsVideoEntry[] items = Array.Empty<ShortsVideoEntry>();
        }

        [Serializable]
        private sealed class ShortsVideoEntry
        {
            public string id;
            public string title;
            public string videoId;
            public string url;

            public string GetVideoOrUrl()
            {
                return string.IsNullOrWhiteSpace(videoId) ? url : videoId;
            }
        }
    }
}
