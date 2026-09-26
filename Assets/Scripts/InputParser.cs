using UnityEngine;
using System.Collections.Generic;
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
    [SerializeField] private float parseInterval = 0.05f;

    public List<(List<string> heirTokens, List<string> belongingTokens)> Clauses => clauses;

    private static readonly char[] WhitespaceSeparators = { ' ', '\t', '\n', '\r' };

    private readonly List<string> willSequence = new();
    private readonly List<string> conjunctionTokens = new() { "and", "&" };
    private readonly List<string> punctuationTokens = new() { ".", "!", "?", "," };

    // Maps a heir token to every objective that accepts it, so parsing costs one
    // lookup per token instead of a scan across the whole objective list.
    private readonly Dictionary<string, List<ObjectiveData>> heirIndex = new();
    private int indexedObjectivesVersion = -1;

    private string pendingValue;
    private bool isParsePending;
    private float nextParseTime;

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
        // Don't let a debounced edit get dropped on the way out.
        if (isParsePending) ParsePendingValue();
    }

    void LateUpdate()
    {
        if (!isParsePending || Time.unscaledTime < nextParseTime) return;
        ParsePendingValue();
    }

    // Typing faster than parseInterval coalesces into a single parse. An edit after
    // an idle gap still parses on the same frame, since nextParseTime is already past.
    private void WillContentChanged(string value)
    {
        pendingValue = value;
        isParsePending = true;
    }

    private void ParsePendingValue()
    {
        isParsePending = false;
        nextParseTime = Time.unscaledTime + parseInterval;

        RebuildHeirIndexIfNeeded();
        ParseValue(pendingValue);
    }

    private void RebuildHeirIndexIfNeeded()
    {
        if (indexedObjectivesVersion == objectivesUI.Version) return;
        indexedObjectivesVersion = objectivesUI.Version;

        heirIndex.Clear();
        foreach (var objective in objectivesUI.objectives)
        {
            foreach (string heirToken in objective.objectiveData.HeirTokens)
            {
                if (!heirIndex.TryGetValue(heirToken, out var matches))
                {
                    matches = new List<ObjectiveData>();
                    heirIndex[heirToken] = matches;
                }
                matches.Add(objective.objectiveData);
            }
        }
    }

    private void ParseValue(string value)
    {
        pendingObjectiveTokens.Clear();
        clauses.Clear();
        clauseBuffer.heirTokens.Clear();
        clauseBuffer.belongingTokens.Clear();
        ResetObjectives();
        willSequence.Clear();
        foreach (string word in value.ToLowerInvariant()
            .Split(WhitespaceSeparators, StringSplitOptions.RemoveEmptyEntries))
        {
            AddWordTokens(word);
        }
        ParseWillContents();
    }

    // Trailing punctuation becomes its own token so the parser can treat it as a
    // clause terminator: "sword!?" -> "sword", "!", "?"
    private void AddWordTokens(string word)
    {
        int end = word.Length;
        while (end > 0 && punctuationTokens.Contains(word[end - 1].ToString())) end--;

        if (end > 0) willSequence.Add(word[..end]);
        for (int i = end; i < word.Length; i++) willSequence.Add(word[i].ToString());
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
            // Order of Operations: Heir -> (Conjunction -> Heir -> ...) -> Belonging -> Punctuation
            // Any rule token (conjunction, punctuation, etc.) processed after heir but before belonging will be added
            // to the heir token buffer. Any rule token processed after belonging will be added to the belonging token buffer.
            if (heirIndex.TryGetValue(willSequence[i], out var heirMatches))
            {
                foreach (var objectiveData in heirMatches)
                {
                    CheckForHeirToken(willSequence[i], objectiveData);
                }
            }

            if (CheckForConjunctionToken(willSequence[i])) continue;
            if (clauseBuffer.belongingTokens.Count > 0)
            {
                if (CheckForPunctuationToken(willSequence[i])) continue;
                AddClause(clauseBuffer.heirTokens, clauseBuffer.belongingTokens);
            }
            if (pendingObjectiveTokens.Count > 0) CheckForBelongingToken(i);
        }
        // If no tokens remain after parsing a belonging, add the clause to the list
        if (clauseBuffer.belongingTokens.Count > 0)
        {
            AddClause(clauseBuffer.heirTokens, clauseBuffer.belongingTokens);
        }
        OnClausesChanged?.Invoke(clauses);
    }

    private void AddClause(List<string> heirTokens, List<string> belongingTokens)
    {
        clauses.Add((new List<string>(heirTokens), new List<string>(belongingTokens)));
        clauseBuffer.heirTokens.Clear();
        clauseBuffer.belongingTokens.Clear();
    }

    // Callers resolve the objective through heirIndex, so reaching here already
    // proves the token is one of this objective's heir tokens.
    private void CheckForHeirToken(string token, ObjectiveData objectiveData)
    {
        if (objectiveData.State == ObjectiveState.COMPLETE) return;
        if (!pendingObjectiveTokens.Contains(objectiveData)) pendingObjectiveTokens.Add(objectiveData);
        if (!clauseBuffer.heirTokens.Contains(token)) clauseBuffer.heirTokens.Add(token);
        objectiveData.SetState(ObjectiveState.PENDING);
    }

    private bool CheckForConjunctionToken(string token)
    {
        if (conjunctionTokens.Contains(token) && pendingObjectiveTokens.Count > 0)
        {
            if (clauseBuffer.belongingTokens.Count > 0)
            {
                clauseBuffer.belongingTokens.Add(token);
            }
            else
            {
                clauseBuffer.heirTokens.Add(token);
            }
            return true;
        }
        return false;
    }

    private bool CheckForPunctuationToken(string token)
    {
        if (punctuationTokens.Contains(token) && pendingObjectiveTokens.Count > 0)
        {
            clauseBuffer.belongingTokens.Add(token);
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
            pendingObjectiveTokens.Clear();

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
