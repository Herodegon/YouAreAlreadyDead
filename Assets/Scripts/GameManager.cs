using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private GameObject endScreen;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 45f;

    private readonly List<GameObject> screens = new();

    void Awake()
    {
        screens.Add(startScreen);
        screens.Add(gameScreen);
        screens.Add(endScreen);

        startScreen.GetComponent<StartScreen>().OnPlaySelect += OnGameStart;
        startScreen.GetComponent<StartScreen>().OnQuitSelect += OnGameQuit;
    }

    private void OnGameStart()
    {
        SelectScreen(gameScreen);
        Timer.Instance.OnTimerComplete += OnGameEnd;
        Timer.Instance.SetTimer(gameDuration);
    }

    private void OnGameEnd()
    {
        SelectScreen(endScreen);
        Timer.Instance.OnTimerComplete -= OnGameEnd;
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
