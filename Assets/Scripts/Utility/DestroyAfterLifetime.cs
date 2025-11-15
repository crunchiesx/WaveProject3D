using System.Collections;
using UnityEngine;

public class DestroyAfterLifetime : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 10f;

    private void Start()
    {
        StartCoroutine(DestroyLifetime());
    }

    private IEnumerator DestroyLifetime()
    {
        yield return new WaitForSeconds(_lifeTime);
        Destroy(gameObject);
    }
}
