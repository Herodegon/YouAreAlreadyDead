using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [Header("Game Screens")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject gameScreen;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 45f;

    private readonly List<GameObject> screens = new();

    void Awake()
    {
        screens.Add(startScreen);
        screens.Add(gameScreen);

        startScreen.GetComponent<StartScreen>().OnPlayGame += OnGameStart;
        startScreen.GetComponent<StartScreen>().OnQuitGame += OnGameQuit;

        SelectScreen(startScreen);
    }

    private void OnGameStart()
    {
        SelectScreen(gameScreen);

        Timer.Instance.OnTimerComplete += OnGameEnd;
        Timer.Instance.OnTimerComplete += gameScreen.GetComponent<GameScreen>().StopTimer;

        Timer.Instance.SetTimer(gameDuration);
        Background.Instance.FadeIn(gameDuration);
    }

    private void OnGameEnd()
    {
        Timer.Instance.OnTimerComplete -= OnGameEnd;
        Timer.Instance.OnTimerComplete -= gameScreen.GetComponent<GameScreen>().StopTimer;

        AudioBus.Instance.PlaySFX("bell_toll");
        Background.Instance.Flash(0.5f);
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
