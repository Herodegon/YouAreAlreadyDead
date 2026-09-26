using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameScreen : MonoBehaviour
{
    [Header("Game Components")]
    [SerializeField] private GameObject willInterface;
    [SerializeField] private GameObject objectivesInterface;
    [SerializeField] private GameObject parserObject;
    [SerializeField] private TextAsset namesFile;
    [SerializeField] private TextAsset belongingsFile;

    [Header("Game Settings")]
    [SerializeField] private int minRelationships = 1;
    [SerializeField] private int maxRelationships = 10;
    [SerializeField] private float minTimeToNextRelationship = 3f;
    [SerializeField] private float maxTimeToNextRelationship = 8f;
    [SerializeField] private float percentageNameReuse = 0.1f;
    [SerializeField] private float percentageBelongingReuse = 0.35f;

    private InputParser inputParser;
    private WillUI willUI;
    private ObjectivesUI objectivesUI;

    private readonly List<string> names = new();
    private readonly List<string> belongings = new();
    private string previousName = "";
    private string previousBelonging = "";

    private readonly ScoreCalculator scoreCalculator = new();

    private float timeToNextRelationship = 0f;
    private bool isTimerRunning = false;

    void Awake()
    {
        inputParser = parserObject.GetComponent<InputParser>();
        willUI = willInterface.GetComponent<WillUI>();
        objectivesUI = objectivesInterface.GetComponent<ObjectivesUI>();

        names.AddRange(namesFile.text.Trim().Split('\n').ToList());
        belongings.AddRange(belongingsFile.text.Trim().Split('\n').ToList());
    }

    void OnEnable()
    {
        objectivesUI.ClearObjectives();
        willUI.isInputLocked = false;

        int numberOfRelationships = Random.Range(minRelationships, maxRelationships + 1);
        GenerateRelationships(numberOfRelationships);

        timeToNextRelationship = Random.Range(minTimeToNextRelationship, maxTimeToNextRelationship);
        isTimerRunning = true;

        scoreCalculator.ClearClauses();
    }

    void Update()
    {
        if (!isTimerRunning) return;
        timeToNextRelationship -= Time.deltaTime;
        if (timeToNextRelationship <= 0f)
        {
            GenerateRelationships(1);
            timeToNextRelationship = Random.Range(minTimeToNextRelationship, maxTimeToNextRelationship);
        }
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        willUI.isInputLocked = true;
        objectivesUI.HideObjectives();
        // Prevent sfx from getting stuck after game is over
        AudioBus.Instance.StopSFX("pencil_on_paper");
    }

    private void GenerateRelationships(int numberOfRelationships)
    {
        List<RelationshipData> relationships = new();
        for (int i = 0; i < numberOfRelationships; i++)
        {
            string name = previousName;
            if (Random.value > percentageNameReuse || previousName == "")
            {
                name = names[Random.Range(0, names.Count)];
            }

            string belonging = previousBelonging;
            if (name == previousName || Random.value > percentageBelongingReuse || previousBelonging == "")
            {
                belonging = belongings[Random.Range(0, belongings.Count)];
            }

            relationships.Add(new RelationshipData(name, belonging));
            previousName = name;
            previousBelonging = belonging;
        }
        objectivesUI.PopulateObjectives(relationships);
    }
}
