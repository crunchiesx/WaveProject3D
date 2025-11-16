using System;
using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum ShootingMode { Single, Burst, Auto }

    public Action<int, int> OnWeaponAmmoCountChange;

    [Header("References")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _bulletSpawn;
    [SerializeField] private ParticleSystem _muzzleEffect;
    [SerializeField] private WeaponItemSO weaponItemSO;

    [Header("Sounds")]
    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private AudioClip _reloadSound;
    [SerializeField] private AudioClip _emptySound;

    [Header("Weapon Settings")]
    [SerializeField] private ShootingMode _shootingMode = ShootingMode.Single;
    [SerializeField] private float _shootingDelay = 0.25f;
    [SerializeField] private float _spreadIntensity = 0.25f;

    [Header("Bullet Settings")]
    [SerializeField] private float _bulletVelocity = 500f;
    [SerializeField] private float _bulletLifeTime = 3f;

    [Header("Reload Settings")]
    [SerializeField] private int _magazineSize = 5;
    [SerializeField] private float _reloadTime = 1.5f;

    [Header("Burst Settings")]
    [SerializeField][Min(1)] private int _bulletsPerBurst = 3;
    [SerializeField] private float _burstDelay = 0.1f;

    public int MagazineSize => _magazineSize;

    private int _bulletsLeft;
    public int BulletsLeft
    {
        get => _bulletsLeft;
        set
        {
            _bulletsLeft = value;
            OnWeaponAmmoCountChange?.Invoke(value, _magazineSize);
        }
    }

    private bool _isShooting;
    private bool _fireHeld;
    private bool _isReloading;

    private bool _isActiveWeapon;
    public bool IsActiveWeapon
    {
        get => _isActiveWeapon;
        set => _isActiveWeapon = value;
    }

    private Camera _playerCamera;
    private Animator _weaponAnimator;
    private Coroutine _currentShootingCoroutine;

    private float _nextAllowedShootTime;

    private readonly int TriggerRecoil = Animator.StringToHash("Recoil");
    private readonly int TriggerReload = Animator.StringToHash("Reload");

    private void Awake()
    {
        _weaponAnimator = GetComponentInChildren<Animator>();
        _weaponAnimator.writeDefaultValuesOnDisable = true;
        BulletsLeft = _magazineSize;
    }

    private void Start()
    {
        _playerCamera = Camera.main;

        GameInputHandler.Instance.OnShootAction += ctx =>
        {
            if (!_isActiveWeapon) return;
            HandleShootInput(ctx);
        };

        GameInputHandler.Instance.OnReloadAction += () =>
        {
            if (!IsActiveWeapon || BulletsLeft >= _magazineSize || _isReloading) return;
            StartCoroutine(ReloadWeapon());
        };
    }

    private void OnEnable()
    {
        _weaponAnimator.Rebind();
        _weaponAnimator.Update(0f);
        _weaponAnimator.ResetTrigger(TriggerReload);
        _weaponAnimator.ResetTrigger(TriggerRecoil);
        _weaponAnimator.Play(0, 0, 0f);
    }

    private void OnDisable()
    {
        _isReloading = false;
        _isShooting = false;
        _fireHeld = false;

        if (_currentShootingCoroutine != null)
        {
            StopCoroutine(_currentShootingCoroutine);
            _currentShootingCoroutine = null;
        }

        SoundManager.Instance.StopSoundsFollowing(transform);
    }

    private void HandleShootInput(bool isPressed)
    {
        _fireHeld = isPressed;

        if (!isPressed)
        {
            _isShooting = false;
            if (_currentShootingCoroutine != null)
            {
                StopCoroutine(_currentShootingCoroutine);
                _currentShootingCoroutine = null;
            }
            return;
        }

        if (!_isReloading && BulletsLeft <= 0)
        {
            SoundManager.Instance.PlayAudio(_emptySound, transform);
            _isShooting = false;
            return;
        }

        _isShooting = true;
        StartShooting();
    }

    private void StartShooting()
    {
        if (_currentShootingCoroutine != null)
        {
            StopCoroutine(_currentShootingCoroutine);
            _currentShootingCoroutine = null;
        }

        switch (_shootingMode)
        {
            case ShootingMode.Single:
                TryShoot();
                break;
            case ShootingMode.Burst:
                _currentShootingCoroutine = StartCoroutine(FireBurstOnce());
                break;
            case ShootingMode.Auto:
                _currentShootingCoroutine = StartCoroutine(AutoFire());
                break;
        }
    }

    private void TryShoot()
    {
        if (_isReloading) return;
        if (Time.time < _nextAllowedShootTime || BulletsLeft <= 0) return;

        FireWeapon();
        _nextAllowedShootTime = Time.time + _shootingDelay;
    }

    private IEnumerator AutoFire()
    {
        while (_isShooting && BulletsLeft > 0 && !_isReloading)
        {
            TryShoot();
            yield return null;
        }

        if (BulletsLeft <= 0)
            SoundManager.Instance.PlayAudio(_emptySound, transform);
    }

    private IEnumerator FireBurstOnce()
    {
        if (_isReloading || BulletsLeft <= 0) yield break;

        int bulletsToFire = Mathf.Min(_bulletsPerBurst, BulletsLeft);
        for (int i = 0; i < bulletsToFire; i++)
        {
            if (_isReloading || BulletsLeft <= 0) break;

            FireWeapon();
            yield return new WaitForSeconds(_burstDelay);
        }

        _nextAllowedShootTime = Time.time + _shootingDelay;
        _isShooting = false;
    }

    private void FireWeapon()
    {
        if (BulletsLeft <= 0) return;

        _muzzleEffect.Play();
        _weaponAnimator.SetTrigger(TriggerRecoil);
        SoundManager.Instance.PlayAudio(_shootSound, _bulletSpawn);

        BulletsLeft--;

        Vector3 shootDirection = CalculateDirectionAndSpread().normalized;
        GameObject bullet = Instantiate(_bulletPrefab, _bulletSpawn.position, Quaternion.identity);
        bullet.transform.forward = shootDirection;
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce(shootDirection * _bulletVelocity, ForceMode.Impulse);

        StartCoroutine(DestroyBulletAfterTime(bullet, _bulletLifeTime));
    }

    private Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit) ? hit.point : ray.GetPoint(100);

        float spreadX = UnityEngine.Random.Range(-_spreadIntensity, _spreadIntensity);
        float spreadY = UnityEngine.Random.Range(-_spreadIntensity, _spreadIntensity);

        Vector3 offset = (_playerCamera.transform.right * spreadX + _playerCamera.transform.up * spreadY) * Vector3.Distance(_playerCamera.transform.position, targetPoint);
        return (targetPoint + offset - _bulletSpawn.position).normalized;
    }

    private IEnumerator ReloadWeapon()
    {
        _isReloading = true;
        _weaponAnimator.SetTrigger(TriggerReload);
        SoundManager.Instance.PlayAudio(_reloadSound, transform);

        yield return new WaitForSeconds(_reloadTime * 0.7f);
        BulletsLeft = _magazineSize;
        yield return new WaitForSeconds(_reloadTime * 0.3f);

        _isReloading = false;

        if (_fireHeld)
        {
            _isShooting = true;
            StartShooting();
        }
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}