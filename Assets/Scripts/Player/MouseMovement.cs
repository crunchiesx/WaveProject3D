using UnityEngine;
using UnityEngine.InputSystem;

public class MouseMovement : MonoBehaviour
{
    private const float SENSITIVITY_SCALE = 0.01f;

    [Header("Camera Settings")]
    [SerializeField] private float _mouseSensitivity = 2.5f;

    [Header("Clamp Camera xRotation")]
    [Range(-90, -30)]
    [SerializeField] private float _topClamp = -90;
    [Range(30, 90)]
    [SerializeField] private float _bottomClamp = 90;

    private float _xRotation;
    private float _yRotation;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Vector2 lookVector = GameInputHandler.Instance.GetMouseVector();

        float mouseY = lookVector.y * _mouseSensitivity * SENSITIVITY_SCALE;
        float mouseX = lookVector.x * _mouseSensitivity * SENSITIVITY_SCALE;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, _topClamp, _bottomClamp);

        _yRotation += mouseX;

        transform.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
    }
}
