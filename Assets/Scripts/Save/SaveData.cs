using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DynamicObjectData
{
    public string id;
    public string prefabKey; // The name to find the prefab
    public string jsonState;
    public Vector3 position; // Helper to spawn it at right spot immediately
    public Quaternion rotation; // Helper to spawn it with right rotation immediately
}

[Serializable]
public class SaveData
{
    public List<string> keys = new List<string>();
    public List<string> values = new List<string>();

    public List<DynamicObjectData> dynamicObjects = new List<DynamicObjectData>();

    public void Add(string key, string value)
    {
        keys.Add(key);
        values.Add(value);
    }

    public bool TryGetValue(string key, out string value)
    {
        int index = keys.IndexOf(key);
        if (index != -1)
        {
            value = values[index];
            return true;
        }
        value = null;
        return false;
    }
}