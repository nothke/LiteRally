using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager e;
    void Awake() { e = this; }

    public GameObject mainMenuObject;
    public Texture2D menuTrackTexture;

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
        DestroyAllCarsInScene();

        // Temp solution:
        TrackManager.e.trackRenderer.material.mainTexture = menuTrackTexture;

        InitPlayerData();
        InitTrackData();

        var tester = GetComponent<TrackTester>();
        if (tester && tester.enabled)
        {
            tester.SetNamesToTrackManager();

            EndMainMenu();
            InitSession();

            return;
        }

        // Start the MAIN MENU
        mainMenuObject.SetActive(true);

        // disable the trackmanager for the main menu
        TrackManager.e.enabled = false;
    }

    void DestroyAllCarsInScene()
    {
        VehicleController[] cars = FindObjectsOfType<VehicleController>();

        if (cars != null)
            for (int i = 0; i < cars.Length; i++)
                Destroy(cars[i].gameObject);
    }

    void InitPlayerData()
    {
        int players = 2;

        for (int i = 0; i < players; i++)
        {
            playerDatas.Add(new PlayerData());

            playerDatas[i].controlScheme = InputManager.e.controlSchemes[i];
            playerDatas[i].vehicle = vehicles[0];
        }
    }

    void InitTrackData()
    {
        TrackManager.e.track = TrackManager.e.AllLayouts[0];
    }

    public void EndMainMenu()
    {
        if (!mainMenuObject.activeSelf) return;

        mainMenuObject.SetActive(false);
    }

    public void InitSession()
    {
        Menu.e.gameTitle.SetActive(false);
        TrackManager.e.enabled = true;
        TrackManager.e.InitThisTrack();
        RaceManager.e.InitRace();

        EnableRaceSkyCamera();
    }

    public void EnableRaceSkyCamera()
    {
        // Not so nice, but fine for now:
        GameObject.Find("SkyCamera").GetComponent<Camera>().enabled = true;
    }
}
