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

    [Obsolete("Use GetFromFile(trackName, layoutName) instead")]
    public static Track GetFromFile(string layoutName)
    {
        string[] layoutPaths = GetLayoutPaths();

        foreach (var layoutPath in layoutPaths)
        {
            Track t = Deserialize(layoutPath);

            if (t.layoutName == layoutName)
                return t;
        }

        Debug.LogError("This layout doesn't exist");
        return null;
    }

    // OLD
    /*
    public static Track GetFromFile(string trackName, string layoutName)
    {
        string serialized = File.ReadAllText(GetLayoutPath(trackName, layoutName));
        return JsonUtility.FromJson<Track>(serialized);
    }*/

    // NEW - should be not be broken
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
    public Vector3 eulerAngles;
    public Vector3 scale;

    public enum CollisionType { None, Mesh, Box, Sphere, Capsule };
    public CollisionType collision;

    // properties

    public string DirPath { get { return "GameData/Objects/" + name + "/"; } }
    public string FilePath { get { return DirPath + name + ".obj"; } }

    public bool Exists()
    {
        if (!File.Exists(FilePath))
        {
            throw new Exception("No track object named + " + name + " exists");
            //Debug.LogError("No track object named + " + name + " exists");
            //return false;
        }

        return true;
    }

    public GameObject Spawn()
    {
        if (!Exists()) return null;

        GameObject GO = OBJLoader.LoadOBJFile(FilePath);

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
                    case CollisionType.Sphere:
                        cGO.AddComponent<SphereCollider>();
                        break;
                    case CollisionType.Capsule:
                        cGO.AddComponent<CapsuleCollider>();
                        break;
                }
            }
        }

        GO.transform.position = position;
        GO.transform.eulerAngles = eulerAngles;
        GO.transform.localScale = scale;

        return GO;
    }

    public static void SpawnFromFile(string name, Vector3 position, Vector3 eulerAngles, Vector3 scale, CollisionType collision = CollisionType.None)
    {
        TrackObject TO = new TrackObject();

        TO.name = name;

        if (!TO.Exists()) return;

        TO.position = position;
        TO.eulerAngles = eulerAngles;
        TO.scale = scale;

        TO.collision = collision;

        TO.Spawn();
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
