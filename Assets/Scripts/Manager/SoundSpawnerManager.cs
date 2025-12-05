using System.Collections.Generic;
using UnityEngine;

public class SoundSpawnerManager : MonoBehaviour
{
    public static SoundSpawnerManager Instance;

    [SerializeField] private GameObject soundOriginPrefab;

    private List<GameObject> SoundOrigins = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void SpawnSoundOrigin(Vector3 position, SoundOptions options)
    {
        GameObject soundOrigin = GetPooledSoundOrigin();

        soundOrigin.SetActive(true);

        soundOrigin.transform.SetParent(transform);

        SoundOrigin soundOriginScript = soundOrigin.GetComponent<SoundOrigin>();

        if (soundOriginScript != null)
        {
            soundOriginScript.Setup(options, position);
            soundOriginScript.PlayEffect();
        }
    }

    private GameObject GetPooledSoundOrigin()
    {
        foreach (var soundOrigin in SoundOrigins)
        {
            if (!soundOrigin.activeInHierarchy)
            {
                return soundOrigin;
            }
        }
        GameObject newSoundOrigin = Instantiate(soundOriginPrefab);
        SoundOrigins.Add(newSoundOrigin);
        return newSoundOrigin;
    }
}
