using UnityEngine;
using TMPro;
using System;
using PrimeTween;

public class WillUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField willInputField;
    [SerializeField] private float inputDelay = 0.5f;

    [Header("Shake")]
    [SerializeField] private float punchStrength = 8f;
    [SerializeField] private float punchDuration = 0.15f;
    [SerializeField] private float punchFrequency = 15f;

    private float inputDelayTimer = 0f;
    public bool isInputLocked = false;

    public event Action<string> OnWillContentChanged;

    private Tween punchTween;

    void Awake()
    {
        willInputField.onValueChanged.AddListener(OnValueChanged);
        willInputField.text = "";
    }

    void Update()
    {
        if (isInputLocked) return;
        if (!willInputField.isFocused) 
        {
            willInputField.Select();
        }
        if (Time.time > inputDelayTimer)
        {
            AudioBus.Instance.PauseSFX("pencil_on_paper");
        }
    }

    private void OnValueChanged(string value)
    {
        if (isInputLocked) return;
        AudioBus.Instance.PlaySFX("pencil_on_paper");
        inputDelayTimer = Time.time + inputDelay;
        ShakeText();
        OnWillContentChanged?.Invoke(value);
    }

    private void ShakeText()
    {
        if (punchTween.isAlive) return;
        Vector3 punchDir = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f).normalized * punchStrength;
        punchTween = Tween.PunchLocalPosition(willInputField.transform, punchDir, punchDuration, punchFrequency);
    }
}
