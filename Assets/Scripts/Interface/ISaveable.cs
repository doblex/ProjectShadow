using UnityEngine;

public interface ISaveable
{
    string ID { get; }

    public object Save();

    public void Load(string stateJson);
}
