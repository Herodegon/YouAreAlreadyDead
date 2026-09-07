using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioPool : MonoBehaviour
{
    public static AudioPool Instance { get; private set;}

    public Transform inactiveContainer;
    public Transform activeContainer;

    public readonly Dictionary<string, List<GameObject>> audioPool = new();

    private readonly HashSet<AudioSource> pausedSources = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        IndexExistingSources();
    }

    public AudioSource GetAudioSource(string name)
    {
        if (!audioPool.ContainsKey(name)) return null;
        var source = audioPool[name].Where(s => !s.activeInHierarchy).FirstOrDefault();
        if (source == null) return null;
        return source.GetComponent<AudioSource>();
    }

    public void PlayAudioSource(AudioSource source, AudioClip clip, bool loop = false, float delay = 0f)
    {
        source.gameObject.SetActive(true);
        source.transform.SetParent(activeContainer, false);
        source.clip = clip;
        source.loop = loop;
        if (delay <= 0f)
        {
            source.Play();
        }
        else
        {
            // PlayScheduled works in absolute audio engine time, not offsets.
            source.PlayScheduled(AudioSettings.dspTime + delay);
        }
        StartCoroutine(ReleaseRoutine(source, delay));
    }

    public void PauseAudioSource(AudioSource source)
    {
        if (source == null || !source.isPlaying) return;
        pausedSources.Add(source);
        source.Pause();
    }

    public void ResumeAudioSource(AudioSource source)
    {
        if (source == null || !pausedSources.Remove(source)) return;
        source.UnPause();
    }

    public bool IsPaused(AudioSource source)
    {
        return source != null && pausedSources.Contains(source);
    }

    private IEnumerator ReleaseRoutine(AudioSource source, float delay)
    {
        // Don't poll isPlaying until the scheduled start has passed, or the
        // source gets recycled out from under the pending playback.
        if (delay > 0f) yield return new WaitForSeconds(delay);
        yield return null;
        yield return new WaitWhile(() => source.isPlaying || pausedSources.Contains(source));
        ReleaseAudioSource(source);
    }

    private void ReleaseAudioSource(AudioSource source)
    {
        pausedSources.Remove(source);
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
        source.transform.SetParent(inactiveContainer, false);
    }

    private void IndexExistingSources()
    {
        foreach (var source in inactiveContainer.GetComponentsInChildren<AudioSource>())
        {
            string name = source.name.Split(' ')[0];
            if (!audioPool.ContainsKey(name))
            {
                audioPool[name] = new List<GameObject>();
            }
            source.gameObject.SetActive(false);
            audioPool[name].Add(source.gameObject);
        }
    }
}
