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
        playerDatas.Add(new PlayerData());
        playerDatas.Add(new PlayerData());

        if (!trackTestingMode)
        {
            // Final game setting, start the "main menu"
            mainMenuObject.SetActive(true);

            // find all cars and turn them off
            VehicleController[] cars = FindObjectsOfType<VehicleController>();

            if (cars != null)
                foreach (var car in cars)
                    car.gameObject.SetActive(false);

            TrackManager.e.enabled = false;

        }
        else
        {
            EndMainMenu();

            InitSession();
        }
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
