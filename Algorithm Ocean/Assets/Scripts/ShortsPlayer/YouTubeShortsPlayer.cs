using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

namespace AlgorithmOcean.ShortsPlayer
{
    public sealed class YouTubeShortsPlayer : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private RectTransform playerRect;
        [SerializeField] private Canvas canvas;

        [Header("Playback")]
        [SerializeField] private string initialVideoOrUrl;
        [SerializeField] private bool playOnStart;
        [SerializeField] private bool muted = true;
        [SerializeField] private bool controls = true;
        [SerializeField] private bool loop;
        [SerializeField] private bool visibleOnEnable = true;

        [Header("Events")]
        public UnityEvent onReady;
        public UnityEvent<string> onStateChanged;
        public UnityEvent<string> onError;

        private readonly Vector3[] worldCorners = new Vector3[4];
        private Rect lastNormalizedRect;
        private bool created;
        private bool hasRect;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void AOYT_Create(string objectName, int controls, int muted, int loop);
        [DllImport("__Internal")] private static extern void AOYT_LoadVideo(string objectName, string videoId, int autoplay);
        [DllImport("__Internal")] private static extern void AOYT_SetRect(string objectName, float x, float y, float width, float height);
        [DllImport("__Internal")] private static extern void AOYT_Play(string objectName);
        [DllImport("__Internal")] private static extern void AOYT_Pause(string objectName);
        [DllImport("__Internal")] private static extern void AOYT_Stop(string objectName);
        [DllImport("__Internal")] private static extern void AOYT_SetVisible(string objectName, int visible);
        [DllImport("__Internal")] private static extern void AOYT_Destroy(string objectName);
#endif

        public string CurrentVideoId { get; private set; }

        public void Configure(RectTransform targetRect, Canvas targetCanvas)
        {
            playerRect = targetRect;
            canvas = targetCanvas;
            UpdatePlayerRect(true);
        }

        private void Reset()
        {
            playerRect = transform as RectTransform;
            canvas = GetComponentInParent<Canvas>();
        }

        private void Awake()
        {
            if (playerRect == null)
            {
                playerRect = transform as RectTransform;
            }

            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }
        }

        private void OnEnable()
        {
            CreatePlayerIfNeeded();
            SetVisible(visibleOnEnable);
            UpdatePlayerRect(true);
        }

        private void Start()
        {
            if (!string.IsNullOrWhiteSpace(initialVideoOrUrl))
            {
                Load(initialVideoOrUrl, playOnStart);
            }
        }

        private void LateUpdate()
        {
            UpdatePlayerRect(false);
        }

        private void OnDisable()
        {
            SetVisible(false);
        }

        private void OnDestroy()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (created)
            {
                AOYT_Destroy(name);
            }
#endif
            created = false;
        }

        public void Load(string videoOrUrl, bool autoplay = true)
        {
            string videoId = ExtractVideoId(videoOrUrl);
            if (string.IsNullOrEmpty(videoId))
            {
                HandleError($"Invalid YouTube Shorts link or video id: {videoOrUrl}");
                return;
            }

            CurrentVideoId = videoId;
            CreatePlayerIfNeeded();
            UpdatePlayerRect(true);

#if UNITY_WEBGL && !UNITY_EDITOR
            AOYT_LoadVideo(name, videoId, autoplay ? 1 : 0);
#else
            Debug.Log($"YouTubeShortsPlayer would load '{videoId}' in a WebGL build.");
            OnYouTubePlayerReady();
            OnYouTubePlayerState(autoplay ? "playing" : "cued");
#endif
        }

        public void Play()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            AOYT_Play(name);
#else
            OnYouTubePlayerState("playing");
#endif
        }

        public void Pause()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            AOYT_Pause(name);
#else
            OnYouTubePlayerState("paused");
#endif
        }

        public void Stop()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            AOYT_Stop(name);
#else
            OnYouTubePlayerState("stopped");
#endif
        }

        public void SetVisible(bool visible)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (created)
            {
                AOYT_SetVisible(name, visible ? 1 : 0);
            }
#endif
        }

        public static string ExtractVideoId(string videoOrUrl)
        {
            if (string.IsNullOrWhiteSpace(videoOrUrl))
            {
                return string.Empty;
            }

            string value = videoOrUrl.Trim();
            if (IsLikelyVideoId(value))
            {
                return value;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
            {
                return string.Empty;
            }

            string[] segments = uri.AbsolutePath.Trim('/').Split('/');
            for (int i = 0; i < segments.Length - 1; i++)
            {
                if (segments[i].Equals("shorts", StringComparison.OrdinalIgnoreCase) ||
                    segments[i].Equals("embed", StringComparison.OrdinalIgnoreCase))
                {
                    return SanitizeVideoId(segments[i + 1]);
                }
            }

            if (uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase) && segments.Length > 0)
            {
                return SanitizeVideoId(segments[0]);
            }

            string query = uri.Query.TrimStart('?');
            foreach (string pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = pair.Split('=', 2);
                if (parts.Length == 2 && parts[0] == "v")
                {
                    return SanitizeVideoId(Uri.UnescapeDataString(parts[1]));
                }
            }

            return string.Empty;
        }

        private void CreatePlayerIfNeeded()
        {
            if (created)
            {
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            AOYT_Create(name, controls ? 1 : 0, muted ? 1 : 0, loop ? 1 : 0);
#endif
            created = true;
        }

        private void UpdatePlayerRect(bool force)
        {
            if (playerRect == null)
            {
                return;
            }

            Rect normalizedRect = GetNormalizedRect();
            if (!force && hasRect && Approximately(lastNormalizedRect, normalizedRect))
            {
                return;
            }

            lastNormalizedRect = normalizedRect;
            hasRect = true;

#if UNITY_WEBGL && !UNITY_EDITOR
            if (created)
            {
                AOYT_SetRect(name, normalizedRect.x, normalizedRect.y, normalizedRect.width, normalizedRect.height);
            }
#endif
        }

        private Rect GetNormalizedRect()
        {
            Camera cameraForCanvas = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cameraForCanvas = canvas.worldCamera;
            }

            playerRect.GetWorldCorners(worldCorners);

            Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(cameraForCanvas, worldCorners[0]);
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(cameraForCanvas, worldCorners[2]);

            float x = Mathf.Clamp01(bottomLeft.x / Screen.width);
            float y = Mathf.Clamp01(1f - topRight.y / Screen.height);
            float width = Mathf.Clamp01((topRight.x - bottomLeft.x) / Screen.width);
            float height = Mathf.Clamp01((topRight.y - bottomLeft.y) / Screen.height);

            return new Rect(x, y, width, height);
        }

        private static bool Approximately(Rect a, Rect b)
        {
            const float tolerance = 0.0005f;
            return Mathf.Abs(a.x - b.x) < tolerance &&
                   Mathf.Abs(a.y - b.y) < tolerance &&
                   Mathf.Abs(a.width - b.width) < tolerance &&
                   Mathf.Abs(a.height - b.height) < tolerance;
        }

        private static bool IsLikelyVideoId(string value)
        {
            if (value.Length != 11)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                {
                    return false;
                }
            }

            return true;
        }

        private static string SanitizeVideoId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            int end = value.IndexOfAny(new[] { '?', '&', '#', '/' });
            string candidate = end >= 0 ? value[..end] : value;
            return IsLikelyVideoId(candidate) ? candidate : string.Empty;
        }

        private void HandleError(string message)
        {
            Debug.LogWarning(message, this);
            onError?.Invoke(message);
        }

        public void OnYouTubePlayerReady()
        {
            onReady?.Invoke();
        }

        public void OnYouTubePlayerState(string state)
        {
            onStateChanged?.Invoke(state);
        }

        public void OnYouTubePlayerError(string error)
        {
            HandleError(error);
        }
    }
}
