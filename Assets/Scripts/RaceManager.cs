using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

[System.Serializable]
public class PlayerData
{
    public string name;
    public Player player;

    public VehicleController vehicle;
    public ControlScheme controlScheme;
}

public class RaceManager : MonoBehaviour
{
    public static RaceManager e;
    void Awake() { e = this; }

    public int lapsToRace = 3;

    public Gradient grassMarksGradient;

    public Player[] players;
    public ControlScheme[] playerControlSchemes;

    public bool doCountdown = true;

    public VehiclePaint[] vehiclePaintSchemes;

    public GameObject playerPrefab;
    public int numberOfPlayers;

    public void InitRace()
    {
        // TODO: move screenText and related functions to UIManager
        if (screenText)
            screenText.text = "";

        InitPlayers();

        PopulateGrid();

        if (doCountdown)
            StartCoroutine(DoCountdown());
    }

    public void InitPlayers()
    {
        playerControlSchemes = new ControlScheme[numberOfPlayers];

        if (numberOfPlayers == 0) return;

        if (players.Length > 0)
            for (int i = 0; i < players.Length; i++)
                Destroy(players[i].gameObject);

        players = new Player[numberOfPlayers];

        for (int i = 0; i < numberOfPlayers; i++)
        {
            GameObject playerGO = Instantiate(playerPrefab);

            Player player = playerGO.GetComponent<Player>();

            players[i] = player;

            // init controllers
            player.GetComponent<VehicleInput>().controlScheme = playerControlSchemes[i];

            /*
            VehicleInput playerInput = players[i].GetComponent<VehicleInput>();
            playerInput.steerAxis = "P" + p + " Horizontal";
            playerInput.accelAxis = "P" + p + " Vertical";
            playerInput.handbrakeButton = "P" + p + " Handbrake";
            */

            int p = i + 1;

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
        foreach (var grid in TrackManager.e.gridPoints)
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



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    // Portals

    // What was this for?
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
}
