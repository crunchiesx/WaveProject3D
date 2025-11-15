using System;
using TMPro;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class AmmoCountTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ammoText;

    private void Start()
    {
        WeaponManager.Instance.OnAmmoCountChange += WeaponManager_OnAmmoCountChange;
    }

    private void WeaponManager_OnAmmoCountChange(int count, int size)
    {
        _ammoText.text = $"{count}/{size}";
    }

    private void OnDestroy()
    {
        WeaponManager.Instance.OnAmmoCountChange -= WeaponManager_OnAmmoCountChange;
    }
}