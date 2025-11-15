using System;
using System.ComponentModel;
using UnityEngine;

public class GameInputHandler : MonoBehaviour
{
    public static GameInputHandler Instance { get; private set; }

    private GameInputActions _inputActions;

    public Action<bool> OnJumpAction;
    public Action<bool> OnSprintAction;
    public Action<bool> OnShootAction;

    public Action OnReloadAction;
    public Action OnInteractAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;

        _inputActions = new GameInputActions();
    }

    private void OnEnable() => _inputActions.Enable();
    private void OnDisable() => _inputActions.Disable();
    private void OnDestroy() => _inputActions.Dispose();

    private void Start()
    {
        SetInputActions();
    }

    private void SetInputActions()
    {
        // Jump
        _inputActions.Player.Jump.performed += ctx => OnJumpAction?.Invoke(true);
        _inputActions.Player.Jump.canceled += ctx => OnJumpAction?.Invoke(false);

        // Sprint
        _inputActions.Player.Sprint.performed += ctx => OnSprintAction?.Invoke(true);
        _inputActions.Player.Sprint.canceled += ctx => OnSprintAction?.Invoke(false);

        // Shoot
        _inputActions.Player.Shoot.performed += ctx => OnShootAction?.Invoke(true);
        _inputActions.Player.Shoot.canceled += ctx => OnShootAction?.Invoke(false);

        // Reload
        _inputActions.Player.Reload.performed += ctx => OnReloadAction?.Invoke();

        // Interact
        _inputActions.Player.Interact.performed += ctx => OnInteractAction?.Invoke();
    }

    public Vector2 GetMovementVector() => _inputActions.Player.Movement.ReadValue<Vector2>();
    public Vector2 GetMouseVector() => _inputActions.Player.Mouse.ReadValue<Vector2>();
}
