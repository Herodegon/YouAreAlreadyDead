using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    [SerializeField] private WillUI willInterface;
    [SerializeField] private ObjectivesUI objectivesInterface;
    [SerializeField] private TextAsset namesFile;
    [SerializeField] private TextAsset belongingsFile;

    [Header("Game Settings")]
    [SerializeField] private int minRelationships = 1;
    [SerializeField] private int maxRelationships = 10;

    private List<string> names = new();
    private List<string> belongings = new();
    private List<RelationshipData> relationships = new();
    private List<string> willSequence = new();

    void Awake()
    {
        names = namesFile.text.Trim().Split('\n').ToList();
        belongings = belongingsFile.text.Trim().Split('\n').ToList();

        Debug.Log(names.Count);
        Debug.Log(belongings.Count);

        willInterface.OnWhitespaceEntered += OnWillContentChanged;
    }

    void Update()
    {
        if (relationships.Count == 0)
        {
            GenerateRelationships();
        }
    }

    private void GenerateRelationships()
    {
        int numberOfRelationships = Random.Range(minRelationships, maxRelationships + 1);
        for (int i = 0; i < numberOfRelationships; i++)
        {
            string name = names[Random.Range(0, names.Count)];
            //string belonging = belongings[Random.Range(0, belongings.Count)];
            string belonging = "car";
            if (i > numberOfRelationships - 3)
            {
                belonging = belongings[Random.Range(0, belongings.Count)];
            }
            relationships.Add(new RelationshipData(name, belonging));
        }
        objectivesInterface.PopulateObjectives(relationships);
    }

    private void OnWillContentChanged(string value)
    {
        willSequence = value.Trim().ToLower().Split(' ').ToList();
        for (int i = 0; i < willSequence.Count; i++)
        {
            for (int j = 0; j < objectivesInterface.objectives.Count; j++)
            {
                if (i == 0)
                {
                    objectivesInterface.objectives[j].Reset();
                }
                objectivesInterface.objectives[j].CheckName(willSequence[i]);
                objectivesInterface.objectives[j].CheckBelonging(willSequence[i]);
            }
        }
    }
}
