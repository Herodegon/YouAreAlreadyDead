using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

[RequireComponent(typeof(Image))]
public class Background : MonoBehaviour
{
    public static Background Instance { get; private set; }

    private Image backgroundImage;

    void Awake()
    {
        Instance = this;
        backgroundImage = GetComponent<Image>();
    }
    
    [Tooltip("Flash the background for a given duration in seconds.")]
    public void Flash(float duration, int flashFactor = 6)
    {
        Color startColor = backgroundImage.color;
        float initialFlashDuration = duration/flashFactor;
        float remainingFlashDuration = duration - initialFlashDuration;
        Tween.Custom(0f, 1f, initialFlashDuration, (value) => 
        {
            backgroundImage.color = new Color(startColor.r, startColor.g, startColor.b, value);
        })
        .Chain(Tween.Custom(1f, 0f, remainingFlashDuration, (value) =>
        {
            backgroundImage.color = new Color(startColor.r, startColor.g, startColor.b, value);
        }));
    }

    public void FadeIn(float duration)
    {
        Color startColor = backgroundImage.color;
        Tween.Custom(0f, 1f, duration, (value) =>
        {
            backgroundImage.color = new Color(startColor.r, startColor.g, startColor.b, value);
        }, ease: Ease.InExpo);
    }
}
