using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "ScriptableObject/ItemSO")]
public class ItemSO : ScriptableObject
{
    public GameObject itemObject;
    public GameObject pickupObject;
}
