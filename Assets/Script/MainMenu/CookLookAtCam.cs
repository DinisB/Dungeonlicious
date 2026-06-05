using UnityEngine;

public class CookLookAtCam : MonoBehaviour
{
    [SerializeField] private GameObject _cam;
    [SerializeField] private GameObject _head;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        _head.transform.LookAt(_cam.transform);
    }
}
