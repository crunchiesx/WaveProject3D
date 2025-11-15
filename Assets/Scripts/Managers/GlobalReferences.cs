using UnityEngine;

public class GlobalReferences : MonoBehaviour
{
    public static GlobalReferences Instance { get; private set; }

    public GameObject BulletImpactEffectPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}