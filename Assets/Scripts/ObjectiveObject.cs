using UnityEngine;
using TMPro;
using System;
using PrimeTween;

public enum ObjectiveState
{
    INCOMPLETE,
    PENDING,
    BROKEN,
    COMPLETE
}

public sealed class ObjectiveData
{
    public ObjectiveData(RelationshipData relationship)
    {
        HeirTokens = Tokenize(relationship.name);
        BelongingTokens = Tokenize(relationship.belonging);
    }
    public string[] HeirTokens { get; }
    public string[] BelongingTokens { get; }
    public ObjectiveState State { get; private set; }
    public void SetState(ObjectiveState state) => State = state;
    public static string[] Tokenize(string text) => text
        .ToLowerInvariant()
        .Split(new[] { ' ', '\t', '\n', '\r', ',', '.', ';', ':', '"', '\'' },
               StringSplitOptions.RemoveEmptyEntries);
}

public class ObjectiveObject : MonoBehaviour
{
    public Transform textParent;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI belongingText;
    [SerializeField] private Color incompleteColor;
    [SerializeField] private Color completeColor;
    [SerializeField] private Color brokenColor;

    [Header("Float")]
    [SerializeField] private Vector2 majorRadiusRange = new(6f, 12f);
    [SerializeField] private Vector2 minorRadiusRange = new(3f, 7f);
    [SerializeField] private Vector2 periodRange = new(4f, 8f);

    public ObjectiveData objectiveData;

    public event Action<GameObject> OnObjectiveComplete;
    public event Action<GameObject> OnObjectiveUncomplete;
    private bool hasReportedCompletion = false;

    private Tween floatTween;
    private Vector3 floatRestPosition;
    private Vector2 floatRadii;
    private Vector2 floatAnchor;
    private float floatStartAngle;
    private float floatDirection;
    private float floatPeriod;
    private float floatCosTilt;
    private float floatSinTilt;

    public void Init(RelationshipData relationship)
    {
        objectiveData = new ObjectiveData(relationship);
        nameText.text = relationship.name;
        belongingText.text = relationship.belonging;
        RandomizeFloatPath();
        StartFloating();
    }

    private void RandomizeFloatPath()
    {
        floatRestPosition = textParent.localPosition;
        floatRadii = new Vector2(UnityEngine.Random.Range(majorRadiusRange.x, majorRadiusRange.y),
                                 UnityEngine.Random.Range(minorRadiusRange.x, minorRadiusRange.y));
        floatPeriod = UnityEngine.Random.Range(periodRange.x, periodRange.y);
        floatStartAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        floatDirection = UnityEngine.Random.value < 0.5f ? -1f : 1f;

        float tilt = UnityEngine.Random.Range(0f, Mathf.PI);
        floatCosTilt = Mathf.Cos(tilt);
        floatSinTilt = Mathf.Sin(tilt);

        floatAnchor = PointOnEllipse(floatStartAngle);
    }

    private Vector2 PointOnEllipse(float angle)
    {
        float x = floatRadii.x * Mathf.Cos(angle);
        float y = floatRadii.y * Mathf.Sin(angle);
        return new Vector2(x * floatCosTilt - y * floatSinTilt,
                           x * floatSinTilt + y * floatCosTilt);
    }

    private void StartFloating()
    {
        if (floatTween.isAlive) return;
        floatTween = Tween.Custom(this, 0f, Mathf.PI * 2f, floatPeriod, static (self, angle) =>
        {
            Vector2 offset = self.PointOnEllipse(self.floatStartAngle + self.floatDirection * angle) - self.floatAnchor;
            self.textParent.localPosition = self.floatRestPosition + (Vector3)offset;
        }, Ease.Linear, cycles: -1);
    }

    private void StopFloating()
    {
        if (floatTween.isAlive) floatTween.Stop();
        textParent.localPosition = floatRestPosition;
    }

    void LateUpdate()
    {
        switch (objectiveData.State)
        {
            case ObjectiveState.INCOMPLETE:
                if (nameText.color != incompleteColor || belongingText.color != incompleteColor)
                {
                    nameText.color = incompleteColor;
                    belongingText.color = incompleteColor;
                }
                break;
            case ObjectiveState.PENDING:
                if (nameText.color != completeColor || belongingText.color != incompleteColor)
                {
                    nameText.color = completeColor;
                    belongingText.color = incompleteColor;
                }
                break;
            case ObjectiveState.BROKEN:
                if (nameText.color != brokenColor || belongingText.color != incompleteColor)
                {
                    nameText.color = brokenColor;
                    belongingText.color = incompleteColor;
                }
                break;
            case ObjectiveState.COMPLETE:
                if (nameText.color != completeColor || belongingText.color != completeColor)
                {
                    nameText.color = completeColor;
                    belongingText.color = completeColor;
                }
                break;
        }
        CheckObjectiveCompletion();
    }

    private void CheckObjectiveCompletion()
    {
        bool isComplete = objectiveData.State == ObjectiveState.COMPLETE;
        if (isComplete && !hasReportedCompletion)
        {
            hasReportedCompletion = true;
            StopFloating();
            OnObjectiveComplete?.Invoke(gameObject);
        }
        else if (!isComplete && hasReportedCompletion)
        {
            hasReportedCompletion = false;
            StartFloating();
            OnObjectiveUncomplete?.Invoke(gameObject);
        }
    }
}
