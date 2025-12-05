using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "PlayerVariables", menuName = "Scriptable Objects/PlayerVariables")]
public class PlayerVariables : ScriptableObject
{
    [Header("Walk movement")]
    public float walkMoveSpeed = 5f;
    public float walkSoundInterval = 1f;
    public SoundOptions walkSoundOptions;

    [Header("Crouch movement")]
    public float crouchSpeed = 2.5f;

    [Header("Dash movement")]
    public float dashSpeed = 10f;
    public float DashSoundInterval = 0.2f;
    public SoundOptions dashSoundOptions;

}
