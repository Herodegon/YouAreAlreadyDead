using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public struct RelationshipData
{
    public RelationshipData(string name, string belonging)
    {
        this.name = name;
        this.belonging = belonging;
    }
    
    public string name;
    public string belonging;
}

public class InputParser : MonoBehaviour
{
    [SerializeField] private WillUI willUI;
    [SerializeField] private ObjectivesUI objectivesUI;

    public List<(List<string> heirTokens, List<string> belongingTokens)> Clauses => clauses;

    private readonly List<string> willSequence = new();
    private readonly List<string> conjunctionTokens = new() { "and", "&" };

    private readonly List<ObjectiveData> pendingObjectiveTokens = new();
    private readonly (List<string> heirTokens, List<string> belongingTokens) clauseBuffer = (new(), new());
    private readonly List<(List<string> heirTokens, List<string> belongingTokens)> clauses = new();

    public event Action<List<(List<string> heirTokens, List<string> belongingTokens)>> OnClausesChanged;

    void OnEnable()
    {
        willUI.OnWillContentChanged += WillContentChanged;
    }

    void OnDisable()
    {
        willUI.OnWillContentChanged -= WillContentChanged;
    }

    private void WillContentChanged(string value)
    {
        pendingObjectiveTokens.Clear();
        clauses.Clear();
        clauseBuffer.heirTokens.Clear();
        clauseBuffer.belongingTokens.Clear();
        ResetObjectives();
        willSequence.Clear();
        willSequence.AddRange(value.ToLowerInvariant().Split(new[] { ' ', '\t', '\n', '\r', ',', '.', ';', ':', '"', '\'' },
               StringSplitOptions.RemoveEmptyEntries));
        ParseWillContents();
    }

    private void ResetObjectives()
    {
        foreach (var objective in objectivesUI.objectives)
        {
            objective.objectiveData.SetState(ObjectiveState.INCOMPLETE);
        }
    }

    private void ParseWillContents()
    {
        for (int i = 0; i < willSequence.Count; i++)
        {
            foreach (var objective in objectivesUI.objectives)
            {
                CheckForHeirToken(willSequence[i], objective.objectiveData);
            }
            if (CheckForConjunctionToken(willSequence[i])) continue;
            if (pendingObjectiveTokens.Count > 0) CheckForBelongingToken(i);
        }
    }

    private void CheckForHeirToken(string token, ObjectiveData objectiveData)
    {
        if (objectiveData.State == ObjectiveState.COMPLETE) return;
        if (objectiveData.HeirTokens.Contains(token))
        {
            if (!pendingObjectiveTokens.Contains(objectiveData)) pendingObjectiveTokens.Add(objectiveData);
            if (!clauseBuffer.heirTokens.Contains(token)) clauseBuffer.heirTokens.Add(token);
            objectiveData.SetState(ObjectiveState.PENDING);
            Debug.Log($"Added heir token: {token} to objective: {objectiveData.HeirTokens}");
        }
    }

    private bool CheckForConjunctionToken(string token)
    {
        if (conjunctionTokens.Contains(token) && pendingObjectiveTokens.Count > 0)
        {
            clauseBuffer.heirTokens.Add(token);
            return true;
        }
        return false;
    }

    private void CheckForBelongingToken(int fromIndex)
    {
        bool objectiveCompleted = false;
        string recentlyCompletedHeirToken = string.Empty;
        List<ObjectiveData> restoredObjectives = new();
        for (int i = pendingObjectiveTokens.Count - 1; i >= 0; i--)
        {
            ObjectiveData objectiveData = pendingObjectiveTokens[i];
            if (objectiveData.State == ObjectiveState.BROKEN) continue;
            if (!MatchesAt(fromIndex, objectiveData.BelongingTokens)) continue;
            if (!clauseBuffer.belongingTokens.Contains(objectiveData.BelongingTokens[0]))
                clauseBuffer.belongingTokens.AddRange(objectiveData.BelongingTokens);
            objectiveData.SetState(ObjectiveState.COMPLETE);
            recentlyCompletedHeirToken = objectiveData.HeirTokens[0];
            pendingObjectiveTokens.RemoveAt(i);
            objectiveCompleted = true;
        }

        // When at least one heir's condition is met, all other invalid pending heirs
        // are marked as broken, and their clause must be restarted or rewritten.
        if (objectiveCompleted)
        {
            pendingObjectiveTokens.ForEach(objectiveData => {
                // If objective has same heir but a different belonging, it will be restored to
                // pending list so the objective can be completed without having to rewrite heir name
                if (objectiveData.HeirTokens[0] == recentlyCompletedHeirToken) 
                {
                    restoredObjectives.Add(objectiveData);
                }
                else
                {
                    objectiveData.SetState(ObjectiveState.BROKEN);
                }
            });
            clauses.Add((new List<string>(clauseBuffer.heirTokens), new List<string>(clauseBuffer.belongingTokens)));
            OnClausesChanged?.Invoke(clauses);
            pendingObjectiveTokens.Clear();
            clauseBuffer.heirTokens.Clear();
            clauseBuffer.belongingTokens.Clear();

            // Restore objectives with same heir token but with different belonging tokens
            pendingObjectiveTokens.AddRange(restoredObjectives);
        }
    }

    private bool MatchesAt(int fromIndex, string[] phrase)
    {
        if (fromIndex + phrase.Length > willSequence.Count) return false;
        for (int i = 0; i < phrase.Length; i++)
        {
            if (willSequence[fromIndex + i] != phrase[i]) return false;
        }
        return true;
    }
}
