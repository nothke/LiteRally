using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[Serializable]
public class Track
{
    // SERIALIZED:
    public string trackName = "MyTrack";
    public string layoutName = "MyLayout";

    public Vector3 scale;

    public Vector3 cameraPosition;
    public Vector3 cameraEulerAngles;

    public Portal[] portals;
    public Grid[] grids;
    public Pit[] pits;
    public TrackObject[] trackObjects;

    public void SerializeToFile()
    {
        if (!Directory.Exists(DirPath))
            Directory.CreateDirectory(DirPath);

        string fileName = layoutName + ".json";
        string path = DirPath + fileName;

        string serialized = JsonUtility.ToJson(this, true);

        File.WriteAllText(path, serialized);

        Debug.Log("Successfully saved " + layoutName + " layout to " + path);
    }

    public string DirPath
    {
        get
        {
            return "GameData/Tracks/" + trackName + "/";
        }
    }

    public static string GetDirPath(string trackName)
    {
        return "GameData/Tracks/" + trackName + "/";
    }

    public static string GetLayoutPath(string trackName, string layoutName)
    {
        return GetDirPath(trackName) + layoutName + ".json";
    }

    public static Track GetFromFile(string trackName, string layoutName)
    {
        return Deserialize(GetLayoutPath(trackName, layoutName));
    }

    /// <summary>
    /// Gets texture from file at the same name as track and in the same folder
    /// </summary>
    /// <returns>Returns texture, null if it doesn't exist or is invalid</returns>
    public Texture2D GetTextureFromFile()
    {
        string filePath = "";

        if (File.Exists(DirPath + trackName + ".jpg"))
            filePath = DirPath + trackName + ".jpg";

        if (File.Exists(DirPath + trackName + ".png"))
            filePath = DirPath + trackName + ".png";

        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("Texture doesn't exist");
            return null;
        }

        Texture2D tex = new Texture2D(2, 2);

        float t = Time.realtimeSinceStartup;
        tex.LoadImage(File.ReadAllBytes(filePath));
        t = Time.realtimeSinceStartup - t;
        Debug.Log("Loading image from file completed in: " + t);

        return tex;
    }

    // NEW
    /// <summary>
    /// Get a list of all layout paths
    /// </summary>
    /// <param name="log">Debug.Log all names?</param>
    public static string[] GetLayoutPaths(bool log = false)
    {
        string dir = "GameData/Tracks/";

        List<string> layouts = new List<string>();

        if (log)
            foreach (var layout in layouts)
                Debug.Log(layout);

        return Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories);
    }

    public static bool Exists(string trackName, string layoutName)
    {
        string[] layoutPaths = GetLayoutPaths();

        foreach (var layoutPath in layoutPaths)
        {
            Track t = Deserialize(layoutPath);

            if (t.layoutName == layoutName && t.trackName == trackName)
                return true;
        }

        return false;
    }

    // NEW
    /// <summary>
    /// Does track of this name exist in GameData?
    /// </summary>
    public static bool Exists(string trackName)
    {
        string[] layoutPaths = GetLayoutPaths();

        foreach (var layoutPath in layoutPaths)
        {
            Track t = Deserialize(layoutPath);

            if (t.trackName == trackName)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets Track object from layoutPath
    /// </summary>
    public static Track Deserialize(string layoutPath)
    {
        if (!File.Exists(layoutPath))
            throw new Exception("Layout with this path doesn't exist");

        string serialized = File.ReadAllText(layoutPath);
        return JsonUtility.FromJson<Track>(serialized);
    }

    /// <summary>
    /// Runs a check and can log errors if something's wrong
    /// </summary>
    /// <param name="debug">log errors?</param>
    /// <returns>Returns true if everything is fine with the track</returns>
    public bool IsValid(bool debug = false)
    {
        string logErrors = "";

        // OLD
        // check if there is texture of the same name
        if (!File.Exists(DirPath + trackName + ".jpg") &&
            !File.Exists(DirPath + trackName + ".png"))
        {
            logErrors += "\n - Texture doesn't exist";
        }

        // TODO: Check if one of scale dimensions is 0

        if (grids == null || grids.Length == 0)
            logErrors += "\n - There are no grid positions";

        if (grids == null || grids.Length < 4)
            logErrors += "\n - There is less than 4 grid positions";

        if (portals == null || portals.Length == 0)
            logErrors += "\n - There are no portals";

        /*
        if (pits == null || pits.Length == 0)
            logErrors += "\n - There are no pits";
            */

        if (portals == null || portals.Length < 2)
            logErrors += "\n - There must be at least 2 portals";

        if (string.IsNullOrEmpty(logErrors))
        {
            if (debug)
                Debug.Log("Track " + trackName + " - " + layoutName + " is valid, no errors found");

            return true;
        }

        if (debug)
            Debug.Log("Track " + trackName + " - " + layoutName + " is invalid, there are some problems:" + logErrors);

        return false;
    }
}

[Serializable]
public class Portal
{
    public Vector3 position;
    public Vector3 eulerAngles;
    public Vector3 center;
    public Vector3 size;
}

[Serializable]
public class Grid
{
    public Vector3 position;
    public Vector3 eulerAngles;
}

[Serializable]
public class Pit
{
    public Vector3 position;
    public Vector3 eulerAngles;
}

// TODO:
[Serializable]
public class TrackObject
{
    // Serialized variables:
    public string name;

    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}

[Serializable]
public class Object
{
    // Name should be the same as folder/model
    public string name;

    public enum CollisionType { None, Mesh, Box } //, Sphere, Capsule };
    public CollisionType collision;

    // Properties
    public string DirPath { get { return "GameData/Objects/" + name + "/"; } }
    public string FilePath { get { return DirPath + name + ".obj"; } }

    public bool Exists()
    {
        if (!File.Exists(FilePath))
            throw new Exception("No track object named + " + name + " exists");

        return true;
    }

    public GameObject Spawn()
    {
        if (!Exists()) return null;

        // Load from .OBJ
        GameObject GO = OBJLoader.LoadOBJFile(FilePath);

        // Add collision
        if (collision != CollisionType.None)
        {
            foreach (Transform child in GO.transform)
            {
                GameObject cGO = child.gameObject;

                switch (collision)
                {
                    case CollisionType.Mesh:
                        cGO.AddComponent<MeshCollider>();
                        break;
                    case CollisionType.Box:
                        cGO.AddComponent<BoxCollider>();
                        break;
                }
            }
        }

        GO.name = name;

        return GO;
    }

    GameObject SpawnAt(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        GameObject GO = Spawn();

        if (!GO) return null;

        GO.transform.position = position;
        GO.transform.eulerAngles = rotation;
        GO.transform.localScale = scale;

        return GO;
    }

    /// <summary>
    /// Get a list of all objects in GameData/Objects folder
    /// </summary>
    /// <param name="log">Debug.Log all names?</param>
    public static string[] GetPaths(bool log = false)
    {
        string dir = "GameData/Objects/";

        List<string> objectPaths = new List<string>();

        if (log)
            foreach (var layout in objectPaths)
                Debug.Log(layout);

        return Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories);
    }

    public static Object[] GetAll()
    {
        string[] paths = GetPaths();

        if (paths == null || paths.Length == 0)
            return null;

        Object[] objects = new Object[paths.Length];

        for (int i = 0; i < paths.Length; i++)
            objects[i] = Deserialize(paths[i]);

        return objects;
    }

    public static Object Deserialize(string path)
    {
        if (!File.Exists(path))
            throw new Exception("Object with this path doesn't exist");

        string serialized = File.ReadAllText(path);
        return JsonUtility.FromJson<Object>(serialized);
    }
}

[Serializable]
public class SurfaceData
{
    public Surface tarmac;
    public Surface grass;
    public Surface dirt;
    public Surface sand;
    public Surface snow;
    public Surface ice;
    public Surface oil;

    public const string filePath = "GameData/surface.json";

    public void Save()
    {
        string serialized = JsonUtility.ToJson(this, true);
        File.WriteAllText(filePath, serialized);
    }

    public static SurfaceData Load()
    {
        string serialized = File.ReadAllText(filePath);
        return JsonUtility.FromJson<SurfaceData>(serialized);
    }
}

[Serializable]
public class Surface
{
    public float gripGain;
    public float resistance;
}
