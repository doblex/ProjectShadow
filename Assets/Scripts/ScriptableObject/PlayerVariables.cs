using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "PlayerVariables", menuName = "Scriptable Objects/PlayerVariables")]
public class PlayerVariables : ScriptableObject
{
    public float moveSpeed = 5f;
    public float dashSpeed = 10f;

    public SerializableDictionary<string, float> AreaSpeedModifier = new SerializableDictionary<string, float>();

    private void Awake()
    {
        string[] areaNames = NavMesh.GetAreaNames();

        foreach (string areaName in areaNames)
        {
            AreaSpeedModifier.Add(areaName, 0f);
        }
    }
}
