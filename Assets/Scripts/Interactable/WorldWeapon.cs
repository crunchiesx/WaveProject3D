using UnityEngine;

public class WorldWeapon : BaseInteractable
{
    public WeaponItemSO weaponItemSO;

    public override void Interact()
    {
        if (weaponItemSO != null)
        {
            WeaponManager.Instance.PickUpWeapon(weaponItemSO, gameObject);
        }
    }
}
