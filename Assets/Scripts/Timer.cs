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
    private int tollCount;
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
        BellTollStateMachine();
    }

    public void SetTimer(float duration)
    {
        if (scrollTween.isAlive) scrollTween.Complete();
        ResetTimer();
        timer = duration;
        scrollTween = Tween.PositionY(scrollTransform, endPoint.position.y, duration, ease: Ease.Linear)
        .OnComplete(() => OnTimerComplete?.Invoke());
    }

    private void BellTollStateMachine()
    {
        switch (tollCount)
        {
            case 0:
                if (timer > 10f) return;
                AudioBus.Instance.PlaySFX("bell_toll");
                tollCount++;
                break;
            case 1:
                if (timer > 5f) return;
                AudioBus.Instance.PlaySFX("bell_toll");
                tollCount++;
                break;
            default:
                break;
        }
    }

    private void ResetTimer()
    {
        timer = 0f;
        tollCount = 0;
        scrollTransform.position = startPoint.position;
    }
}
