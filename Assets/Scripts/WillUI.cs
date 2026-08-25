using UnityEngine;
using TMPro;
using System;

public class WillUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField willInputField;
    [SerializeField] private float inputDelay = 0.5f;

    private float inputDelayTimer = 0f;

    public event Action<string> OnWillContentChanged;

    void Awake()
    {
        willInputField.onValueChanged.AddListener(OnValueChanged);
    }

    void Update()
    {
        if (Time.time > inputDelayTimer)
        {
            AudioBus.Instance.PauseSFX("pencil_on_paper");
        }
    }

    private void OnValueChanged(string value)
    {
        AudioBus.Instance.PlaySFX("pencil_on_paper");
        inputDelayTimer = Time.time + inputDelay;
        OnWillContentChanged?.Invoke(value);
    }
}
