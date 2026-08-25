using System.Collections.Generic;
using UnityEngine;

public class ObjectivesUI : MonoBehaviour
{
    [SerializeField] private ObjectiveObject objectivePrefab;
    [SerializeField] private Transform objectivesContainer;
    [SerializeField] private Transform completedObjectivesContainer;

    public List<ObjectiveObject> objectives = new();

    public void PopulateObjectives(List<RelationshipData> relationships)
    {
        foreach (var relationship in relationships)
        {
            ObjectiveObject objective = Instantiate(objectivePrefab, objectivesContainer);
            objective.Init(relationship);
            objective.OnObjectiveComplete += OnObjectiveComplete;
            objective.OnObjectiveUncomplete += OnObjectiveUncomplete;
            objectives.Add(objective);
        }
    }

    private void OnObjectiveComplete(GameObject objectiveObject)
    {
        Debug.Log("Objective completed: " + objectiveObject.name);
        objectiveObject.transform.SetParent(completedObjectivesContainer);
    }

    private void OnObjectiveUncomplete(GameObject objectiveObject)
    {
        Debug.Log("Objective uncompleted: " + objectiveObject.name);
        objectiveObject.transform.SetParent(objectivesContainer);
    }
}
