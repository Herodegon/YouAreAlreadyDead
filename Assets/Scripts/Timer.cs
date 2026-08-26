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

    private Tween scrollTween;

    void Awake()
    {
        Instance = this;
    }

    public void SetTimer(float duration)
    {
        if (scrollTween.isAlive) scrollTween.Complete();
        scrollTransform.position = startPoint.position;
        scrollTween = Tween.PositionY(scrollTransform, endPoint.position.y, duration, ease: Ease.Linear)
        .OnComplete(() => OnTimerComplete?.Invoke());
    }
}
