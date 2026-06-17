using UnityEngine;
using UnityEngine.SceneManagement;

public class SeedKeeper : MonoBehaviour
{
    private static SeedKeeper _instance;
    public static SeedKeeper Instance => _instance;
    public string Seed { get; set; }
    public bool IsInfinite { get; set; }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "Main Menu")
        {
            MenuManager menu = FindFirstObjectByType<MenuManager>();
            menu.DestroyAllDontDestroyOnLoad();
        }

        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSeed(string value)
    {
        Seed = value;
    }
}