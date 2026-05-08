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
        private bool isOpening;

        private void Awake()
        {
            if (root != null && !isOpening)
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
                isOpening = true;
                root.SetActive(true);
                isOpening = false;
            }

            onShortsUrlReceived?.Invoke(shortsUrl);

            if (shortsPlayer != null)
            {
                shortsPlayer.SetVisible(true);
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
