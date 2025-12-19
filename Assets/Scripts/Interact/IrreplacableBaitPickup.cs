using UnityEngine;

[RequireComponent(typeof(Highlighter))]
public class IrreplacableBaitPickup : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Interact works");
        AbilityController controller = FindFirstObjectByType<AbilityController>();

        if (controller != null)
        {
            controller.AddIBait();
            Destroy(gameObject);
        }
    }
}
