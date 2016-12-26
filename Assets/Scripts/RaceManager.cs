using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class RaceManager : MonoBehaviour
{
    public static RaceManager e;
    void Awake() { e = this; }

    public int lapsToRace = 3;

    public Renderer trackRenderer;

    public Texture2D tex;

    public Gradient grassMarksGradient;

    public Player[] players;

    public bool doCountdown = true;

    public VehiclePaint[] vehiclePaintSchemes;

    public GameObject playerPrefab;
    public int numberOfPlayers;

    private void Start()
    {
        if (screenText)
            screenText.text = "";

        InitTexture();

        InitPlayers();

        PopulateGrid();

        if (doCountdown)
            StartCoroutine(DoCountdown());
    }

    public void InitPlayers()
    {
        if (numberOfPlayers == 0) return;

        players = new Player[numberOfPlayers];

        for (int i = 0; i < numberOfPlayers; i++)
        {
            GameObject playerGO = Instantiate(playerPrefab);

            Player player = playerGO.GetComponent<Player>();

            players[i] = player;

            // init controllers

            VehicleInput playerInput = players[i].GetComponent<VehicleInput>();
            int p = i + 1;
            playerInput.steerAxis = "P" + p + " Horizontal";
            playerInput.accelAxis = "P" + p + " Vertical";
            playerInput.handbrakeButton = "P" + p + " Handbrake";

            player.name = player.name.Replace("(Clone)", "");
            player.name = "P" + p + "_" + player.name;
        }

        for (int i = 0; i < players.Length; i++)
        {
            vehiclePaintSchemes[i].Paint(players[i].vehicleBody);
        }
    }

    public Text screenText;

    IEnumerator DoCountdown()
    {
        foreach (var player in players)
        {
            player.EnableInput(false);
        }

        yield return new WaitForSeconds(1);

        // 3
        if (screenText)
            screenText.text = "3";

        yield return new WaitForSeconds(1);

        // 2
        if (screenText)
            screenText.text = "2";

        yield return new WaitForSeconds(1);

        // 1
        if (screenText)
            screenText.text = "1";

        yield return new WaitForSeconds(1);

        // START!
        if (screenText)
            screenText.text = "";

        foreach (var player in players)
        {
            player.EnableInput(true);
        }


    }

    void PopulateGrid()
    {
        GridHelper[] grids = FindObjectsOfType<GridHelper>();

        foreach (var grid in grids)
        {
            int i = -1;

            if (!int.TryParse(grid.name, out i))
            {
                Debug.LogError("Invalid grid object name, must be int");
                continue;
            }

            if (i >= players.Length) continue;

            players[i].transform.position = grid.transform.position;
            players[i].transform.rotation = grid.transform.rotation;
        }
    }

    /// <summary>
    /// Prepares the texture for pixel setting.
    /// Copies the texture asset and sets to the track material.
    /// </summary>
    void InitTexture()
    {
        Texture2D origTex = e.trackRenderer.material.mainTexture as Texture2D;
        tex = Instantiate(origTex) as Texture2D;

        colors = tex.GetPixels32();

        trackRenderer.material.mainTexture = tex;
    }

    [HideInInspector]
    public Color32[] colors;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);

        // Apply main texture every frame
        tex.SetPixels32(colors);
        tex.Apply();
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

    // Portals

    public int GetNumberOfPortals()
    {
        GameObject portalGO = GameObject.Find("Portals");

        if (!portalGO)
        {
            Debug.Log("Portals not found");
            return 0;
        }

        return portalGO.transform.childCount;
    }

    public void EndRace(Player player)
    {
        screenText.text = "RACE ENDED!\n" +
            player.name + " has won!" + "\n" +
            "Fastest lap: " + player.GetFastestTime();

        Debug.Log("RACE ENDED!");
    }

    public Track track;

    [ContextMenu("Serialize Track")]
    public void SerializeTrack()
    {
        track.SerializeToFile();
    }

    [ContextMenu("Deserialize")]
    public void DeserializeTrackTest()
    {
        track = Track.GetFromFile("MyTrack");
    }
}
