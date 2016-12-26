using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[System.Serializable]
public class Track
{
    [NonSerialized] // Remove?
    public Texture texture;
    [NonSerialized] // Remove?
    public Mesh mesh;

    // SERIALIZED:
    public string name = "MyTrack";

    public Vector3 scale;

    public Portal[] portals;
    public Grid[] grids;
    public Pit[] pits;

    public void SerializeToFile()
    {
        if (!Directory.Exists(DirPath))
            Directory.CreateDirectory(DirPath);

        string fileName = name + ".json";

        string serialized = JsonUtility.ToJson(this, true);

        File.WriteAllText(DirPath + fileName, serialized);

        Debug.Log("Saved " + name + " track to file successfully");
    }

    public string DirPath
    {
        get
        {
            return "GameData/Tracks/" + name + "/";
        }
    }

    public static string GetDirPath(string trackName)
    {
        return "GameData/Tracks/" + trackName + "/";
    }

    public static string GetFilePath(string trackName)
    {
        return GetDirPath(trackName) + trackName + ".json";
    }

    internal static Track GetFromFile(string fileName)
    {
        string serialized = File.ReadAllText(GetFilePath(fileName));
        return JsonUtility.FromJson<Track>(serialized);
    }

    /// <summary>
    /// Gets texture from file at the same name as track and in the same folder
    /// </summary>
    /// <returns>Returns texture, null if it doesn't exist or is invalid</returns>
    public Texture2D GetTextureFromFile()
    {
        string filePath = "";

        if (File.Exists(DirPath + name + ".jpg"))
            filePath = DirPath + name + ".jpg";

        if (File.Exists(DirPath + name + ".png"))
            filePath = DirPath + name + ".png";

        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("Texture doesn't exist");
            return null;
        }

        //filePath += ".binary";

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(filePath));

        return tex;
    }

    public static bool Exists(string trackName)
    {
        if (!Directory.Exists(GetDirPath(trackName)))
            return false;

        if (!File.Exists(GetFilePath(trackName)))
            return false;

        return true;
    }

    /// <summary>
    /// Runs a check and can log errors if something's wrong
    /// </summary>
    /// <param name="debug">log errors?</param>
    /// <returns>Returns true if everything is fine with the track</returns>
    public bool IsValid(bool debug = false)
    {
        string logErrors = "";

        // check if there is texture of the same name
        if (!File.Exists(DirPath + name + ".jpg") &&
            !File.Exists(DirPath + name + ".png"))
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
                Debug.Log("Track is valid, no errors found");

            return true;
        }

        if (debug)
            Debug.Log("Track is invalid, there are some problems:" + logErrors);

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
    public string name;

    public Vector3 position;
    public Vector3 eulerAngles;
    public Vector3 scale;
}