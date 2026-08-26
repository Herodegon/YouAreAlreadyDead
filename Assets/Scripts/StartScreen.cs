using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

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
    [SerializeField] private Image trafficLightImage; 
    [SerializeField] private List<TrafficLightData> trafficLightData;
    [SerializeField] private Animator willAnimator;

    private static readonly int ReadyWillState = Animator.StringToHash("ReadyWill");

    public event Action OnPlaySelect;
    public event Action OnQuitSelect;

    public void OnEnable()
    {
        titleContainer.SetActive(true);
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

    public void Button_ClickPlay()
    {
        titleContainer.SetActive(false);
        willContainer.SetActive(true);

        AudioBus.Instance.PlaySFX("car_crash");

        willAnimator.Play(ReadyWillState, 0, 0f);
        willAnimator.Update(0f);
    }

    public void Button_ClickQuit()
    {
        OnQuitSelect?.Invoke();
    }

    public void ChangeTrafficLightState(TrafficLightStates newState)
    {
        TrafficLightData data = trafficLightData.Find(data => data.state == newState);
        if (data != null)
        {
            trafficLightImage.sprite = data.sprite;
        }
    }
}
