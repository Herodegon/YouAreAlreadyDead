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

    public string WillText { get { return willInputField.text;}}

    private float inputDelayTimer = 0f;
    public bool isInputLocked = false;

    public event Action<string> OnWillContentChanged;

    private Tween punchTween;

    void OnEnable()
    {
        willInputField.onValueChanged.AddListener(OnValueChanged);
        willInputField.text = "";
        willInputField.interactable = true;
    }

    void OnDisable()
    {
        willInputField.onValueChanged.RemoveListener(OnValueChanged);
    }

    void Update()
    {
        if (isInputLocked)
        {
            if (willInputField.interactable)
            {
                willInputField.interactable = false;
            }
            return;
        }
        
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
        AudioBus.Instance.PlaySFX("pencil_on_paper", canOverlap: true);
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
