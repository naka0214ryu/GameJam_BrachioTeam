using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class AudioData {
    public string key;
    public AudioClip clip;
}
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
            }
            return _instance;
        }
    }

    [SerializeField] AudioMixer audioMixer;

    [SerializeField] AudioSource bgmSource;
    [SerializeField] List<AudioSource> seSources;

    [SerializeField, Header("bgmClip")] List<AudioData> bgmClips;
    [SerializeField, Header("seClip")] List<AudioData> seClips;

    Dictionary<string, AudioClip> bgmDict;
    Dictionary<string, AudioClip> seDict;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        bgmDict = new Dictionary<string, AudioClip>();
        foreach (var data in bgmClips)
        {
            bgmDict[data.key] = data.clip;
        }

        seDict = new Dictionary<string, AudioClip>();
        foreach (var data in seClips)
        {
            seDict[data.key] = data.clip;
        }
    }

    public void SetBGMVolume(float value) {
        audioMixer.SetFloat("BGM" , Mathf.Log10(Mathf.Max(value,0.0001f)) * 20f);
    }    
    
    public void SetSEVolume(float value) {
        audioMixer.SetFloat("SE" , Mathf.Log10(Mathf.Max(value,0.0001f)) * 20f);
    }

    public float GetBGMVolume()
    {
        audioMixer.GetFloat("BGM", out float dB);
        return Mathf.Pow(10f, dB / 20f);
    }    
    
    public float GetSEVolume()
    {
        audioMixer.GetFloat("SE", out float dB);
        return Mathf.Pow(10f, dB / 20f);
    }

    public void PlayBGM(string name, bool loop = true)
    {
        if (bgmDict.TryGetValue(name, out var clip))
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }
    }


    public void PlaySE(string name)
    {
        if (seDict.TryGetValue(name, out var clip))
        {
            AudioSource source = GetFreeSeSource();

            if (source != null)
            {
                source.clip = clip;
                source.loop = false;
                source.Play();
            }
        }
    }

    public void PlayLoop(string name)
    {
        if (seDict.TryGetValue(name, out var clip))
        {
            AudioSource source = GetFreeSeSource();

            if (source != null)
            {
                source.clip = clip;
                source.loop = true;
                source.Play();
            }
        }
    }

    public void StopBGM() => bgmSource.Stop();

    public void StopAllSE()
    {
        foreach (var source in seSources)
        {
            source.Stop();
        }
    }

    private AudioSource GetFreeSeSource()
    {
        foreach (var source in seSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        return seSources.Count > 0 ? seSources[0] : null;
    }
}
