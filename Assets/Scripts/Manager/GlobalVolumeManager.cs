using UnityEngine;
using UnityEngine.Rendering;

public class GlobalVolumeManager : MonoBehaviour
{
    public static GlobalVolumeManager Instance;

    Volume volumeComponent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        volumeComponent = GetComponent<Volume>();

        SetHiding(false);
    }


    public void SetHiding(bool isHiding)
    { 
        volumeComponent.weight = isHiding ? 1f : 0.3f;
    }
}
