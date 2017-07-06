using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTester : MonoBehaviour
{

    public string vehicleName;
    public string trackName;
    public string layoutName;

	// Just to make it enablable in editor:
    void Start() {}

    public void SetNamesToTrackManager()
    {
        TrackManager.e.DeserializeTrack(trackName, layoutName);
    }
}
