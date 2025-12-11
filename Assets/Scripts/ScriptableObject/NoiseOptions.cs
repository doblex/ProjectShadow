using UnityEngine;

[CreateAssetMenu(fileName = "NoiseOptions", menuName = "Scriptable Objects/NoiseOptions")]
public class NoiseOptions : ScriptableObject
{
    public float duration = 2;
    public float startSize = 10;
    public ParticleSystem.MinMaxCurve sizeOverLifeTime = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, 0, 1, 1));

    public AudioClip SFX = null;
};


