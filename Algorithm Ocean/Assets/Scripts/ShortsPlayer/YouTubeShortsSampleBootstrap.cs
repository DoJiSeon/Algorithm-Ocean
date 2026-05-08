using UnityEngine;
using UnityEngine.UI;

namespace AlgorithmOcean.ShortsPlayer
{
    public sealed class YouTubeShortsSampleBootstrap : MonoBehaviour
    {
        [SerializeField] private bool createOnStart = true;

        private Font defaultFont;

        private void Start()
        {
            if (createOnStart)
            {
                CreateSamplePlayer();
            }
        }

        [ContextMenu("Create Sample Player")]
        public void CreateSamplePlayer()
        {
            if (FindFirstObjectByType<YouTubeShortsPlayer>() != null)
            {
                return;
            }

            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Canvas canvas = CreateCanvas();
            RectTransform root = canvas.GetComponent<RectTransform>();

            Image ocean = CreatePanel("Ocean Backdrop", root, new Color(0.04f, 0.18f, 0.24f, 1f));
            Stretch(ocean.rectTransform);

            RectTransform playerFrame = CreatePanel("YouTube Shorts Player", root, Color.black).rectTransform;
            Anchor(playerFrame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360f, 640f));

            Text placeholder = CreateText("Editor Placeholder", playerFrame, "WebGL YouTube Player", 24, TextAnchor.MiddleCenter);
            Stretch(placeholder.rectTransform);

            YouTubeShortsPlayer player = playerFrame.gameObject.AddComponent<YouTubeShortsPlayer>();
            player.Configure(playerFrame, canvas);

            YouTubeShortsJsonLoader loader = playerFrame.gameObject.AddComponent<YouTubeShortsJsonLoader>();
            loader.Configure(player);
        }

        private Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Shorts Player Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private Image CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private Text CreateText(string name, Transform parent, string value, int size, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = defaultFont;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }

        private static void Stretch(RectTransform rectTransform, float horizontalPadding = 0f, float verticalPadding = 0f)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(horizontalPadding, verticalPadding);
            rectTransform.offsetMax = new Vector2(-horizontalPadding, -verticalPadding);
        }

        private static void Anchor(RectTransform rectTransform, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
        {
            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
        }
    }
}
