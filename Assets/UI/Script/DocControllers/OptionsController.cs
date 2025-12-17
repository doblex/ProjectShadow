using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class OptionsController : BaseDocController
{
    [SerializeField] Options options;
    [SerializeField] AudioMixer mixer;

    Button backButton;
    protected override bool SetComponents()
    {
        bool bInit = base.SetComponents();

        backButton = Root.Q<Button>("Back");
        backButton.clicked += Back;

        return bInit;
    }
    private void Back()
    {
        OnValidate();
        ShowDoc(false);
    }

    private void OnValidate()
    {
        OptionsData data = options.ToData();

        mixer.SetFloat("MasterVolume", Mathf.Log10(data.generalVolume) * 20);
        mixer.SetFloat("MusicVolume", Mathf.Log10(data.musicVolume) * 20);
        mixer.SetFloat("SFXVolume", Mathf.Log10(data.sfxVolume) * 20);

        Screen.fullScreen = data.isFullScreen;
        Screen.SetResolution(data.resolution.width, data.resolution.height, data.isFullScreen);
    }
}
