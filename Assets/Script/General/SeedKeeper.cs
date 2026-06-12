using UnityEngine;

public class SeedKeeper : MonoBehaviour
{
    private string seed;
    private static SeedKeeper _instance;
    public static SeedKeeper Instance => _instance;
    public string Seed { get; set; }

    public void SetSeed(string value)
    {
        Seed = value;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_instance == null) { _instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
