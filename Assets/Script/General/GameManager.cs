namespace Dungeonlicious.Assets.Script
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.InputSystem;

    public class GameManager : MonoBehaviour
    {
        static public GameManager instance;
        [SerializeField] GameObject canvas;
        [SerializeField] GameObject pauseScreen;
        private bool paused = false;

        void Awake()
        {
            EnforceSingleton();
            RegisterEventListeners();
        }

        void Start()
        {
            canvas.SetActive(false);
        }

        void Update()
        {
            if (InputSystem.actions.FindAction("UI/Pause").WasPressedThisFrame())
            {
                TogglePause();
            }
        }

        void TogglePause()
        {
            paused = !paused;

            pauseScreen.SetActive(paused);
            Time.timeScale = paused ? 0f : 1f;
        }

        private void EnforceSingleton()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        private void RegisterEventListeners()
        {
            PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
            playerHealth.OnHealthChanged += CheckForGameOver;
            //playerHealth.OnHealthChanged += UpdateHealthBarFill;
        }

        private void CheckForGameOver(float ratio)
        {
            if (ratio <= 0f)
            {
                SceneManager.LoadScene("Main Menu");
            }
        }
        /*
        private void UpdateHealthBarFill(float ratio)
        {
            _hudManager.SetHealthFill(ratio);
        }
        */

        public void NextLevel(string levelName)
        {
            //TileDungeonGenerator tileDungeonGenerator = FindFirstObjectByType<TileDungeonGenerator>();
            // if (tileDungeonGenerator != null) tileDungeonGenerator.IncreaseLevel();
            canvas.SetActive(false);
            SceneManager.LoadScene(levelName);
        }
    }
}