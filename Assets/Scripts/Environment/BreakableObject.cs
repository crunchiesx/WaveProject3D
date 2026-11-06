using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BreakableObject : MonoBehaviour
{
    [SerializeField] private List<Rigidbody> allParts;
    [SerializeField] private float destroyTime = 5f;

    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    public async Awaitable BreakObject()
    {
        foreach (Rigidbody part in allParts)
        {
            part.isKinematic = false;
        }

        await Awaitable.WaitForSecondsAsync(0.1f);

        _collider.enabled = false;

        DestroyAfterTime(destroyTime);
    }

    private void DestroyAfterTime(float time)
    {
        Destroy(gameObject, time);
    }
}