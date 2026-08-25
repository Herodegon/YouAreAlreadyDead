using UnityEngine;
using TMPro;
using System;

public enum ObjectiveState
{
    INCOMPLETE,
    PENDING,
    BROKEN,
    COMPLETE
}

public sealed class ObjectiveData
{
    public string[] HeirTokens { get; }
    public string[] BelongingTokens { get; }
    public ObjectiveState State { get; private set; }
    public ObjectiveData(RelationshipData relationship)
    {
        HeirTokens = Tokenize(relationship.name);
        BelongingTokens = Tokenize(relationship.belonging);
    }
    public void SetState(ObjectiveState state) => State = state;
    public static string[] Tokenize(string text) => text
        .ToLowerInvariant()
        .Split(new[] { ' ', '\t', '\n', '\r', ',', '.', ';', ':', '"', '\'' },
               StringSplitOptions.RemoveEmptyEntries);
}

public class ObjectiveObject : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI belongingText;
    [SerializeField] private Color incompleteColor;
    [SerializeField] private Color completeColor;
    [SerializeField] private Color brokenColor;
    public ObjectiveData objectiveData;

    public event Action<GameObject> OnObjectiveComplete;
    public event Action<GameObject> OnObjectiveUncomplete;
    private bool hasReportedCompletion = false;

    public void Init(RelationshipData relationship)
    {
        objectiveData = new ObjectiveData(relationship);
        nameText.text = relationship.name;
        belongingText.text = relationship.belonging;
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
            OnObjectiveComplete?.Invoke(gameObject);
        }
        else if (!isComplete && hasReportedCompletion)
        {
            hasReportedCompletion = false;
            OnObjectiveUncomplete?.Invoke(gameObject);
        }
    }
}
