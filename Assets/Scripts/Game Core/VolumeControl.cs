using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/*[System.Serializable]
public class VolumePack
{
    public AudioSource aud;
    public float volume;
}*/

public class VolumeControl : MonoBehaviour
{
    public float currentVolume;
    public float maxVolume = 2.0f; //can bring volume up to 200%. I don't see why you'd ever want to do that though.
    public static float storedVolume = 1.0f;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;
    public List<AudioSource> allAudioSources;
    // [SerializeField]
    //public VolumePack[] audioSourceVolumeTracker;
    public List<float> originalAudioVolumes;
   
    void Awake()
    {
        InitializeAudioSourceTrackers();
    }

    void InitializeAudioSourceTrackers()
    {
        originalAudioVolumes = new List<float>();
        for (int i = 0; i < allAudioSources.Count; i++)
        {
            originalAudioVolumes.Add(allAudioSources[i].volume);
        }
    }

    void Start()
    {
        currentVolume = storedVolume;
        volumeSlider.value = currentVolume;
        volumeSlider.maxValue = maxVolume;
        OnVolumeChanged(currentVolume);
    }

    void OnDisable()
    {
        storedVolume = currentVolume;
    }

    public void OnVolumeChanged()
    {
        OnVolumeChanged(volumeSlider.value);
    }

    public void OnVolumeChanged(float newVolume)
    {
        currentVolume = newVolume;
        volumeText.text = GameMaster.StandardRounding(currentVolume * 100) + "%";
        for(int i = 0; i < allAudioSources.Count; i++)
        {
            allAudioSources[i].volume = originalAudioVolumes[i] * newVolume;
        }
    }

    public void RegisterNewAudioSource(AudioSource aud)
    {
        if (!aud) return;
        if (allAudioSources.Contains(aud)) return;

        allAudioSources.Add(aud);
        originalAudioVolumes.Add(aud.volume);
        aud.volume *= currentVolume;
    }
}
