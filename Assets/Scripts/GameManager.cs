using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        InitSession();
    }

    public void InitSession()
    {
        TrackManager.e.InitThisTrack();
        RaceManager.e.InitRace();
    }
}
