using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] AudioClip clip;

    public void PlaySound()
    {
        if(clip == null) return;

        SoundManager.Instance?.PlaySFX(clip);
    }
}
