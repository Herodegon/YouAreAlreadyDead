using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using PrimeTween;

public class WillUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField willInputField;
    [SerializeField] private Transform textContainer;
    [SerializeField] private float inputDelay = 0.5f;
    [SerializeField] private int maxLineCount = 20;

    [Header("Shake")]
    [SerializeField] private float punchStrength = 8f;
    [SerializeField] private float punchDuration = 0.15f;
    [SerializeField] private float punchFrequency = 15f;

    public string WillText { get { return willInputField.text;}}

    public bool isInputLocked = false;
    private float inputDelayTimer = 0f;
    private int currentPageNumber = 1;

    public event Action<string> OnWillContentChanged;

    private Tween punchTween;
    private Sequence spinTween;

    private bool debug_autoInput = false;

    void OnEnable()
    {
        willInputField.onValueChanged.AddListener(OnValueChanged);
        willInputField.textComponent.pageToDisplay = 1;
        willInputField.text = "";
        currentPageNumber = 1;
        willInputField.interactable = true;
        willInputField.textComponent.ForceMeshUpdate();
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
        else if (!willInputField.interactable)
        {
            willInputField.interactable = true;
        }
        
        if (!willInputField.isFocused) 
        {
            willInputField.Select();
        }
        if (Time.time > inputDelayTimer)
        {
            AudioBus.Instance.PauseSFX("pencil_on_paper");
        }

        if (Keyboard.current.semicolonKey.wasPressedThisFrame)
        {
            debug_autoInput = !debug_autoInput;
        }
        if (debug_autoInput)
        {
            // Assigning text already raises onValueChanged, which is what drives the parse.
            char randomChar = (char)UnityEngine.Random.Range(96, 123);
            willInputField.text += randomChar.ToString();
        }
    }

    void LateUpdate()
    {
        CheckRotation();
    }

    private void OnValueChanged(string value)
    {
        if (isInputLocked) return;
        int lineCount = willInputField.textComponent.textInfo.lineCount;
        if (lineCount > currentPageNumber * maxLineCount)
        {
            SpinWill();
            return;
        }
        AudioBus.Instance.PlaySFX("pencil_on_paper", canOverlap: true);
        inputDelayTimer = Time.time + inputDelay;
        ShakeText();
        OnWillContentChanged?.Invoke(value);
    }

    private void CheckRotation()
    {
        if (gameObject.transform.localEulerAngles.y % 360f > 90f && gameObject.transform.localEulerAngles.y % 360f < 270f)
        {
            textContainer.gameObject.SetActive(false);
        }
        else if (!textContainer.gameObject.activeSelf)
        {
            textContainer.gameObject.SetActive(true);
            IncrementPageNumber();
        }
    }

    private void IncrementPageNumber()
    {
        currentPageNumber++;
        willInputField.textComponent.pageToDisplay = currentPageNumber;
        willInputField.textComponent.ForceMeshUpdate();
    }

    private void ShakeText()
    {
        if (punchTween.isAlive) return;
        Vector3 punchDir = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f).normalized * punchStrength;
        punchTween = Tween.PunchLocalPosition(willInputField.transform, punchDir, punchDuration, punchFrequency);
    }

    private void SpinWill()
    {
        if (spinTween.isAlive) return;
        isInputLocked = true;
        spinTween = Sequence.Create()
        .Chain(Tween.LocalEulerAngles(transform, Vector3.zero, new Vector3(0f, 360f, 0f), 1f, Ease.Linear))
        .OnComplete(() => {
            isInputLocked = false;
        });
    }
}
