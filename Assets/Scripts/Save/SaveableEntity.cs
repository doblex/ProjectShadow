using UnityEngine;
using System;

[RequireComponent(typeof(ISaveable))] // Enforce the logic component exists
public class SaveableEntity : MonoBehaviour
{
    [SerializeField] private string id = "";

    [Tooltip("If true, this object will be re-instantiated on load.")]
    [SerializeField] private bool isDynamic = false;

    [Tooltip("The Key used to find this prefab in the SaveSystem registry.")]
    [SerializeField] private string prefabKey = "";

    public string ID => id;
    public bool IsDynamic => isDynamic;
    public string PrefabKey => prefabKey;

    [ContextMenu("Generate ID")]
    private void GenerateID() => id = Guid.NewGuid().ToString();

    // Ensure ID is not empty
    private void Awake()
    {
        if (string.IsNullOrEmpty(id)) GenerateID();
    }

    public void SetId(string value)
    {
        id = value;
    }
}