using UnityEngine;

public class TrackObjectEntity : MonoBehaviour
{
    public string trackObjectName;

    //public TrackObject.CollisionType collision;

    private void Start()
    {
        //TrackObject.SpawnFromFile(trackObjectName, transform.position, transform.eulerAngles, transform.localScale, collision);
    }

    public TrackObject ToTrackObject()
    {
        TrackObject TO = new TrackObject();

        TO.name = trackObjectName;

        TO.position = transform.position;
        TO.rotation = transform.eulerAngles;
        TO.scale = transform.localScale;

        //TO.collision = collision;

        return TO;
    }

    [ContextMenu("Serialize To File TEST")]
    void SerializeToFile()
    {
        Object to = new Object();
        to.name = trackObjectName;
        to.collision = Object.CollisionType.Mesh;

        string serialized = JsonUtility.ToJson(to, true);
        System.IO.File.WriteAllText("GameData/Objects/" + trackObjectName + "/" + trackObjectName + ".json", serialized);
    }

    [ContextMenu("Try Deserialize")]
    void TryDeserialize()
    {
        string serialized = System.IO.File.ReadAllText("GameData/Objects/" + trackObjectName + "/" + trackObjectName + ".json");
        JsonUtility.FromJson<Object>(serialized);

        Debug.Log("Deserialized successfully");
    }
}
