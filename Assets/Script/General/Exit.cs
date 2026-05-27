namespace Dungeonlicious.Assets.Script
{
using UnityEngine;

public class Exit : MonoBehaviour
{
    private UpgradeManager upgradeManager;
    void Start()
    {
        upgradeManager = FindFirstObjectByType<UpgradeManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        
        if (player != null)
        {
            DontDestroyOnLoad(upgradeManager);
            QuitGame();
        }
    }

    void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void ShowPossibleUpgrades()
    {
        
    }
}
}