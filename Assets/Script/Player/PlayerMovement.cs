using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _gravityAcceleration;
    [SerializeField] private float _maxFallSpeed;
    [SerializeField] private float _speed;
    [SerializeField] private float _rotationSensitivity;
    [SerializeField] private GameObject model;

    private CharacterController _controller;
    private InputAction _moveAction;
    private Vector3 _velocity;

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
            Vector3 direction = new Vector3(moveInput.x, 0, moveInput.y);
            model.transform.rotation = Quaternion.Slerp(model.transform.rotation,
            Quaternion.LookRotation(direction), 0.1f);
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

        Vector3 move = new Vector3(moveInput.x * _speed, 0,
        moveInput.y * _speed);

        _velocity.x = move.x;
        _velocity.z = move.z;
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

}
