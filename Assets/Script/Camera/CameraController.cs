using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _followTarget;
    [SerializeField] private float _followResponsiveness;
    [SerializeField] private float _rotationSensitivity;
    [SerializeField] private float _resetRotationSpeed;
    [SerializeField] private float _maxLookUpAngle;
    [SerializeField] private float _maxLookDownAngle;
    [SerializeField] private float _zoomSensitivity;
    [SerializeField] private float _minZoomDistance;
    [SerializeField] private float _maxZoomDistance;
    [SerializeField] private float _zoomDeceleration;
    [SerializeField] private float _followTreshold;

    private Transform _cameraTransform;
    private Camera _camera;
    private InputAction _zoomAction;
    private float _zoomAcceleration;
    private float _zoomVelocity;
    private InputAction _mouseAction;
    private InputAction _mouseClickAction;
    private InputAction _moveAction;
    private bool canMoveCamera;
    private bool _isLocked;
    private GameObject _currentTarget;

    private void Start()
    {
        _cameraTransform = GetComponentInChildren<Camera>().transform;
        _zoomAction = InputSystem.actions.FindAction("Zoom");
        _mouseAction = InputSystem.actions.FindAction("Look");
        _mouseClickAction = InputSystem.actions.FindAction("Click");
        _moveAction = InputSystem.actions.FindAction("Move");
        _zoomVelocity = 0f;
        canMoveCamera = true;
        _isLocked = false;
    }

    private void Update()
    {
        UpdatePosition();
        UpdateZoom();

        if (_mouseClickAction.IsPressed())
        {
            _isLocked = false;
            canMoveCamera = true;
            transform.position = Vector3.Lerp(transform.position, transform.position - (Vector3)_mouseAction.ReadValue<Vector2>(), 1 - Mathf.Exp(-_followResponsiveness * Time.deltaTime));
        }
        else if (_moveAction.ReadValue<Vector2>() != Vector2.zero && !_isLocked)
        {
            canMoveCamera = false;
        }

        if (_isLocked && _currentTarget != null)
        {
            transform.position = Vector3.Lerp(transform.position, _currentTarget.transform.position, 1 - Mathf.Exp(-_followResponsiveness * Time.deltaTime));
        }
    }

    private void UpdatePosition()
    {
        if (!canMoveCamera && !_isLocked)
        {
            transform.position = Vector3.Lerp(transform.position, _followTarget.position, 1 - Mathf.Exp(-_followResponsiveness * Time.deltaTime));
        }
    }

    private void UpdateZoom()
    {
        UpdateZoomVelocity();
        UpdateZoomPosition();
    }

    private void UpdateZoomVelocity()
    {
        _zoomAcceleration = _zoomAction.ReadValue<float>() * _zoomSensitivity;

        if (_zoomAcceleration != 0f)
            _zoomVelocity += _zoomAcceleration * Time.deltaTime;
        else if (_zoomVelocity > 0f)
        {
            _zoomVelocity -= _zoomDeceleration * Time.deltaTime;
            _zoomVelocity = Mathf.Max(0f, _zoomVelocity);
        }
        else
        {
            _zoomVelocity += _zoomDeceleration * Time.deltaTime;
            _zoomVelocity = Mathf.Min(0f, _zoomVelocity);
        }
    }

    private void UpdateZoomPosition()
    {
        if (_zoomVelocity != 0f)
        {
            _camera = _cameraTransform.GetComponent<Camera>();
            _camera.orthographicSize += _zoomVelocity * Time.deltaTime;

            if (_camera.orthographicSize > _minZoomDistance || _camera.orthographicSize < _maxZoomDistance)
            {
                _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _minZoomDistance, _maxZoomDistance);
                _zoomVelocity = 0f;
            }
        }
    }

    public bool IsLocked
    {
        get { return _isLocked; }
        set
        {
            _isLocked = value;
            _currentTarget = null;
        }
    }

    public void LockOnTarget(Transform target)
    {
        _isLocked = true;
        _currentTarget = target.gameObject;
    }

    public GameObject GetCurrentTarget()
    {
        return _currentTarget;
    }
}
