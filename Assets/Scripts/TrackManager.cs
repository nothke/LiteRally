using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    public static TrackManager e;
    void Awake() { e = this; }

    public GameObject trackGO;

    Renderer trackRenderer;
    public Mesh trackMesh;

    public Texture2D tex;

    public string loadFromFileName;

    public Transform[] gridPoints;

    public Track track;

    Transform _worldRoot;
    public Transform WorldRoot
    {
        get
        {
            if (!_worldRoot) _worldRoot = GameObject.Find("World").transform;

            return _worldRoot;
        }
    }

    void Start()
    {
        if (!string.IsNullOrEmpty(loadFromFileName))
        {
            DeserializeTrack(loadFromFileName);
            CreateTrack();
        }
    }

    [ContextMenu("Serialize Scene Track")]
    void SerializeSceneTrack()
    {

    }

    [ContextMenu("Deserialize")]
    void Deserialize()
    {
        DeserializeTrack(loadFromFileName);
    }

    public void DeserializeTrack(string trackName)
    {
        track = Track.GetFromFile(trackName);

        string dirPath = track.DirPath;

        // Runs a check and logs if something's wrong
        track.IsValid(debug: true);
    }

    public void CreateTrack()
    {
        if (!track.IsValid()) return;

        tex = track.GetTexture();

        trackRenderer.material.mainTexture = tex;

        CreatePortalObjects(track.portals);
        CreateGridObjects(track.grids);
    }

    // Do I need to store them?
    GameObject CreatePortalObjects(Portal[] portals)
    {
        GameObject rootGO = new GameObject("Portals");
        rootGO.transform.parent = WorldRoot;

        for (int i = 0; i < portals.Length; i++)
        {
            GameObject GO = new GameObject(i.ToString());
            GO.transform.position = portals[i].position;
            GO.transform.eulerAngles = portals[i].eulerAngles;
            GO.transform.parent = trackGO.transform;

            BoxCollider col = GO.AddComponent<BoxCollider>();
            col.center = portals[i].center;
            col.size = portals[i].size;
        }

        return rootGO;
    }

    GameObject CreateGridObjects(Grid[] grids)
    {
        GameObject rootGO = new GameObject("Grids");
        rootGO.transform.parent = WorldRoot;

        gridPoints = new Transform[grids.Length];

        for (int i = 0; i < grids.Length; i++)
        {
            GameObject GO = new GameObject(i.ToString());
            GO.transform.position = grids[i].position;
            GO.transform.eulerAngles = grids[i].eulerAngles;
            GO.transform.parent = trackGO.transform;

            // not in build?
            GO.AddComponent<GridHelper>();

            gridPoints[i] = GO.transform;
        }

        return rootGO;
    }

    // TODO: Pit objects, ^ just copy this basically
}
