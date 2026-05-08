using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class SubmitData
{
    public string userId;
    public string category;
    public string youtube;
}

[Serializable]
public class CategoryToggle
{
    public Toggle toggle;
    public string categoryName;
}

public class FirebaseRestManager : MonoBehaviour
{
    [Header("Firebase Realtime Database URL")]
    [SerializeField]
    private string databaseUrl = "https://alogorithm-ocean-default-rtdb.firebaseio.com/";

    [Header("Category Toggles")]
    [SerializeField]
    private CategoryToggle[] categoryToggles;

    [Header("YouTube Input")]
    [SerializeField]
    private TMP_InputField youtubeInputField;

    private string userId;
    private readonly List<SubmitData> cachedSubmissions = new();

    public IReadOnlyList<SubmitData> CachedSubmissions => cachedSubmissions;

    void Start()
    {
        if (PlayerPrefs.HasKey("userId"))
        {
            userId = PlayerPrefs.GetString("userId");
        }
        else
        {
            userId = "user_" + Guid.NewGuid().ToString();
            PlayerPrefs.SetString("userId", userId);
            PlayerPrefs.Save();
        }

        Debug.Log("현재 유저 ID: " + userId);
    }

    public void Submit()
    {
        string selectedCategory = GetSelectedCategory();
        string youtubeValue = youtubeInputField.text.Trim();

        if (string.IsNullOrEmpty(selectedCategory))
        {
            Debug.LogWarning("카테고리를 선택해야 합니다.");
            return;
        }

        if (string.IsNullOrEmpty(youtubeValue))
        {
            Debug.LogWarning("YouTube videoId 또는 영상 링크를 입력해야 합니다.");
            return;
        }

        SubmitData data = new SubmitData
        {
            userId = userId,
            category = selectedCategory,
            youtube = youtubeValue
        };

        StartCoroutine(SubmitCoroutine(data));
    }

    private string GetSelectedCategory()
    {
        foreach (CategoryToggle item in categoryToggles)
        {
            if (item.toggle != null && item.toggle.isOn)
            {
                return item.categoryName;
            }
        }

        return "";
    }

    private IEnumerator SubmitCoroutine(SubmitData data)
    {
        string json = JsonUtility.ToJson(data);

        // submissions/{자동생성ID}.json 형태로 저장됨
        // userId는 데이터 안에 같이 저장
        string url = databaseUrl + "submissions.json";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("제출 성공!");
            Debug.Log("Firebase 응답: " + request.downloadHandler.text);

            ClearInput();
        }
        else
        {
            Debug.LogError("제출 실패: " + request.error);
            Debug.LogError(request.downloadHandler.text);
        }
    }

    public IEnumerator FetchSubmissionsCoroutine(Action<List<SubmitData>> onCompleted)
    {
        string url = databaseUrl + "submissions.json";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Submissions fetch failed: " + request.error);
            Debug.LogError(request.downloadHandler.text);
            onCompleted?.Invoke(new List<SubmitData>());
            yield break;
        }

        List<SubmitData> submissions = ParseSubmissions(request.downloadHandler.text);

        cachedSubmissions.Clear();
        cachedSubmissions.AddRange(submissions);

        onCompleted?.Invoke(new List<SubmitData>(cachedSubmissions));
    }

    private static List<SubmitData> ParseSubmissions(string json)
    {
        var submissions = new List<SubmitData>();
        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return submissions;
        }

        JObject root = JObject.Parse(json);
        foreach (JProperty property in root.Properties())
        {
            SubmitData data = property.Value.ToObject<SubmitData>();
            if (data != null && !string.IsNullOrWhiteSpace(data.youtube))
            {
                submissions.Add(data);
            }
        }

        return submissions;
    }

    private void ClearInput()
    {
        // 입력창 비우기
        youtubeInputField.text = "";

        // 카테고리 토글 전부 끄기
        foreach (CategoryToggle item in categoryToggles)
        {
            if (item.toggle != null)
            {
                item.toggle.SetIsOnWithoutNotify(false);
            }
        }
    }
}
