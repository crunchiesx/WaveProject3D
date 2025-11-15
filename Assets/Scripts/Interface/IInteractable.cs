using System;
using UnityEngine;

public interface IInteractable
{
    public void Interact();

    public string GetPrompt();

    public void SetOutline(bool set);
}
