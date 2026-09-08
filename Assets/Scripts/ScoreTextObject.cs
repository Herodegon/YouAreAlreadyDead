using UnityEngine;
using TMPro;
using PrimeTween;
using System;

using Random = UnityEngine.Random;

public class ScoreTextObject : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Color positiveColor;
    [SerializeField] private Color negativeColor;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float floatUpDuration = 0.25f;
    [SerializeField] [Min(15f)]private float floatUpDistance = 100f;

    public event Action<GameObject> OnFinished;

    void OnEnable()
    {
        effectText.color = positiveColor;
        scoreText.color = positiveColor;
        FadeInOut();
        FloatUp();
    }

    public void UpdateText(string text, int score = 0)
    {
        effectText.text = text;

        if (score == 0) 
        {
            scoreText.gameObject.SetActive(false);
            return;
        }

        char operatorSymbol = '+';
        Color scoreColor = positiveColor;
        if (score < 0)
        {
            operatorSymbol = '-';
            scoreColor = negativeColor;
        }
        scoreText.text = $"{operatorSymbol}{score}";
        scoreText.color = scoreColor;
    }

    private void FadeInOut()
    {
        float initialFadeDuration = fadeDuration/4f;
        float remainingFadeDuration = fadeDuration - initialFadeDuration;
        Tween.Custom(0f, 1f, duration: initialFadeDuration, (value) =>
        {
            effectText.color = new Color(effectText.color.r, effectText.color.g, effectText.color.b, value);
            scoreText.color = new Color(scoreText.color.r, scoreText.color.g, scoreText.color.b, value);
        }, ease: Ease.Linear)
        .Chain(Tween.Custom(1f, 0f, duration: remainingFadeDuration, (value) =>
        {
            effectText.color = new Color(effectText.color.r, effectText.color.g, effectText.color.b, value);
            scoreText.color = new Color(scoreText.color.r, scoreText.color.g, scoreText.color.b, value);
        }, ease: Ease.Linear))
        .OnComplete(() => OnFinished?.Invoke(gameObject));
    }

    private void FloatUp()
    {
        float initialY = transform.position.y;
        float randomY = Random.Range(15f, floatUpDistance);
        Tween.Custom(initialY, initialY + randomY, duration: floatUpDuration, (value) =>
        {
            transform.position = new Vector3(transform.position.x, value, transform.position.z);
        }, ease: Ease.OutSine);
    }
}
