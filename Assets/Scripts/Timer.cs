using UnityEngine;
using PrimeTween;
using System;

public class Timer : MonoBehaviour
{
    public static Timer Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Transform scrollTransform;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    public event Action OnTimerComplete;

    private float timer;
    private Tween scrollTween;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        ResetTimer();
    }

    void Update()
    {
        if (timer > 0f) timer -= Time.deltaTime;
    }

    public void SetTimer(float duration)
    {
        if (scrollTween.isAlive) scrollTween.Complete();
        ResetTimer();
        timer = duration;
        float tickLength = AudioBus.Instance.GetClipLength("clock_ticking");
        float delay = duration % tickLength;
        AudioBus.Instance.PlaySFX("clock_ticking", delay);
        AudioBus.Instance.PlaySFX("bell_toll", duration - tickLength);
        AudioBus.Instance.PlaySFX("bell_toll", duration - tickLength*2f);

        scrollTween = Tween.PositionY(scrollTransform, endPoint.position.y, duration, ease: Ease.Linear)
        .OnComplete(() => {
            AudioBus.Instance.PlaySFX("bell_toll");
            AudioBus.Instance.StopSFX("clock_ticking");
            Background.Instance.Flash(0.5f);
            OnTimerComplete?.Invoke();
        });
    }

    public void ResetTimer()
    {
        timer = 0f;
        scrollTransform.position = startPoint.position;
    }
}
