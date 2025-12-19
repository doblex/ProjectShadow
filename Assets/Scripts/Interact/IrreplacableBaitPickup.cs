using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Highlighter))]
public class IrreplacableBaitPickup : MonoBehaviour, IInteractable, ISaveable
{
    private string id;

    public string ID => id;

    private void Awake()
    {
        id = System.Guid.NewGuid().ToString();
    }

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

    private struct IBaitPickupData
    {
        public Vector3 position;
        public Vector3 rotation;
    }

    public void Load(string stateJson)
    {
        IBaitPickupData data = JsonUtility.FromJson<IBaitPickupData>(stateJson);
        
        transform.position = data.position;
        transform.eulerAngles = data.rotation;
    }

    public object Save()
    {
        return new IBaitPickupData
        {
            position = transform.position,
            rotation = transform.eulerAngles,
        };
    }
}
