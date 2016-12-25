using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public float timeSinceLastLap;
    public List<float> lapTimes;

    int currentPortal;
    int nextPortal;

    int portalNum;

    private void Start()
    {
        portalNum = RaceManager.e.GetNumberOfPortals();

        nextPortal = 1;
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
            lapTimes.Add(lapTime);

            timeSinceLastLap = Time.time;

            if (lapTimes.Count > RaceManager.e.lapsToRace)
            {
                RaceManager.e.EndRace();
            }

            Debug.Log("LAPPED! Time: " + lapTime);
        }
        else
            Debug.Log("Passed tru " + nextPortal);

        nextPortal++;

        if (nextPortal >= portalNum)
            nextPortal = 0;
    }
}
