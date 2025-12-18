using UnityEngine;

[RequireComponent(typeof(Highlighter))]
public class IrreplacableBaitPickup : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        AbilityController controller = FindFirstObjectByType<AbilityController>();

        if (controller != null)
        {
            controller.AddIBait();
            Destroy(gameObject);
        }
    }
}
