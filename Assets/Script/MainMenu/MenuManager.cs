using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Dungeonlicious.Assets.Script;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string _playSceneName;
    [SerializeField] private TMP_InputField seedText;
    [SerializeField] private Image background;
    [SerializeField] private Sprite rooster;
    [SerializeField] private TMP_Dropdown drop;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "VictoryCutscene")
        {
            DestroyAllDontDestroyOnLoad();
        }

        if (drop != null)
            drop.onValueChanged.AddListener(ChangeResolution);

        Time.timeScale = 1f;
    }

    public void DestroyAllDontDestroyOnLoad()
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

    public void ChangeResolution(int index)
    {
        background.sprite = rooster;
        switch (index)
        {
            case 0:
                Screen.SetResolution(1920, 1080, true);
                break;

            case 1:
                Screen.SetResolution(3840, 2160, true);
                break;

            case 2:
                Screen.SetResolution(640, 480, true);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeBackground()
    {
        background.sprite = rooster;
    }

    public void Play()
    {
        string seedValue = (seedText != null && !string.IsNullOrEmpty(seedText.text))
            ? seedText.text
            : Environment.TickCount.ToString();

        if (SeedKeeper.Instance != null)
            SeedKeeper.Instance.SetSeed(seedValue);

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
