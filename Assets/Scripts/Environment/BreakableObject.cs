using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [SerializeField] private List<Rigidbody> _allParts;

    public void BreakObject()
    {
        foreach (Rigidbody part in _allParts)
        {
            part.isKinematic = false;
        }
    }
}
