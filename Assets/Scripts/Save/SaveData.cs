using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public List<string> keys = new List<string>();
    public List<string> values = new List<string>();

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