using System;
using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }

    public Action<int, int> OnWeaponAmmoCountChange;

    [Header("References")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _bulletSpawn;
    [SerializeField] private ParticleSystem _muzzleEffect;

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

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 rotation;

    [Header("Burst Settings (If shooting mode is burst)")]
    [SerializeField][Min(1)] private int _bulletsPerBurst = 1;
    [SerializeField] private float _burstDelay = 0.1f;

    private int _bulletsLeft;
    public int BulletsLeft
    {
        get { return _bulletsLeft; }
        set
        {
            _bulletsLeft = value;
            OnWeaponAmmoCountChange?.Invoke(value, _magazineSize);
        }
    }
    public int MagazineSize => _magazineSize;

    private bool _isShooting = false;
    private bool _readyToShoot = true;
    private bool _isReloading;
    private bool _fireHeld;

    private Camera _playerCamera;
    private Animator _weaponAnimator;
    private Coroutine _currentShootingCoroutine;

    private readonly int TriggerRecoil = Animator.StringToHash("Recoil");
    private readonly int TriggerReload = Animator.StringToHash("Reload");

    private void Awake()
    {
        _weaponAnimator = GetComponentInChildren<Animator>();
        BulletsLeft = _magazineSize;
    }

    private void Start()
    {
        GameInputHandler.Instance.OnShootAction += value => HandleShootInput(value);
        GameInputHandler.Instance.OnReloadAction += () =>
        {
            if (BulletsLeft < _magazineSize && !_isReloading)
            {
                StartCoroutine(ReloadWeapon());
            }
        };

        WeaponManager.Instance.SetCurrentWeapon(this);
        _playerCamera = Camera.main;
    }

    private void HandleShootInput(bool isPressed)
    {
        _fireHeld = isPressed;

        if (isPressed)
        {
            if (_isReloading)
                return;

            if (BulletsLeft <= 0)
            {
                SoundManager.Instance.PlayAudio(_emptySound, transform);
                _isShooting = false;
                return;
            }

            _isShooting = true;
            StartShooting();
        }
        else
        {
            _isShooting = false;

            if (_currentShootingCoroutine != null)
            {
                StopCoroutine(_currentShootingCoroutine);
                _currentShootingCoroutine = null;
            }
        }
    }

    private void TryShoot()
    {
        if (!_readyToShoot || BulletsLeft <= 0) return;

        FireWeapon();
        StartCoroutine(WaitForNextShot());
    }

    private IEnumerator AutoFire()
    {
        while (_isShooting && BulletsLeft > 0 && !_isReloading)
        {
            TryShoot();
            yield return new WaitForSeconds(_shootingDelay);
        }

        if (BulletsLeft <= 0)
        {
            SoundManager.Instance.PlayAudio(_emptySound, transform);
        }
    }

    private IEnumerator FireBurst()
    {
        if (!_readyToShoot || _isReloading || BulletsLeft <= 0)
        {
            yield break;
        }

        _readyToShoot = false;

        for (int i = 0; i < _bulletsPerBurst; i++)
        {
            if (BulletsLeft <= 0 || _isReloading) break;

            FireWeapon();
            yield return new WaitForSeconds(_burstDelay);
        }

        yield return new WaitForSeconds(_shootingDelay);
        _readyToShoot = true;
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

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100);
        }

        float spreadX = UnityEngine.Random.Range(-_spreadIntensity, _spreadIntensity);
        float spreadY = UnityEngine.Random.Range(-_spreadIntensity, _spreadIntensity);

        Vector3 targetDirection = targetPoint - _playerCamera.transform.position;
        float distance = targetDirection.magnitude;

        Vector3 spreadOffset = (_playerCamera.transform.right * spreadX + _playerCamera.transform.up * spreadY) * distance;
        Vector3 finalTarget = targetPoint + spreadOffset;

        return (finalTarget - _bulletSpawn.position).normalized;
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
                _currentShootingCoroutine = StartCoroutine(FireBurst());
                break;

            case ShootingMode.Auto:
                _currentShootingCoroutine = StartCoroutine(AutoFire());
                break;
        }
    }

    private IEnumerator ReloadWeapon()
    {
        _isReloading = true;
        SoundManager.Instance.PlayAudio(_reloadSound, transform);
        _weaponAnimator.SetTrigger(TriggerReload);

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

    private IEnumerator WaitForNextShot()
    {
        _readyToShoot = false;
        yield return new WaitForSeconds(_shootingDelay);
        _readyToShoot = true;
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}
