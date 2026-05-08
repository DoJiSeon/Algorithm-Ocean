# WebGL YouTube Shorts Player

This player renders a YouTube IFrame Player as an HTML overlay on top of the Unity WebGL canvas.

## Quick test

Open `Assets/Scenes/SampleScene.unity` and enter Play Mode. The `Main Camera` has a `YouTubeShortsSampleBootstrap` component that creates a portrait player UI and loads the first entry from `Assets/Resources/shorts_links.json`.

In the Unity Editor, the player only reports simulated state changes. Build and run as WebGL to see the real YouTube iframe.

## JSON format

Put the playback list in `Assets/Resources/shorts_links.json`:

```json
{
  "items": [
    {
      "id": "sample-first-video",
      "title": "Sample Shorts Entry",
      "videoId": "jNQXAC9IVRw",
      "url": "https://www.youtube.com/shorts/jNQXAC9IVRw"
    }
  ]
}
```

`videoId` is preferred when present. `url` can be a Shorts, watch, or youtu.be link.

## Runtime API

Add `YouTubeShortsPlayer` to the RectTransform that should host the overlay, then call:

```csharp
player.Load("https://www.youtube.com/shorts/VIDEO_ID", true);
player.Load("VIDEO_ID", true);
player.Pause();
player.Play();
player.Stop();
```

For JSON-driven playback, add `YouTubeShortsJsonLoader` beside the player and call:

```csharp
loader.LoadIndex(0);
loader.LoadNext();
loader.LoadPrevious();
```

Supported link formats:

- `https://www.youtube.com/shorts/VIDEO_ID`
- `https://www.youtube.com/watch?v=VIDEO_ID`
- `https://youtu.be/VIDEO_ID`
- raw 11-character video IDs

## Notes

- The iframe is a DOM overlay, not a RenderTexture. It cannot be placed on a 3D mesh.
- Autoplay can be blocked by the browser unless muted or started from user input.
- This uses the official YouTube IFrame API and does not extract direct media URLs.
