using UnityEngine;
using TMPro;
using System;

public class WillUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField willInputField;

    public event Action<string> OnWillContentChanged;
    int prevNumberOfCharacters = 0;

    void Awake()
    {
        willInputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(string value)
    {
        OnWillContentChanged?.Invoke(value);
    }
}
