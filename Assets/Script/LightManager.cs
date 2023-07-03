using System;
using UnityEngine;

[ExecuteAlways]
public class LightManager : MonoBehaviour
{
    //Scene References
    [SerializeField] private Light sun;
    [SerializeField] private LightingPreset preset;
    //Variables
    [SerializeField, Range(0, 24)] private float timeOfDay;
    float timeProgression = 7f;
    float targetTime;
    private void Start()
    {
        targetTime = timeOfDay;
        UpdateLighting(timeOfDay / 24f);
    }

    float lastTimeOfDay = 0;

    void Update()
    {
        if (preset != null)
        {
            if (Application.isPlaying)
            {
                if(timeProgression > 0f && Math.Abs(timeOfDay - targetTime) > 0.5f)
                {
                    timeOfDay += Time.deltaTime * timeProgression;
                    timeOfDay %= 24; //Modulus to ensure always between 0-24
                }
            }
            
            if(lastTimeOfDay != timeOfDay)
            {
                UpdateLighting(timeOfDay / 24f);
            }
        }
        lastTimeOfDay = timeOfDay;
    }


    void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = preset.FogColor.Evaluate(timePercent);
        sun.color = preset.DirectionalColor.Evaluate(timePercent);
        sun.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
    }

    public void GoToNight()
    {
        targetTime = 23;
    }

    public void GoToDay(float delay)
    {
        Invoke("ActualGoToDay", delay);
    }

    public void ActualGoToDay()
    {
        targetTime = 14;
    }
}