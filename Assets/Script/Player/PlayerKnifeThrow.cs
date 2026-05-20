using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKnifeThrow : MonoBehaviour
{
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private Transform throwSpot;
    [SerializeField] private GameObject modelDirection;
    [SerializeField] private int knifeCount = 5;

    [SerializeField] private TextMeshProUGUI knifeCountText;
    [SerializeField] LineRenderer lineRenderer;
    private InputAction _knifeThrowAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _knifeThrowAction = InputSystem.actions.FindAction("KnifeThrow");

        UpdateKnifeUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (_knifeThrowAction.IsPressed() && knifeCount > 0)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, throwSpot.position);
            lineRenderer.SetPosition(1, throwSpot.position + modelDirection.transform.forward * 5f);
        }
        if (_knifeThrowAction.WasReleasedThisFrame() && knifeCount > 0)
        {
            lineRenderer.enabled = false;
            GameObject knife = Instantiate(knifePrefab,
            throwSpot.position,
            transform.rotation);

            Vector3 direction = new Vector3(modelDirection.transform.forward.x, 0, modelDirection.transform.forward.z).normalized;

            knife.GetComponent<Knife>().SetDirection(direction);

            knife.GetComponent<Knife>().SetOwner(gameObject);

            knifeCount--;

            UpdateKnifeUI();
        }
    }

    public void AddKnife()
    {
        knifeCount++;

        UpdateKnifeUI();
    }
    private void UpdateKnifeUI()
    {
        knifeCountText.text = "Knives: " + knifeCount;
    }
}
