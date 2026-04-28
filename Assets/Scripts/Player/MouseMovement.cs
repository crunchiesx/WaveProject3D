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

    [Header("Recoil Settings")]
    [SerializeField] private float _recoilRotationSpeed = 10f;
    [SerializeField] private float _recoilReturnSpeed = 15f;

    private float _xRotation;
    private float _yRotation;

    private Vector3 _currentRecoilRotation;
    private Vector3 _targetRecoilRotation;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Subscribe to the fire event
        WeaponManager.Instance.OnCurrentWeaponFire += FireRecoil;
    }

    private void OnDisable()
    {
        WeaponManager.Instance.OnCurrentWeaponFire -= FireRecoil;
    }

    private void Update()
    {
        // 1. Handle Mouse Input
        Vector2 lookVector = GameInputHandler.Instance.GetMouseVector();

        float mouseY = lookVector.y * _mouseSensitivity * SENSITIVITY_SCALE;
        float mouseX = lookVector.x * _mouseSensitivity * SENSITIVITY_SCALE;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, _topClamp, _bottomClamp);
        _yRotation += mouseX;

        // 2. Handle Recoil Math
        // Smoothly pull target back to zero
        _targetRecoilRotation = Vector3.Lerp(_targetRecoilRotation, Vector3.zero, _recoilReturnSpeed * Time.deltaTime);
        // Snappily move current rotation toward the target
        _currentRecoilRotation = Vector3.Slerp(_currentRecoilRotation, _targetRecoilRotation, _recoilRotationSpeed * Time.deltaTime);

        // 3. Combine Mouse + Recoil
        // We add the recoil offsets to the mouse rotations
        transform.localRotation = Quaternion.Euler(_xRotation + _currentRecoilRotation.x, _yRotation + _currentRecoilRotation.y, _currentRecoilRotation.z);
    }

    public void FireRecoil(float recoilX, float recoilY)
    {
        // IMPORTANT: Use negative recoilX (e.g., -2f) to make the camera kick UP
        _targetRecoilRotation += new Vector3(-recoilX, Random.Range(-recoilY, recoilY), 0f);
    }
}