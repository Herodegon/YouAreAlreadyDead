using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [Header("Game Screens")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject inputParser;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 45f;

    private readonly List<GameObject> screens = new();

    void Awake()
    {
        screens.Add(startScreen);
        screens.Add(gameScreen);
        screens.Add(endScreen);

        startScreen.GetComponent<StartScreen>().OnPlayGame += OnGameStart;
        startScreen.GetComponent<StartScreen>().OnQuitGame += OnGameQuit;

        inputParser.SetActive(false);
        SelectScreen(startScreen);
    }

    private void OnGameStart()
    {
        inputParser.SetActive(true);
        inputParser.GetComponent<WillUI>().isInputLocked = false;
        SelectScreen(gameScreen);

        Timer.Instance.OnTimerComplete += OnGameEnd;
        Timer.Instance.OnTimerComplete += inputParser.GetComponent<InputParser>().StopTimer;

        Timer.Instance.SetTimer(gameDuration);
        Background.Instance.FadeIn(gameDuration);
    }

    private void OnGameEnd()
    {
        Timer.Instance.OnTimerComplete -= OnGameEnd;
        Timer.Instance.OnTimerComplete -= inputParser.GetComponent<InputParser>().StopTimer;
        inputParser.GetComponent<WillUI>().isInputLocked = true;

        AudioBus.Instance.PlaySFX("bell_toll");
        Background.Instance.Flash(0.5f);

        SelectScreen(endScreen);
    }

    private void OnGameQuit()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private void SelectScreen(GameObject screen)
    {
        foreach (var s in screens)
        {
            s.SetActive(s == screen);
        }
    }
}
