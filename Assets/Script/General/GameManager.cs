using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager instance;

    void Awake()
    {
        EnforceSingleton();
        RegisterEventListeners();
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
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        }
    }
    /*
    private void UpdateHealthBarFill(float ratio)
    {
        _hudManager.SetHealthFill(ratio);
    }
    */


}
