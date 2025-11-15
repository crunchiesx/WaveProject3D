using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            CreateBulletImpactEffect(other);
            Destroy(gameObject);
        }

        if (other.gameObject.CompareTag("Wall"))
        {
            CreateBulletImpactEffect(other);
            Destroy(gameObject);
        }

        if (other.gameObject.CompareTag("Breakable"))
        {
            other.gameObject.GetComponent<BreakableObject>().BreakObject();
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
