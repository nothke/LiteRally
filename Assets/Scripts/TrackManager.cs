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

    bool trackHasBeenLoadedFromFile;

    public SurfaceData surfaceData;

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

    public void InitThisTrack()
    {
        if (string.IsNullOrEmpty(loadFromFileName)) return;

        DeserializeTrack(loadFromFileName);
        CreateTrack();

        InitTexture();
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

        TrackObjectEntity[] TOs = FindObjectsOfType<TrackObjectEntity>();

        if (TOs != null && TOs.Length > 0)
        {
            track.trackObjects = new TrackObject[TOs.Length];

            for (int i = 0; i < TOs.Length; i++)
            {
                track.trackObjects[i] = TOs[i].ToTrackObject();
            }
        }
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

        tex = track.GetTextureFromFile();

        if (!trackRenderer) Debug.LogError("No track renderer");
        trackRenderer.material.mainTexture = tex;

        CreatePortalObjects(track.portals);
        CreateGridObjects(track.grids);

        foreach (var TO in track.trackObjects)
            TO.Spawn();

        trackHasBeenLoadedFromFile = true;
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
            GO.transform.parent = rootGO.transform;

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
            GO.transform.parent = rootGO.transform;

            // not in build?
            GO.AddComponent<GridHelper>();

            gridPoints[i] = GO.transform;
        }

        return rootGO;
    }

    // TODO: Pit objects, ^ just copy this basically

    #region Texture Picking and Stamping


    [HideInInspector]
    public Color32[] colors;

    /// <summary>
    /// Prepares the texture for pixel setting.
    /// Copies the texture asset and sets to the track material.
    /// </summary>
    void InitTexture()
    {
        if (!trackHasBeenLoadedFromFile)
        {
            Texture2D origTex = trackRenderer.material.mainTexture as Texture2D;
            tex = Instantiate(origTex) as Texture2D;
        }

        colors = tex.GetPixels32();

        trackRenderer.material.mainTexture = tex;
    }


    int w { get { return tex.width; } }

    // PIXEL PAINTING

    public static void LerpPixel(Color color, float u, float v, float amount, int size = 1)
    {
        Texture2D tex = e.tex;

        int x = Mathf.RoundToInt(u * tex.width);
        int y = Mathf.RoundToInt(v * tex.height);

        e.LerpPixel(x, y, color, amount);

        if (size > 1)
        {
            e.LerpPixel(x - 1, y, color, amount);
            e.LerpPixel(x + 1, y, color, amount);
            e.LerpPixel(x, y + 1, color, amount);
            e.LerpPixel(x, y - 1, color, amount);
        }
    }

    public static void MultPixel(Color color, float u, float v, int size = 1)
    {
        Texture2D tex = e.tex;

        int x = Mathf.RoundToInt(u * tex.width);
        int y = Mathf.RoundToInt(v * tex.height);

        e.MultPixel(x, y, color);

        if (size > 1)
        {
            e.MultPixel(x - 1, y, color);
            e.MultPixel(x + 1, y, color);
            e.MultPixel(x, y + 1, color);
            e.MultPixel(x, y - 1, color);
        }
    }



    void MultPixel(int x, int y, Color color)
    {
        e.colors[y * w + x] *= color;
    }

    void LerpPixel(int x, int y, Color color, float amount)
    {
        e.colors[y * w + x] = Color32.Lerp(e.colors[y * w + x], color, amount);
    }



    #endregion

    private void Update()
    {
        // Apply main texture every frame
        tex.SetPixels32(colors);
        tex.Apply();
    }

    [ContextMenu("Save SurfaceData")]
    void SaveSurface()
    {
        surfaceData.Save();
    }

    [ContextMenu("Load SurfaceData")]
    void LoadSurface()
    {
        surfaceData = SurfaceData.Load();
    }
}
