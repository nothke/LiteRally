using UnityEngine;

public class TrackObjectEntity : MonoBehaviour
{
    public string trackObjectName;

    public bool isCollidable = true;

    private void Start()
    {
        TrackObject.SpawnFromFile(trackObjectName, transform.position, transform.eulerAngles, transform.localScale, isCollidable);
    }
}
