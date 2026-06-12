using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string _playSceneName;
    [SerializeField] private TMP_InputField seedText;
    private SeedKeeper seedKeeper;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        seedKeeper = FindFirstObjectByType<SeedKeeper>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Play()
    {
        if (seedText != null && !string.IsNullOrEmpty(seedText.text))
        {
            seedKeeper.SetSeed(seedText.text);
        }
        else
        {
            seedKeeper.SetSeed(Environment.TickCount.ToString());
        }
        SceneManager.LoadScene(_playSceneName);
    }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif  
    }
}
