using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager e;
    void Awake() { e = this; }

    public GameObject mainMenuObject;

    public bool trackTestingMode;

    public List<PlayerData> playerDatas = new List<PlayerData>();
    public VehicleController[] vehicles;

    public string[] GetVehicleNames()
    {
        string[] names = new string[vehicles.Length];

        for (int i = 0; i < vehicles.Length; i++)
            names[i] = vehicles[i].name;

        return names;
    }

    private void Start()
    {
        if (!trackTestingMode)
        {
            // Start the MAIN MENU
            mainMenuObject.SetActive(true);

            // find all cars and destroy them
            VehicleController[] cars = FindObjectsOfType<VehicleController>();

            if (cars != null)
                for (int i = 0; i < cars.Length; i++)
                    Destroy(cars[i].gameObject);

            InitPlayerData();
            InitTrackData();

            // disable the trackmanager for the main menu
            TrackManager.e.enabled = false;

        }
        else
        {
            EndMainMenu();

            InitSession();
        }
    }

    void InitPlayerData()
    {
        int players = 2;

        for (int i = 0; i < players; i++)
        {
            playerDatas.Add(new PlayerData());

            playerDatas[i].controlScheme = InputManager.e.controlSchemes[0];
            playerDatas[i].vehicle = vehicles[i];
        }
    }

    void InitTrackData()
    {

    }

    public void EndMainMenu()
    {
        if (!mainMenuObject.activeSelf) return;

        mainMenuObject.SetActive(false);
    }

    public void InitSession()
    {
        TrackManager.e.enabled = true;
        TrackManager.e.InitThisTrack();
        RaceManager.e.InitRace();
    }
}
