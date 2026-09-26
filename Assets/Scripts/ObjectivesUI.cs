using System.Collections.Generic;
using UnityEngine;

public class ObjectivesUI : MonoBehaviour
{
    [SerializeField] private ObjectiveObject objectivePrefab;
    [SerializeField] private Transform objectivesContainer;
    [SerializeField] private Transform completedObjectivesContainer;

    [Header("Game Settings")]
    [SerializeField] [Range(1, 20)] private int maxObjectiveDisplay = 6;
    private readonly List<RelationshipData> queuedObjectives = new();
    
    public List<ObjectiveObject> objectives = new();

    public int Version { get; private set; }

    void LateUpdate()
    {
        if (queuedObjectives.Count > 0 && objectivesContainer.childCount < maxObjectiveDisplay)
        {
            int numberToPopulate = Mathf.Min(queuedObjectives.Count, maxObjectiveDisplay - objectivesContainer.childCount);
            PopulateObjectives(queuedObjectives.GetRange(0, numberToPopulate));
            queuedObjectives.RemoveRange(0, numberToPopulate);
            Debug.Log($"Number of Queued Objectives Remaining: {queuedObjectives.Count}");
        }
    }

    public void PopulateObjectives(List<RelationshipData> relationships)
    {
        int objectivesToDisplay = Mathf.Min(maxObjectiveDisplay, relationships.Count);
        for (int i = 0; i < objectivesToDisplay; i++)
        {
            ObjectiveObject objective = Instantiate(objectivePrefab, objectivesContainer);
            objective.Init(relationships[i]);
            objective.OnObjectiveComplete += OnObjectiveComplete;
            objective.OnObjectiveUncomplete += OnObjectiveUncomplete;
            objectives.Add(objective);
        }
        relationships.RemoveRange(0, objectivesToDisplay);
        queuedObjectives.AddRange(relationships);
        Version++;
        Debug.Log($"Queued {queuedObjectives.Count} objectives");
    }

    public void HideObjectives()
    {
        objectivesContainer.gameObject.SetActive(false);
    }

    public void ShowObjectives()
    {
        objectivesContainer.gameObject.SetActive(true);
    }

    public void ClearObjectives()
    {
        foreach (var objective in objectives)
        {
            Destroy(objective.gameObject);
        }
        objectives.Clear();
        Version++;
    }

    private void OnObjectiveComplete(GameObject objectiveObject)
    {
        objectiveObject.transform.SetParent(completedObjectivesContainer);
    }

    private void OnObjectiveUncomplete(GameObject objectiveObject)
    {
        objectiveObject.transform.SetParent(objectivesContainer);
    }
}
