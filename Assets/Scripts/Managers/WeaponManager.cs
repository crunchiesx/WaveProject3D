using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [SerializeField] private Transform firstWeaponSlot;
    [SerializeField] private Transform secondWeaponSlot;

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

        SetActiveWeaponSlot(1);
    }

    private void Start()
    {
        GameInputHandler.Instance.OnFirstSlotAction += () => { SetActiveWeaponSlot(1); };
        GameInputHandler.Instance.OnSecondSlotAction += () => { SetActiveWeaponSlot(2); };
    }

    private void SetCurrentWeapon(Weapon weapon)
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.OnWeaponAmmoCountChange -= HandleAmmoCountChange;
            _currentWeapon.IsActiveWeapon = false;
        }

        _currentWeapon = weapon;
        _currentWeapon.IsActiveWeapon = true;
        _currentWeapon.OnWeaponAmmoCountChange += HandleAmmoCountChange;
        HandleAmmoCountChange(weapon.BulletsLeft, weapon.MagazineSize);
    }

    public void PickUpWeapon(WeaponItemSO weaponItemSO, GameObject caller)
    {
        if (weaponItemSO.weaponObject != null)
        {
            if (firstWeaponSlot.childCount == 0)
            {
                GameObject weapon = Instantiate(weaponItemSO.weaponObject);
                weapon.transform.SetParent(firstWeaponSlot, false);
                SetCurrentWeapon(weapon.GetComponent<Weapon>());
                SetActiveWeaponSlot(1);
                Destroy(caller);
            }
            else if (secondWeaponSlot.childCount == 0)
            {
                GameObject weapon = Instantiate(weaponItemSO.weaponObject);
                weapon.transform.SetParent(secondWeaponSlot, false);
                SetCurrentWeapon(weapon.GetComponent<Weapon>());
                SetActiveWeaponSlot(2);
                Destroy(caller);

            }
        }
    }

    private void SetActiveWeaponSlot(int slot)
    {
        if (slot == 1)
        {
            firstWeaponSlot.gameObject.SetActive(true);
            secondWeaponSlot.gameObject.SetActive(false);

            if (firstWeaponSlot.GetComponentInChildren<Weapon>() is Weapon activeSlotWeapon)
            {
                SetCurrentWeapon(activeSlotWeapon);
            }
            else
            {
                if (_currentWeapon == null) return;
                _currentWeapon.IsActiveWeapon = false;
                HandleAmmoCountChange();
            }
        }
        else if (slot == 2)
        {
            firstWeaponSlot.gameObject.SetActive(false);
            secondWeaponSlot.gameObject.SetActive(true);

            if (secondWeaponSlot.GetComponentInChildren<Weapon>() is Weapon activeSlotWeapon)
            {
                SetCurrentWeapon(activeSlotWeapon);
            }
            else
            {
                if (_currentWeapon == null) return;
                _currentWeapon.IsActiveWeapon = false;
                HandleAmmoCountChange();
            }
        }
    }

    private void HandleAmmoCountChange(int count = 0, int size = 0)
    {
        OnAmmoCountChange?.Invoke(count, size);
    }
}