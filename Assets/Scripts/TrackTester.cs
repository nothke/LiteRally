using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTester : MonoBehaviour
{

    public string vehicleName;
    public bool loadTrackInTrackManager;
    public string trackName;
    public string layoutName;

    // Just to make it enablable in editor:
    void Start() { }

    public void SetNamesToTrackManager()
    {
        if (loadTrackInTrackManager)
			TrackManager.e.InitThisTrack();
        else
            TrackManager.e.DeserializeTrack(trackName, layoutName);
    }
}
