using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Dungeonlicious.Assets.Script;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string _playSceneName;
    [SerializeField] private TMP_InputField seedText;
    private SeedKeeper seedKeeper;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Main Menu")
        {
            DestroyAllDontDestroyOnLoad();
        }

        Time.timeScale = 1f;
        seedKeeper = FindFirstObjectByType<SeedKeeper>();
    }

    public static void DestroyAllDontDestroyOnLoad()
    {
        GameObject temp = new GameObject();
        DontDestroyOnLoad(temp);

        Scene dontDestroyScene = temp.scene;

        GameObject[] rootObjects = dontDestroyScene.GetRootGameObjects();

        foreach (GameObject obj in rootObjects)
        {
            Destroy(obj);
        }
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
        SaveManager.Instance.DeleteSave();
        SceneManager.LoadScene(_playSceneName);
    }

    public void PlayWithSave()
    {
        PendingSeed.UseExisting = true;
        SceneManager.LoadScene(_playSceneName);
    }

    public void MainMenu()
    {
        DestroyAllDontDestroyOnLoad();
        SceneManager.LoadScene("Main Menu");
    }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif  
    }
}
