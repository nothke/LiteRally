using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public float timeSinceLastLap;
    public List<float> lapTimes;

    int currentPortal;
    int nextPortal = 1;

    int portalNum;

    public Renderer vehicleBody;

    public VehicleController control
    {
        get
        {
            if (!_control)
                _control = GetComponent<VehicleController>();

            return _control;
        }
    }


    private VehicleController _control;


    private void Start()
    {
        StartCoroutine(FrameWait());

        nextPortal = 1;
    }

    IEnumerator FrameWait()
    {
        yield return null;

        portalNum = RaceManager.e.GetNumberOfPortals();
    }

    public void EnableInput(bool enabled, bool freezeRigidbody = true)
    {
        control.activeInput = enabled;

        if (freezeRigidbody)
            GetComponent<Rigidbody>().isKinematic = !enabled;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Portal")
        {
            if (other.name == nextPortal.ToString())
                ValidatePortal();
        }
    }

    void ValidatePortal()
    {
        if (nextPortal == 0)
        {
            // LAP
            float lapTime = Time.time - timeSinceLastLap;

            if (lapTime == 0)
                Debug.LogError("Lap time is 0 ??? WUT!?");
            else
                lapTimes.Add(lapTime);

            timeSinceLastLap = Time.time;

            if (lapTimes.Count >= RaceManager.e.lapsToRace)
            {
                RaceManager.e.EndRace(this);
            }

            Debug.Log("LAPPED! Time: " + lapTime);
        }
        else
            Debug.Log(name + " passed tru " + nextPortal);

        nextPortal++;

        if (nextPortal >= portalNum)
            nextPortal = 0;
    }

    public float GetFastestTime()
    {
        return Mathf.Min(lapTimes.ToArray());
    }
}
