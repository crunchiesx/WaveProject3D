using System;
using UnityEngine;
using UnityEngine.Rendering;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    public Action<int, int> OnAmmoCountChange;

    private Weapon _currentWeapon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetCurrentWeapon(Weapon weapon)
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.OnWeaponAmmoCountChange -= HandleAmmoCountChange;
        }

        _currentWeapon = weapon;
        _currentWeapon.OnWeaponAmmoCountChange += HandleAmmoCountChange;
        HandleAmmoCountChange(weapon.BulletsLeft, weapon.MagazineSize);
    }

    public void PickUpWeapon(GameObject weapon)
    {
        Destroy(weapon);
    }

    private void HandleAmmoCountChange(int count, int size)
    {
        OnAmmoCountChange?.Invoke(count, size);
    }
}