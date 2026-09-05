using UnityEngine;
using System.Collections.Generic;

using Random = UnityEngine.Random;

[System.Serializable]
public class AudioSourceValue
{
    public string name;
    public AudioPool sourcePool;
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
    private readonly Dictionary<string, AudioPool> audioPools = new();
    private readonly Dictionary<string, AudioClipData> audioClips = new();

    private readonly Dictionary<string, AudioSource> activeSources = new();
    private readonly List<string> finishedNames = new();

    void Awake()
    {
        Instance = this;
        foreach (var audioSourceValue in audioSourceValues)
        {
            audioPools.Add(audioSourceValue.name, audioSourceValue.sourcePool);
        }
        foreach (var audioClipValue in audioClipValues)
        {
            audioClips.Add(audioClipValue.name, audioClipValue.data);
        }
    }

    void Update()
    {
        finishedNames.Clear();
        foreach (var pair in activeSources)
        {
            if (pair.Value.clip == null)
            {
                finishedNames.Add(pair.Key);
            }
        }
        foreach (var name in finishedNames)
        {
            activeSources.Remove(name);
        }
    }

    public void PlaySFX(string name)
    {
        if (!audioClips.TryGetValue(name, out AudioClipData data)) return;
        if (!audioPools.TryGetValue(data.audioSourceName, out AudioPool pool)) return;

        if (activeSources.TryGetValue(name, out AudioSource active))
        {
            pool.ResumeAudioSource(active);
            return;
        }

        AudioSource source = pool.GetAudioSource(data.audioSourceName);
        if (source == null) return;

        int index = Random.Range(0, data.clips.Count);
        source.volume = data.volume;
        source.pitch = data.pitch + Random.Range(-data.pitchVariance, data.pitchVariance);
        pool.PlayAudioSource(source, data.clips[index]);
        activeSources[name] = source;
    }

    public void StopSFX(string name)
    {
        if (!audioClips.TryGetValue(name, out AudioClipData data)) return;
        if (!audioPools.TryGetValue(data.audioSourceName, out AudioPool pool)) return;
        if (!activeSources.TryGetValue(name, out AudioSource source)) return;

        pool.ResumeAudioSource(source);
        source.Stop();
        activeSources.Remove(name);
    }

    public void PauseSFX(string name)
    {
        if (!audioClips.TryGetValue(name, out AudioClipData data)) return;
        if (!audioPools.TryGetValue(data.audioSourceName, out AudioPool pool)) return;
        if (!activeSources.TryGetValue(name, out AudioSource source)) return;

        pool.PauseAudioSource(source);
    }

    public void ResumeSFX(string name)
    {
        if (!audioClips.TryGetValue(name, out AudioClipData data)) return;
        if (!audioPools.TryGetValue(data.audioSourceName, out AudioPool pool)) return;
        if (!activeSources.TryGetValue(name, out AudioSource source)) return;

        pool.ResumeAudioSource(source);
    }

    public bool IsPlaying(string name)
    {
        return activeSources.TryGetValue(name, out AudioSource source) && source.isPlaying;
    }

    public bool IsPaused(string name)
    {
        if (!audioClips.TryGetValue(name, out AudioClipData data)) return false;
        if (!audioPools.TryGetValue(data.audioSourceName, out AudioPool pool)) return false;
        if (!activeSources.TryGetValue(name, out AudioSource source)) return false;

        return pool.IsPaused(source);
    }
}
