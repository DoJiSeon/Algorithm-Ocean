using AlgorithmOcean.ShortsPlayer;
using UnityEngine;
using UnityEngine.Events;

namespace AlgorithmOcean.Dohyeon
{
    public sealed class ShortsPlaybackUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private YouTubeShortsPlayer shortsPlayer;

        public UnityEvent<string> onShortsUrlReceived;

        private void Awake()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        public void Open(string shortsUrl)
        {
            if (string.IsNullOrWhiteSpace(shortsUrl))
            {
                Debug.LogWarning("[ShortsPlaybackUI] Shorts URL is empty.", this);
                return;
            }

            if (root != null)
            {
                root.SetActive(true);
            }

            onShortsUrlReceived?.Invoke(shortsUrl);

            if (shortsPlayer != null)
            {
                shortsPlayer.Load(shortsUrl, true);
            }
            else
            {
                Debug.Log($"[ShortsPlaybackUI] Received shorts URL: {shortsUrl}", this);
            }
        }

        public void Close()
        {
            if (shortsPlayer != null)
            {
                shortsPlayer.Stop();
            }

            if (root != null)
            {
                root.SetActive(false);
            }
        }
    }
}
