using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTester : MonoBehaviour
{

    public string vehicleName;
    public bool loadTrackInTrackManager;
    public string trackName;
    public string layoutName;

    void Awake()
    {
#if !UNITY_EDITOR
		Destroy(this);
#endif
    }

    // Just to make it enablable in editor:
    void Start() { }

    public void SetNamesToTrackManager()
    {
        if (!loadTrackInTrackManager)
            TrackManager.e.DeserializeTrack(trackName, layoutName);
    }
}
