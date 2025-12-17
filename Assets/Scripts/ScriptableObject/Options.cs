using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct OptionsData
{
    public float generalVolume;
    public float musicVolume;
    public float sfxVolume;
    public bool isFullScreen;
    public Resolution resolution;
}

[CreateAssetMenu(fileName = "Options", menuName = "Scriptable Objects/Options")]
public class Options : ScriptableObject
{
    [SerializeField][Range(0, 1)] float generalVolume = 1f;
    [SerializeField][Range(0, 1)] float musicVolume = 1f;
    [SerializeField][Range(0, 1)] float sfxVolume = 1f;

    [SerializeField] bool isFullScreen = true;

    public int selectedResolutionIndex = 0;
    public List<string> resolutions;

    private void OnEnable()
    {

        foreach (Resolution res in Screen.resolutions)
        {
            resolutions.Add(res.ToString());
        }
    }

    public OptionsData ToData()
    {
        OptionsData data = new OptionsData
        {
            generalVolume = generalVolume,
            musicVolume = musicVolume,
            sfxVolume = sfxVolume,
            isFullScreen = isFullScreen,
            resolution = Screen.resolutions[selectedResolutionIndex]
        };
        return data;
    }
}
