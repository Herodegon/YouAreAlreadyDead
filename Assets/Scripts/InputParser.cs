using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

using Random = UnityEngine.Random;

public struct RelationshipData
{
    public string name;
    public string belonging;

    public RelationshipData(string name, string belonging)
    {
        this.name = name;
        this.belonging = belonging;
    }
}

public class InputParser : MonoBehaviour
{
    [Header("Game Data")]
    public WillUI willInterface;
    [SerializeField] private ObjectivesUI objectivesInterface;
    [SerializeField] private TextAsset namesFile;
    [SerializeField] private TextAsset belongingsFile;

    [Header("Game Settings")]
    [SerializeField] private int minRelationships = 1;
    [SerializeField] private int maxRelationships = 10;
    [SerializeField] private float minTimeToNextRelationship = 3f;
    [SerializeField] private float maxTimeToNextRelationship = 8f;

    private readonly List<string> names = new();
    private readonly List<string> belongings = new();
    private readonly List<string> willSequence = new();

    private readonly List<ObjectiveData> pendingObjectiveTokens = new();

    private float timeToNextRelationship = 0f;
    private bool isTimerRunning = false;

    void OnEnable()
    {
        names.AddRange(namesFile.text.Trim().Split('\n').ToList());
        belongings.AddRange(belongingsFile.text.Trim().Split('\n').ToList());

        willInterface.OnWillContentChanged += WillContentChanged;
        objectivesInterface.ClearObjectives();

        int numberOfRelationships = Random.Range(minRelationships, maxRelationships + 1);
        GenerateRelationship(numberOfRelationships);

        timeToNextRelationship = Random.Range(minTimeToNextRelationship, maxTimeToNextRelationship);
        isTimerRunning = true;
    }

    void Update()
    {
        if (!isTimerRunning) return;
        timeToNextRelationship -= Time.deltaTime;
        if (timeToNextRelationship <= 0f)
        {
            int numberOfRelationships = Random.Range(minRelationships, maxRelationships + 1);
            GenerateRelationship(numberOfRelationships);
            timeToNextRelationship = Random.Range(minTimeToNextRelationship, maxTimeToNextRelationship);
        }
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    private void GenerateRelationship(int numberOfRelationships)
    {
        List<RelationshipData> relationships = new();
        for (int i = 0; i < numberOfRelationships; i++)
        {
            string name = names[Random.Range(0, names.Count)];
            string belonging = belongings[Random.Range(0, belongings.Count)];
            relationships.Add(new RelationshipData(name, belonging));
        }
        objectivesInterface.PopulateObjectives(relationships);
    }

    private void WillContentChanged(string value)
    {
        pendingObjectiveTokens.Clear();
        ResetObjectives();
        willSequence.Clear();
        willSequence.AddRange(value.ToLowerInvariant().Split(new[] { ' ', '\t', '\n', '\r', ',', '.', ';', ':', '"', '\'' },
               StringSplitOptions.RemoveEmptyEntries));
        ParseWillContents();
    }

    private void ResetObjectives()
    {
        foreach (var objective in objectivesInterface.objectives)
        {
            objective.objectiveData.SetState(ObjectiveState.INCOMPLETE);
        }
    }

    private void ParseWillContents()
    {
        for (int i = 0; i < willSequence.Count; i++)
        {
            foreach (var objective in objectivesInterface.objectives)
            {
                if (CheckForHeirToken(willSequence[i], objective.objectiveData)) break;
            }
            if (pendingObjectiveTokens.Count > 0) CheckForBelongingToken(i);
        }
    }

    private bool CheckForHeirToken(string token, ObjectiveData objectiveData)
    {
        if (objectiveData.HeirTokens.Contains(token))
        {
            if (!pendingObjectiveTokens.Contains(objectiveData)) pendingObjectiveTokens.Add(objectiveData);
            objectiveData.SetState(ObjectiveState.PENDING);
            return true;
        }
        return false;
    }

    private void CheckForBelongingToken(int fromIndex)
    {
        bool objectiveCompleted = false;
        for (int i = pendingObjectiveTokens.Count - 1; i >= 0; i--)
        {
            ObjectiveData objectiveData = pendingObjectiveTokens[i];
            if (objectiveData.State == ObjectiveState.BROKEN) continue;
            if (!MatchesAt(fromIndex, objectiveData.BelongingTokens))
            {
                continue;
            }
            objectiveData.SetState(ObjectiveState.COMPLETE);
            pendingObjectiveTokens.RemoveAt(i);
            objectiveCompleted = true;
        }
        if (objectiveCompleted)
        {
            pendingObjectiveTokens.ForEach(objectiveData => objectiveData.SetState(ObjectiveState.BROKEN));
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
