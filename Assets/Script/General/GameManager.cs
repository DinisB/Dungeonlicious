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
        private bool isLoading = false;

        private bool paused = false;

        void Awake()
        {
            EnforceSingleton();
            RegisterEventListeners();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            isLoading = false;
        }

        void Start()
        {
            canvas.SetActive(false);

            if (pauseScreen.scene.buildIndex != -1)
            {
                DontDestroyOnLoad(pauseScreen);
            }
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
                SceneManager.LoadScene("Game Over");
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
            if (isLoading) return;
            isLoading = true;

            TileDungeonGenerator tileDungeonGenerator = FindFirstObjectByType<TileDungeonGenerator>();
            if (tileDungeonGenerator != null)
                tileDungeonGenerator.IncreaseLevel();

            if (tileDungeonGenerator.Level > tileDungeonGenerator.MaxLevel - 1)
            {
                SceneManager.LoadScene("Main Menu");
            }
            else { SceneManager.LoadScene(levelName); }
        }
    }
}