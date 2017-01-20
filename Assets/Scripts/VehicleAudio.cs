using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleAudio : MonoBehaviour
{

    VehicleController control;

    public AudioSource engineSource;
    public float engineMinPitch = 0.2f;
    public float engineMaxPitch = 1;

    public AudioSource tireSquealSource;

    void Start()
    {
        control = GetComponent<VehicleController>();
    }

    float refVelo;
    float throttle;

    void Update()
    {
        if (engineSource)
        {
            engineSource.volume = 0.1f;

            throttle = Mathf.SmoothDamp(throttle, control.accelInput, ref refVelo, 0.2f);

            float pitchT = Mathf.Lerp(throttle, control.speed * 0.02f, 0.5f);

            engineSource.pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, pitchT);
        }
    }
}
