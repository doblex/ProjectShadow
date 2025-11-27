using UnityEngine;
using UnityEngine.Audio;

public class ProveMusica : MonoBehaviour
{
    [SerializeField] AudioResource firstSong;
    [SerializeField] AudioResource secondSong;

    AudioSource audioSource;
    AudioResource currentSong;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = 0.5f;

        currentSong = firstSong;

        audioSource.resource = currentSong;
    }

    private void Start()
    {
        audioSource.Play();
        ActionManager.Instance.onInteract += () => ChangeSong(currentSong == firstSong ? secondSong : firstSong);
    }

    private void ChangeSong(AudioResource nextSong)
    {
        float currentTime = audioSource.time;

        currentSong = nextSong;
        audioSource.resource = currentSong;
        audioSource.time = currentTime;
        audioSource.Play();
        

        Debug.Log("Cambiata canzone a: " + currentSong.name);
    }
}
