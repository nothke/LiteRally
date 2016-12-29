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

    [Header("Load from File")]
    public bool loadFromFile;
    public string loadTrack;
    public string loadLayout;

    public Transform[] gridPoints;

    [Header("TRACK")]
    public Track track;

    bool trackHasBeenLoadedFromFile;

    [Header("OTHER")]
    public SurfaceData surfaceData;

    public Object[] objects;
    GameObject[] sourceObjects;

    void LoadObjects()
    {
        objects = Object.GetAll();

        GameObject sourceGO = GetOrCreate("SourceObjects", null);
        sourceGO.SetActive(false);

        if (objects == null) throw new System.Exception("No objects found. Null");
        if (objects.Length == 0) throw new System.Exception("No objects found. 0");

        sourceObjects = new GameObject[objects.Length];

        for (int i = 0; i < objects.Length; i++)
        {
            sourceObjects[i] = objects[i].Spawn();
            sourceObjects[i].transform.parent = sourceGO.transform;
        }
    }

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

    Track[] _allLayouts;
    public Track[] AllLayouts
    {
        get
        {
            if (_allLayouts == null)
                _allLayouts = GetLayoutsFromGameData();

            return _allLayouts;
        }
    }

    public Track[] GetLayoutsFromGameData()
    {
        string[] paths = Track.GetLayoutPaths();

        if (paths == null || paths.Length == 0)
            throw new System.Exception("No tracks in GameData");

        Track[] tracks = new Track[paths.Length];

        for (int i = 0; i < paths.Length; i++)
        {
            tracks[i] = Track.Deserialize(paths[i]);
        }

        return tracks;
    }

    public string[] GetLayoutNames()
    {
        string[] layoutNames = new string[AllLayouts.Length];

        for (int i = 0; i < layoutNames.Length; i++)
        {
            layoutNames[i] = AllLayouts[i].trackName + " - " + AllLayouts[i].layoutName;
        }

        return layoutNames;
    }

    public void InitThisTrack()
    {
        if (loadFromFile && !string.IsNullOrEmpty(loadLayout))
            DeserializeTrack(loadTrack, loadLayout);

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

        // Camera
        GameObject skyCamera = GameObject.Find("SkyCamera");
        track.cameraPosition = skyCamera.transform.position;
        track.cameraEulerAngles = skyCamera.transform.eulerAngles;

        // Portals

        GameObject portalsGO = GameObject.Find(portalsH);

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

        GameObject gridGO = GameObject.Find(gridH);

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

        Debug.Log("TOs size: " + TOs.Length);

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
        if (string.IsNullOrEmpty(loadTrack) || string.IsNullOrEmpty(loadLayout))
        {
            Debug.LogError("No name suppied to loadTrack or loadLayout");
            return;
        }

        /*
        if (!Track.Exists(loadTrack))
        {

        }*/

        if (!Track.Exists(loadTrack, loadLayout))
        {
            Debug.LogError("No track with name " + loadTrack + " - " + " exists");
            return;
        }

        DeserializeTrack(loadTrack, loadLayout);
    }

    public void DeserializeTrack(string trackName, string layoutName)
    {
        track = Track.GetFromFile(trackName, layoutName);

        string dirPath = track.DirPath;

        // Runs a check and logs if something's wrong
        track.IsValid(debug: true);
    }

    Camera _skyCamera;
    Camera SkyCamera
    {
        get
        {
            if (!_skyCamera) _skyCamera = GameObject.Find("SkyCamera").GetComponent<Camera>();

            return _skyCamera;
        }
    }

    Transform _bounds;
    Transform Bounds
    {
        get
        {
            if (!_bounds) _bounds = GameObject.Find("World/Bounds").transform;

            return _bounds;
        }
    }

    [ContextMenu("Create from Track")]
    public void CreateTrack()
    {
        if (!track.IsValid()) return;

        tex = track.GetTextureFromFile();

        if (!trackRenderer) Debug.LogError("No track renderer");

        // Resize track
        trackGO.transform.localScale = track.scale;

        // Resize bounds - border wall
        Bounds.transform.localScale = track.scale * 0.1f;

        // Reposition camera
        SkyCamera.transform.position = track.cameraPosition;
        SkyCamera.transform.eulerAngles = track.cameraEulerAngles;

        // Get texture
        if (Application.isPlaying)
            trackRenderer.material.mainTexture = tex;
        else trackRenderer.sharedMaterial.mainTexture = tex;

        // Create all stuff
        CreatePortalObjects(track.portals);
        CreateGridObjects(track.grids);
        CreateTrackObjects();

        //trackHasBeenLoadedFromFile = true;
    }

    // Addresses in hierarchy
    public const string portalsH = "World/Portals";
    public const string gridH = "World/Grid";
    public const string objectsH = "World/Objects";
    public const string pitsH = "World/Pits";
    public const string cameraH = "SkyCamera";

    /// <summary>
    /// Destroys portals, grid, pits and objects
    /// </summary>
    void CleanUpTrack()
    {
        GameObject portalRoot = GameObject.Find(portalsH);
        if (portalRoot) DestroyGO(portalRoot);

        GameObject gridRoot = GameObject.Find(gridH);
        if (gridRoot) DestroyGO(gridRoot);

        GameObject objectsRoot = GameObject.Find(objectsH);
        if (objectsRoot) DestroyGO(objectsRoot);

        // TODO: when pits are implemented, uncomment:
        /*
        GameObject pitsRoot = GameObject.Find(pitsH);
        if (pitsRoot) DestroyGO(pitsRoot);
        */
    }

    GameObject GetOrCreate(string hierarchyPath, Transform atParent = null)
    {
        GameObject GO = GameObject.Find(hierarchyPath);

        if (!GO)
        {
            GO = new GameObject(System.IO.Path.GetFileName(hierarchyPath));
            GO.transform.parent = atParent;
        }

        return GO;
    }

    void DestroyGO(GameObject go)
    {
        if (Application.isPlaying)
            Destroy(go);
        else
            DestroyImmediate(go);
    }

    void DestroyChildren(GameObject go)
    {
        var children = new List<GameObject>();

        foreach (Transform child in go.transform)
            children.Add(child.gameObject);

        children.ForEach(child => DestroyGO(child));
    }

    // Do I need to store them?
    GameObject CreatePortalObjects(Portal[] portals)
    {
        GameObject rootGO = GetOrCreate(portalsH, WorldRoot);
        DestroyChildren(rootGO);

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
        GameObject rootGO = GetOrCreate(gridH, WorldRoot);
        DestroyChildren(rootGO);

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

        Debug.Assert(rootGO);

        return rootGO;
    }

    // TODO: Pit objects, ^ just copy this basically

    void CreateTrackObjects()
    {
        LoadObjects();

        GameObject rootGO = GetOrCreate(objectsH, WorldRoot);
        DestroyChildren(rootGO);

        if (track.trackObjects == null)
        {
            Debug.LogWarning("Track has no objects");
            return;
        }

        foreach (var TO in track.trackObjects)
        {
            GameObject sGO = null;

            foreach (var so in sourceObjects)
                if (so.name == TO.name)
                    sGO = so;

            if (sGO == null) throw new System.Exception("There is no such object loaded");

            GameObject GO = Instantiate(sGO);
            GO.transform.parent = rootGO.transform;

            GO.transform.position = TO.position;
            GO.transform.eulerAngles = TO.rotation;
            GO.transform.localScale = TO.scale;

            if (!Application.isPlaying)
            {
                // Doesn't work with GetComponent() :(
                // GO.hideFlags = HideFlags.DontSave;

                TrackObjectEntity TOE = GO.AddComponent<TrackObjectEntity>();
                TOE.trackObjectName = TO.name;
            }
        }
    }

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
        if (!tex) return;

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
