using UnityEngine;

[RequireComponent(typeof(Outline))]
public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    private Outline outline;

    protected virtual void Awake()
    {
        outline = GetComponent<Outline>();
    }

    public virtual void Interact()
    {
        Destroy(gameObject);
    }

    public virtual string GetPrompt()
    {
        return "Default Prompt";
    }

    public void SetOutline(bool set)
    {
        outline.enabled = set;
    }
}
