using System.Collections.Generic;
using UnityEngine;

public class ObjectivesUI : MonoBehaviour
{
    [SerializeField] private ObjectiveObject objectivePrefab;
    [SerializeField] private Transform objectivesContainer;

    public List<ObjectiveObject> objectives = new();

    public void PopulateObjectives(List<RelationshipData> relationships)
    {
        foreach (var relationship in relationships)
        {
            ObjectiveObject objective = Instantiate(objectivePrefab, objectivesContainer);
            objective.Init(relationship);
            objective.OnObjectiveComplete += OnObjectiveComplete;
            objectives.Add(objective);
        }
    }

    private void OnObjectiveComplete(GameObject objectiveObject)
    {
        objectives.Remove(objectiveObject.GetComponent<ObjectiveObject>());
        Destroy(objectiveObject);
    }
}
