namespace Dungeonlicious.Assets.Script
{
    using UnityEngine;

    public class Exit : MonoBehaviour
    {
        [SerializeField]
        private UpgradeCanvasManager upgradeCanvasManager;
        void Start()
        {
            upgradeCanvasManager = UpgradeCanvasManager.Instance;
            upgradeCanvasManager.gameObject.SetActive(false);
        }
        private void OnTriggerEnter(Collider other)
        { 
            if (TileDungeonGenerator.Instance.Level < TileDungeonGenerator.Instance.MaxLevel - 1) UpgradeCanvasManager.Instance.gameObject.SetActive(true);
            else
            {
                UpgradeCanvasManager.Instance.gameObject.SetActive(true);
                UpgradeCanvasManager.Instance.NextLevel();
            }
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