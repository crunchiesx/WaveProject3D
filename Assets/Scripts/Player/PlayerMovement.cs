using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private const float GRAVITY = -9.81f;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 12f;
    [SerializeField] private float _speedMultiplier = 2f;
    [SerializeField] private float _jumpHeight = 3f;
    [SerializeField] private float _weight = 2f;

    [Header("Ground Settings")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _groundDistance;

    private CharacterController _controller;

    private bool _isGrounded;
    private bool _isMoving;
    private bool _isJumping;
    private bool _isSprinting;

    private Vector3 _velocity;
    private Vector3 _lastPosition = Vector3.zero;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        GameInputHandler.Instance.OnJumpAction += value => _isJumping = value;
        GameInputHandler.Instance.OnSprintAction += value => _isSprinting = value;
    }

    private void Update()
    {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        Vector2 movementVector = GameInputHandler.Instance.GetMovementVector().normalized;
        Vector3 moveDir = transform.right * movementVector.x + transform.forward * movementVector.y;

        float currentSpeed = _isSprinting ? _moveSpeed * _speedMultiplier : _moveSpeed;
        _controller.Move(currentSpeed * Time.deltaTime * moveDir);

        if (_isJumping && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(-2 * GRAVITY * _jumpHeight);
        }

        _velocity.y += GRAVITY * _weight * Time.deltaTime;

        _controller.Move(_velocity * Time.deltaTime);

        if (_lastPosition != transform.position && _isGrounded)
        {
            _isMoving = true;
        }
        else
        {
            _isMoving = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_groundCheck.position, _groundDistance);
    }
}
