using UnityEngine;
using System.Collections.Generic;

using Random = UnityEngine.Random;

[System.Serializable]
public class AudioSourceValue
{
    public string name;
    public AudioSource source;
}

[System.Serializable]
public class AudioClipValue
{
    public string name;
    public AudioClipData data;
}

[System.Serializable]
public class AudioClipData
{
    public string audioSourceName;
    public float volume;
    public float pitch;
    public float pitchVariance;
    public List<AudioClip> clips;
}

public class AudioBus : MonoBehaviour
{
    public static AudioBus Instance { get; set; }

    public List<AudioSourceValue> audioSourceValues = new();
    public List<AudioClipValue> audioClipValues = new();
    private readonly Dictionary<string, AudioSource> audioSources = new();
    private readonly Dictionary<string, AudioClipData> audioClips = new();

    void Awake()
    {
        Instance = this;
        foreach (var audioSourceValue in audioSourceValues)
        {
            audioSources.Add(audioSourceValue.name, audioSourceValue.source);
        }
        foreach (var audioClipValue in audioClipValues)
        {
            audioClips.Add(audioClipValue.name, audioClipValue.data);
        }
    }

    public void PlaySFX(string name)
    {
        if (audioClips.TryGetValue(name, out AudioClipData data))
        {
            if (audioSources.TryGetValue(data.audioSourceName, out AudioSource source))
            {
                source.UnPause();
                if (source.isPlaying) return;
                int index = Random.Range(0, data.clips.Count);
                source.volume = data.volume;
                source.pitch = data.pitch + Random.Range(-data.pitchVariance, data.pitchVariance);
                source.PlayOneShot(data.clips[index]);
            }
        }
    }

    public void StopSFX(string name)
    {
        if (audioClips.TryGetValue(name, out AudioClipData data))
        {
            if (audioSources.TryGetValue(data.audioSourceName, out AudioSource source))
            {
                source.Stop();
            }
        }
    }

    public void PauseSFX(string name)
    {
        if (audioClips.TryGetValue(name, out AudioClipData data))
        {
            if (audioSources.TryGetValue(data.audioSourceName, out AudioSource source))
            {
                source.Pause();
            }
        }
    }

    public bool IsPlaying(string name)
    {
        if (audioClips.TryGetValue(name, out AudioClipData data))
        {
            if (audioSources.TryGetValue(data.audioSourceName, out AudioSource source))
            {
                return source.isPlaying;
            }
        }
        return false;
    }
}
