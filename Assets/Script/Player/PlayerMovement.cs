using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _gravityAcceleration;
    [SerializeField] private float _maxFallSpeed;
    [SerializeField] private float _speed;
    [SerializeField] private float _rotationSensitivity;
    [SerializeField] private GameObject model;

    [SerializeField] private Transform cameraTransform;

    private CharacterController _controller;
    private InputAction _moveAction;
    private Vector3 _velocity;
    private bool _isLocked;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _moveAction = InputSystem.actions.FindAction("Move");
        _velocity = Vector3.zero;
    }

    private void Update()
    {
        UpdateRotation();
        UpdatePosition();
    }

    private void UpdateRotation()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();

        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 moveDirection = GetCameraRelativeDirection(moveInput);
            
            //Vector3 direction = new Vector3(moveInput.x + moveInput.x/2, 0, moveInput.y + moveInput.y/2);
            /*
            model.transform.rotation = Quaternion.Slerp(model.transform.rotation,
            Quaternion.LookRotation(direction), 0.1f);
            */
            model.transform.rotation = Quaternion.Slerp(model.transform.rotation,
            Quaternion.LookRotation(moveDirection), _rotationSensitivity * Time.deltaTime);
        }
    }

    private void UpdatePosition()
    {
        UpdateVelocityZX();
        UpdateVelocityY();
        MoveCharacterController();
    }

    private void UpdateVelocityZX()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        moveInput.Normalize();

        /*
        Vector3 move = new Vector3(moveInput.x * _speed, 0,
        moveInput.y * _speed);
        */

        Vector3 moveDirection = GetCameraRelativeDirection(moveInput);

        /*
        _velocity.x = move.x;
        _velocity.z = move.z;
        */
        _velocity.x = moveDirection.x * _speed;
        _velocity.z = moveDirection.z * _speed;
    }

    private void UpdateVelocityY()
    {
        if (_controller.isGrounded)
            _velocity.y = -1f;
        else if (_velocity.y > -_maxFallSpeed)
        {
            _velocity.y += _gravityAcceleration * Time.deltaTime;
            _velocity.y = Mathf.Max(_velocity.y += _gravityAcceleration * Time.deltaTime, -_maxFallSpeed);
        }
    }

    private void MoveCharacterController()
    {
        Vector3 motion = transform.TransformVector(_velocity * Time.deltaTime);

        _controller.Move(motion);
    }

    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        return (forward * input.y + right * input.x).normalized;
    }
}
