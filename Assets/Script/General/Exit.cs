namespace Dungeonlicious.Assets.Script
{
using UnityEngine;

public class Exit : MonoBehaviour
{
    private UpgradeManager upgradeManager;
    [SerializeField]
    private UpgradeCanvasManager upgradeCanvasManager;
    void Start()
    {
        upgradeManager = FindFirstObjectByType<UpgradeManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        
        if (player != null)
        {
            upgradeCanvasManager.gameObject.SetActive(true);
        }
    }

    void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
}