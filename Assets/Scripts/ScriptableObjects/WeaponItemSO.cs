using UnityEngine;

[CreateAssetMenu(fileName = "WeaponItemSO", menuName = "ScriptableObject/WeaponItemSO")]
public class WeaponItemSO : ItemSO
{
    [Header("WeaponItemSO")]
    public GameObject weaponObject;
    public GameObject worldObject;
}