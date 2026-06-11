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
            DontDestroyOnLoad(upgradeManager.gameObject);
            if (upgradeCanvasManager == null)
            {
                upgradeCanvasManager = FindFirstObjectByType<UpgradeCanvasManager>();
                upgradeCanvasManager.gameObject.SetActive(false);
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();

            upgradeCanvasManager.gameObject.SetActive(true);
            Time.timeScale = 0f;
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