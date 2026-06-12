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