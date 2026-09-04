using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections;

public enum TrafficLightStates
{
    OFF,
    RED,
    GREEN
}

[System.Serializable]
public class TrafficLightData
{
    public TrafficLightStates state;
    public Sprite sprite;
}

public class StartScreen : MonoBehaviour
{
    [SerializeField] private GameObject titleContainer;
    [SerializeField] private GameObject willContainer;
    [SerializeField] private GameObject buttonsContainer;
    [SerializeField] private Image trafficLightImage; 
    [SerializeField] private List<TrafficLightData> trafficLightData;
    [SerializeField] private Animator willAnimator;

    private static readonly int ReadyWillState = Animator.StringToHash("ReadyWill");

    public event Action OnPlayGame;
    public event Action OnQuitGame;

    public void OnEnable()
    {
        titleContainer.SetActive(true);
        buttonsContainer.SetActive(true);
        willContainer.SetActive(false);
        ChangeTrafficLightState(TrafficLightStates.OFF);
    }

    public void Button_LightOff()
    {
        ChangeTrafficLightState(TrafficLightStates.OFF);
    }

    public void Button_LightRed()
    {
        ChangeTrafficLightState(TrafficLightStates.RED);
    }

    public void Button_LightGreen()
    {
        ChangeTrafficLightState(TrafficLightStates.GREEN);
    }

    public void Button_ClickPlay() {StartCoroutine(RunPlayIntro());}

    public void Button_ClickQuit() => OnQuitGame?.Invoke();

    public void ChangeTrafficLightState(TrafficLightStates newState)
    {
        TrafficLightData data = trafficLightData.Find(data => data.state == newState);
        if (data != null)
        {
            trafficLightImage.sprite = data.sprite;
        }
    }

    private IEnumerator RunPlayIntro()
    {
        buttonsContainer.SetActive(false);
        ChangeTrafficLightState(TrafficLightStates.GREEN);

        AudioBus.Instance.PlaySFX("car_crash_1");
        yield return null;
        yield return new WaitUntil(() => !AudioBus.Instance.IsPlaying("car_crash_1"));

        AudioBus.Instance.PlaySFX("car_crash_2");
        titleContainer.SetActive(false);
        Background.Instance.Flash(0.5f);
        yield return null;
        yield return new WaitUntil(() => !AudioBus.Instance.IsPlaying("car_crash_2"));

        willContainer.SetActive(true);
        willAnimator.Play(ReadyWillState, 0, 0f);
        yield return null;
        yield return new WaitUntil(() => willAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

        OnPlayGame?.Invoke();
    }
}
