using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

public class ObjectiveObject : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI belongingText;
    [SerializeField] private Color incompleteColor;
    [SerializeField] private Color completeColor;

    public RelationshipData relationship;
    public bool isNameWritten = false;
    public bool isBelongingWritten = false;

    private string nameTarget = "";
    private List<string> belongingTargetSequence = new();
    private int currentTargetIndex = 0;

    public event Action<GameObject> OnObjectiveComplete;

    public void Init(RelationshipData relationship)
    {
        this.relationship = relationship;
        nameText.text = relationship.name;
        belongingText.text = relationship.belonging;
        nameTarget = relationship.name.Trim().ToLower();
        belongingTargetSequence = relationship.belonging.Trim().ToLower().Split(' ').ToList();
    }

    void LateUpdate()
    {
        if (isNameWritten && isBelongingWritten)
        {
            OnObjectiveComplete?.Invoke(gameObject);
        }
        
        if (isNameWritten && nameText.color != completeColor)
        {
            nameText.color = completeColor;
        }

        if (isBelongingWritten && belongingText.color != completeColor)
        {
            belongingText.color = completeColor;
        }
    }

    public void Reset()
    {
        isNameWritten = false;
        isBelongingWritten = false;
        currentTargetIndex = 0;
        nameText.color = incompleteColor;
        belongingText.color = incompleteColor;
    }

    public void CheckName(string name)
    {
        if (name == nameTarget)
        {
            isNameWritten = true;
        }
    }

    public void CheckBelonging(string nextBelongingWord)
    {
        if (nextBelongingWord == belongingTargetSequence[currentTargetIndex] && isNameWritten)
        {
            currentTargetIndex++;
        }
        else
        {
            currentTargetIndex = 0;
        }

        if (currentTargetIndex == belongingTargetSequence.Count)
        {
            isBelongingWritten = true;
        }
    }
}
