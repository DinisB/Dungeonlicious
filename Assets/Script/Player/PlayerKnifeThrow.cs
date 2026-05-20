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
    [SerializeField] private LockOnScript lockOnScript;
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
            if (lockOnScript.IsLocked())
            {
                Vector3 direction = lockOnScript.GetCurrentTarget().transform.position - transform.position;
                direction.y = 0;
                lineRenderer.SetPosition(1, throwSpot.position + direction.normalized * 5f);
            }
            else
            {
                lineRenderer.SetPosition(1, throwSpot.position + modelDirection.transform.forward * 5f);
            }
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, throwSpot.position);
        }
        if (_knifeThrowAction.WasReleasedThisFrame() && knifeCount > 0)
        {
            Vector3 direction;
            lineRenderer.enabled = false;
            GameObject knife = Instantiate(knifePrefab,
            throwSpot.position,
            transform.rotation);

            if (lockOnScript.IsLocked())
            {
                direction = lockOnScript.GetCurrentTarget().transform.position - transform.position;
                direction.y = 0;
                knife.GetComponent<Knife>().SetDirection(direction.normalized);
            }
            else {
            direction = modelDirection.transform.forward;
            }

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
