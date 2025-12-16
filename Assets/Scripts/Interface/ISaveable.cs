using UnityEngine;

public interface ISaveable
{
    public string ID { get; }

    public object Save();

    public void Load(string stateJson);
}
