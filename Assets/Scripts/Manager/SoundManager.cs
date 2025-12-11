using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEditor.PlayerSettings;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] AudioMixerGroup sfxMixerGroup;
    [SerializeField] AudioMixerGroup musicMixerGroup;

    // SFX management
    [SerializeField] AudioSource sfxSource;

    // OST management
    [SerializeField] AudioSource ostSouce;
    [SerializeField] AudioClip ostClip;
    [SerializeField] AudioClip muffledOstClip;

    //SFX Pool
    private List<GameObject> SFXObjs = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;

        ostSouce.clip = ostClip;
        ostSouce.playOnAwake = true;
        ostSouce.loop = true;
        ostSouce.outputAudioMixerGroup = musicMixerGroup;
    }

    public void PlaySFXOnPos(AudioClip clip, Vector3 pos)
    {
        GameObject tempGO = GetPooledSFXObj();

        tempGO.SetActive(true);

        tempGO.transform.position = pos;

        if (tempGO.TryGetComponent<AudioSource>(out AudioSource audioSource))
        {
            audioSource.clip = clip;
            audioSource.Play();
        }


        StartCoroutine(SetInactiveAfterSeconds(tempGO, clip.length + 0.1f));
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void StopMusic() 
    {
        ostSouce.Stop();
    }

    public void ChangeOstOnCrouch(bool isCrouching)
    { 
        float duration = ostSouce.time;

        ostSouce.clip = isCrouching ? muffledOstClip : ostClip;
        ostSouce.time = duration;
        ostSouce.Play();
    }

    private GameObject GetPooledSFXObj()
    {
        foreach (var SFXObj in SFXObjs)
        {
            if (!SFXObj.activeInHierarchy)
            {
                return SFXObj;
            }
        }

        GameObject pooledSfx = new GameObject("sfx");

        pooledSfx.transform.parent = transform;

        AudioSource tempSource = pooledSfx.AddComponent<AudioSource>();
        tempSource.outputAudioMixerGroup = sfxMixerGroup;
        tempSource.spatialBlend = 1.0f;
        tempSource.playOnAwake = false;
        tempSource.loop = false;

        SFXObjs.Add(pooledSfx);
        return pooledSfx;
    }

    private IEnumerator SetInactiveAfterSeconds(GameObject go, float time)
    { 
        yield return new WaitForSeconds(time);
        go.SetActive(false);
    }
}
