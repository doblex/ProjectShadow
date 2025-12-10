using UnityEngine;

[CreateAssetMenu(fileName = "SoundOptions", menuName = "Scriptable Objects/SoundOptions")]
public class SoundOptions : ScriptableObject
{
    public float duration = 2;
    public float startSize = 10; 
    public ParticleSystem.MinMaxCurve sizeOverLifeTime;
}
