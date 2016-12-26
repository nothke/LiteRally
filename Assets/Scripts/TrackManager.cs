using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    public static TrackManager e;
    void Awake() { e = this; }

    public GameObject trackGO;

    public Renderer trackRenderer;
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
            // Considering "World" exists, baaad programming
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

    [ContextMenu("Convert Scene to Track")]
    void SerializeSceneTrack()
    {
        // Scale
        if (!trackGO)
            trackGO = GameObject.Find("World/Track");

        track.scale = trackGO.transform.localScale;

        // Portals

        GameObject portalsGO = GameObject.Find("World/Portals");

        if (portalsGO)
        {
            int childCount = portalsGO.transform.childCount;
            track.portals = new Portal[childCount];

            for (int i = 0; i < childCount; i++)
            {
                Transform child = portalsGO.transform.GetChild(i);

                track.portals[i] = new Portal();
                track.portals[i].position = child.position;
                track.portals[i].eulerAngles = child.eulerAngles;

                BoxCollider boxCollider = child.GetComponent<BoxCollider>();
                // TODO: Check if has BoxCollider
                track.portals[i].center = boxCollider.center;
                track.portals[i].size = boxCollider.size;
            }
        }
        else Debug.LogError("Portals not found");

        // Grid

        GameObject gridGO = GameObject.Find("World/StartGrid");

        if (gridGO)
        {
            int childCount = gridGO.transform.childCount;
            track.grids = new Grid[childCount];

            for (int i = 0; i < childCount; i++)
            {
                Transform child = gridGO.transform.GetChild(i);

                track.grids[i] = new Grid();
                track.grids[i].position = child.position;
                track.grids[i].eulerAngles = child.eulerAngles;
            }
        }
        else Debug.LogError("Grid not found");

        /* Not pits for now
        
        // Pits

        GameObject pitsGO = GameObject.Find("World/Pits");
        
        if (pitsGO)
        {
            int childCount = pitsGO.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {

            }
        }*/

        // TODO: Trackside objects!
    }

    [ContextMenu("Save Track to File")]
    void SerializeToFile()
    {
        track.SerializeToFile();
    }

    [ContextMenu("Load Track from File")]
    void Deserialize()
    {
        if (string.IsNullOrEmpty(loadFromFileName))
        {
            Debug.LogError("No name assigned in loadFromFileName");
            return;
        }

        if (!Track.Exists(loadFromFileName))
        {
            Debug.LogError("No track with name " + loadFromFileName + " exists");
            return;
        }

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

        if (!trackRenderer) Debug.LogError("No track renderer");
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
            col.isTrigger = true;
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
