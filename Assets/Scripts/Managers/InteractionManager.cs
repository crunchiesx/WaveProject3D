using System;
using TMPro;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 5f;

    private BaseInteractable hoveredInteractable = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        GameInputHandler.Instance.OnInteractAction += Interact;
    }

    private void Interact()
    {
        if (hoveredInteractable)
        {
            hoveredInteractable.Interact();
        }
    }

    private void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        BaseInteractable newInteractable = null;

        if (Physics.Raycast(ray, out RaycastHit hitInfo, interactionDistance))
        {
            hitInfo.transform.TryGetComponent(out newInteractable);
        }

        if (hoveredInteractable != newInteractable)
        {
            if (hoveredInteractable != null)
            {
                hoveredInteractable.SetOutline(false);
            }

            hoveredInteractable = newInteractable;

            if (hoveredInteractable != null)
            {
                hoveredInteractable.SetOutline(true);
            }
        }
    }
}
