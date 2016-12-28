using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager e;
    void Awake() { e = this; }

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
