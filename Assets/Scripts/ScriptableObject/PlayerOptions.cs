using UnityEngine;

[CreateAssetMenu(fileName = "Options", menuName = "Scriptable Objects/Options")]
public class PlayerOptions : ScriptableObject
{
    [SerializeField, Range(0f, 10f)] private float _moveSpeed = 5f;
    [SerializeField, Range(0f, 10f)] private float _angleSpeed = 5f;

    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
    public float AngleSpeed { get => _angleSpeed; set => _angleSpeed = value; }
}
