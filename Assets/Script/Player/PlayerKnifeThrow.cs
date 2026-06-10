namespace Dungeonlicious.Assets.Script
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class PlayerKnifeThrow : MonoBehaviour
    {
        [SerializeField] private GameObject knifePrefab;
        [SerializeField] private Transform throwSpot;
        [SerializeField] private int knifeCount = 5;

        [SerializeField] private TextMeshProUGUI knifeCountText;
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] private LockOnScript lockOnScript;
        private InputAction _knifeThrowAction;

        void Start()
        {
            _knifeThrowAction = InputSystem.actions.FindAction("KnifeThrow");
            UpdateKnifeUI();
        }

        void Update()
        {
            if (_knifeThrowAction.IsPressed() && knifeCount > 0)
            {
                Vector3 direction = GetAimDirection();
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, throwSpot.position);
                lineRenderer.SetPosition(1, throwSpot.position + direction * 5f);
            }

            if (_knifeThrowAction.WasReleasedThisFrame() && knifeCount > 0)
            {
                lineRenderer.enabled = false;

                GameObject knifeGO = Instantiate(knifePrefab, throwSpot.position, transform.rotation);
                Knife knife = knifeGO.GetComponent<Knife>();

                knife.SetDirection(GetAimDirection());
                knife.SetOwner(gameObject);

                knifeCount--;
                UpdateKnifeUI();
            }
        }

        private Vector3 GetAimDirection()
        {
            if (lockOnScript.IsLocked())
            {
                Vector3 toTarget = lockOnScript.GetCurrentTarget().transform.position - transform.position;
                toTarget.y = 0;
                return toTarget.normalized;
            }

            return GetMouseDirection();
        }

        private Vector3 GetMouseDirection()
        {
            if (Mouse.current == null)
                return transform.forward;

            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

            Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);

            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 mouseWorldPos = ray.GetPoint(distance);
                Vector3 direction = mouseWorldPos - throwSpot.position;
                direction.y = 0;
                return direction.normalized;
            }

            return transform.forward;
        }

        public void AddKnife()
        {
            knifeCount++;
            UpdateKnifeUI();
        }

        public void AddKnives(int amount)
        {
            knifeCount += amount;
            UpdateKnifeUI();
        }

        private void UpdateKnifeUI()
        {
            knifeCountText.text = ": " + knifeCount;
        }
    }
}