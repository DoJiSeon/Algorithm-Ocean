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
        private bool isPlaying;

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

            isPlaying = true;
            SoundManager.Instance?.PauseBgm();
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
            StopPlayback();

            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (!isOpening)
            {
                StopPlayback();
            }
        }

        private void StopPlayback()
        {
            if (!isPlaying)
            {
                return;
            }

            isPlaying = false;

            if (shortsPlayer != null)
            {
                shortsPlayer.Stop();
                shortsPlayer.SetVisible(false);
            }

            SoundManager.Instance?.ResumeBgm();
        }
    }
}
