using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("User Preference Toggles")]
    [SerializeField] private Toggle[] preferenceToggles;

    [Header("Saved User Preference Categories")]
    [SerializeField] private List<string> selectedPreferenceCategories = new List<string>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (Toggle toggle in preferenceToggles)
        {
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(_ => SavePreferenceCategories());
            }
        }

        SavePreferenceCategories();
    }

    public void SavePreferenceCategories()
    {
        selectedPreferenceCategories.Clear();

        foreach (Toggle toggle in preferenceToggles)
        {
            if (toggle == null) continue;

            if (toggle.isOn)
            {
                Text label = toggle.GetComponentInChildren<Text>();

                if (label != null)
                {
                    string category = label.text.Trim();

                    if (!string.IsNullOrEmpty(category))
                    {
                        selectedPreferenceCategories.Add(category);
                    }
                }
            }
        }

        Debug.Log("현재 선택된 카테고리: " + string.Join(", ", selectedPreferenceCategories));
    }

    public List<string> GetSelectedPreferenceCategories()
    {
        return selectedPreferenceCategories;
    }
}