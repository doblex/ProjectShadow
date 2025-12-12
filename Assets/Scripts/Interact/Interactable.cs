using UnityEngine;

public interface Interactable
{
    void Interact();
}

public interface BooleanInteractable : Interactable
{
    bool Active { get; set; }
}

