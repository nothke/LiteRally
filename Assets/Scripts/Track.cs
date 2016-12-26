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

public class TrackObject
{
    public string name;

    public Vector3 position;
    public Vector3 eulerAngles;
    public Vector3 scale;
}