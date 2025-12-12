using UnityEngine;

public class PickupObject : MonoBehaviour, Interactable
{
    //[SerializeField] InventoryItem pickup;
    public void Interact()
    {
        //Will add specific item to inventory when implemented

        Destroy(gameObject);
    }
}
