using System.Collections.Generic;
using UnityEngine;

public class NoiseSpawnerManager : MonoBehaviour
{
    public static NoiseSpawnerManager Instance;

    [SerializeField] private GameObject noiseOriginPrefab;
    [SerializeField] AudioClip clip;

    private List<GameObject> NoiseOrigins = new List<GameObject>();

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
    }

    public void SpawnNoiseOrigin(Vector3 position, SoundOptions options)
    {
        GameObject noiseOrigin = GetPooledNoiseOrigin();

        noiseOrigin.SetActive(true);

        noiseOrigin.transform.SetParent(transform);

        if (noiseOrigin.TryGetComponent<NoiseOrigin>(out var noiseOriginScript))
        {
            noiseOriginScript.Setup(options, position);
            noiseOriginScript.PlayEffect();
        }

        if (clip != null)
        { 
           SoundManager.Instance.PlaySFXOnPos(clip, position);
        }
    }

    private GameObject GetPooledNoiseOrigin()
    {
        foreach (var noiseOrigin in NoiseOrigins)
        {
            if (!noiseOrigin.activeInHierarchy)
            {
                return noiseOrigin;
            }
        }
        GameObject newNoiseOrigin = Instantiate(noiseOriginPrefab);
        NoiseOrigins.Add(newNoiseOrigin);
        return newNoiseOrigin;
    }
}
