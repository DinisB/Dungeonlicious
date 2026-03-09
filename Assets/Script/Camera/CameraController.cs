using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform  _followTarget;
    [SerializeField] private float      _followResponsiveness;
    [SerializeField] private float      _rotationSensitivity;
    [SerializeField] private float      _resetRotationSpeed;
    [SerializeField] private float      _maxLookUpAngle;
    [SerializeField] private float      _maxLookDownAngle;
    [SerializeField] private float      _zoomSensitivity;
    [SerializeField] private float      _minZoomDistance;
    [SerializeField] private float      _maxZoomDistance;
    [SerializeField] private float      _zoomDeceleration;
    [SerializeField] private float _followTreshold;

    private Transform   _cameraTransform;
    private Camera   _camera;
    private InputAction _zoomAction;
    private float       _zoomAcceleration;
    private float       _zoomVelocity;

    void Start()
    {
        _cameraTransform    = GetComponentInChildren<Camera>().transform;
        _zoomAction         = InputSystem.actions.FindAction("Zoom");
        _zoomVelocity       = 0f;
    }

    void Update()
    {
        UpdatePosition();
        UpdateZoom();
    }

    private void UpdatePosition()
    {
        transform.position = Vector3.Lerp(transform.position, _followTarget.position, 1 - Mathf.Exp(-_followResponsiveness * Time.deltaTime));
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
                _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize,_minZoomDistance, _maxZoomDistance);
                _zoomVelocity = 0f;
            }
        }
    }

}
