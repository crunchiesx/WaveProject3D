using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private LayerMask hittableLayers;

    private async void OnCollisionEnter(Collision other)
    {
        if ((hittableLayers.value & (1 << other.gameObject.layer)) != 0)
        {
            CreateBulletImpactEffect(other);
            Destroy(gameObject);
        }

        if (other.gameObject.TryGetComponent(out BreakableObject breakableObject))
        {
            await breakableObject.BreakObject();
        }
    }

    private void CreateBulletImpactEffect(Collision other)
    {
        ContactPoint contact = other.contacts[0];

        GameObject hole = Instantiate
        (
            GlobalReferences.Instance.BulletImpactEffectPrefab,
            contact.point,
            Quaternion.LookRotation(contact.normal)
        );

        hole.transform.SetParent(other.transform);
    }
}
