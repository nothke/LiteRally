using UnityEngine;

public class TrackObjectEntity : MonoBehaviour
{
    public string trackObjectName;

    public TrackObject.CollisionType collision;

    private void Start()
    {
        TrackObject.SpawnFromFile(trackObjectName, transform.position, transform.eulerAngles, transform.localScale, collision);
    }

    public TrackObject ToTrackObject()
    {
        TrackObject TO = new TrackObject();

        TO.name = trackObjectName;

        TO.position = transform.position;
        TO.eulerAngles = transform.eulerAngles;
        TO.scale = transform.localScale;

        TO.collision = collision;

        return TO;
    }
}
