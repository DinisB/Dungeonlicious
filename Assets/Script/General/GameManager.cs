using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager instance;

    void Awake()
    {
        EnforceSingleton();
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

    void Start()
    {
        
    }

}
