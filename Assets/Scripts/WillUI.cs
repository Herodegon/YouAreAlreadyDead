using UnityEngine;
using TMPro;
using System;

public class WillUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField willInputField;

    public event Action<string> OnWhitespaceEntered;
    int prevNumberOfCharacters = 0;

    void Awake()
    {
        willInputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(string value)
    {
        if (value.Length == 0) return;
        char newChar = value[^1];
        if (newChar == ' ' || value.Length < prevNumberOfCharacters)
        {
            OnWhitespaceEntered?.Invoke(value);
        }
        prevNumberOfCharacters = value.Length;
    }
}
