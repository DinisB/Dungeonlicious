using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKnifeThrow : MonoBehaviour
{
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private Transform throwSpot;
    [SerializeField] private GameObject modelDirection;
    [SerializeField] private int knifeCount = 5;
    private InputAction _knifeThrowAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _knifeThrowAction = InputSystem.actions.FindAction("KnifeThrow");
    }

    // Update is called once per frame
    void Update()
    {
        if (_knifeThrowAction.WasPressedThisFrame() && knifeCount > 0)
        {
            GameObject knife = Instantiate(knifePrefab,
            throwSpot.position,
            transform.rotation);

            Vector3 direction = modelDirection.transform.forward;

            knife.GetComponent<Knife>().SetDirection(direction);

            knifeCount--;
        }
    }

    public void AddKnife()
    {
        knifeCount++;
    }
}
