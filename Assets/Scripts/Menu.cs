using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject menu;

    List<Transform> textTransforms = new List<Transform>();

    public AudioClip selectClip;
    public AudioClip confirmClip;

    Option currentOption { get { return tabber.GetTree()[selectedIndex]; } }

    public class Tab
    {
        public string name = "NONAME";

        public Tab() { }
        public Tab(string name) { this.name = name; }

        public List<Option> options = new List<Option>();

        public Option[] GetTree()
        {
            List<Option> optionTree = new List<Option>();

            for (int i = 0; i < options.Count; i++)
            {
                optionTree.Add(options[i]);

                if (options[i].HasTabs)
                    optionTree.AddRange(options[i].CurrentTab.GetTree());
            }

            return optionTree.ToArray();
        }
    }

    public class Option
    {
        // what goes before the options text 'Preface: Option1'
        public string preface;

        public Tab[] tabs; // if added will switch tabs;
        public string[] subOptions; // else it will switch suboptions

        public int currentIndex;

        // Properties
        public Tab CurrentTab { get { return tabs[currentIndex]; } }
        public string CurrentOption { get { return subOptions[currentIndex]; } }

        public bool HasTabs { get { return tabs != null && tabs.Length > 0; } }

        /// <summary>
        /// A number of subOptions. If contains tabs, returns number of tabs
        /// </summary>
        public int Length
        {
            get
            {
                if (HasTabs)
                    return tabs.Length;

                if (subOptions == null) return 0;

                return subOptions.Length;
            }
        }

        // Delegates
        public delegate void ConfirmHandler(); // a delegate for confirming
        public ConfirmHandler Confirm;
        public delegate void SelectHandler(int i); // a delegate for selecting
        public SelectHandler Select;
        public delegate void BecomeSelectedHandler();
        public BecomeSelectedHandler BecomeSelected;
        public delegate void EndSelectedHandler();
        public EndSelectedHandler EndSelected;

        // Constructors
        public Option() { }

        public Option(string preface, params string[] subOptions)
        {
            this.preface = preface;

            if (subOptions == null || subOptions.Length == 0) return;

            this.subOptions = new string[subOptions.Length];

            for (int i = 0; i < subOptions.Length; i++)
            {
                this.subOptions[i] = subOptions[i];
            }
        }

        // Methods

        public Option[] GetTree()
        {
            if (!HasTabs) return null;

            List<Option> options = new List<Option>();

            options.Add(this);

            options.AddRange(CurrentTab.GetTree());

            return options.ToArray();
        }

        public override string ToString()
        {
            if (Length == 0)
                return preface;

            return preface + subOptions[currentIndex];
        }

        public Tab GetCurrentTab()
        {
            Debug.Assert(HasTabs, "You are requesting tabs, but this option doesn't have them");

            return tabs[currentIndex];
        }

        // Delegate calls

        public void SelectBy(int by)
        {
            currentIndex += by;

            if (currentIndex >= Length)
                currentIndex = 0;

            if (currentIndex < 0)
                currentIndex = Length - 1;

            if (Select != null)
                Select(currentIndex);
        }

        public void Selected()
        {
            if (BecomeSelected != null)
                BecomeSelected();
        }

        public void Deselected()
        {
            if (EndSelected != null)
                EndSelected();
        }
    }

    void Start()
    {
        foreach (Transform item in menu.transform)
            textTransforms.Add(item);

        InitMenu();

        ShowMenu(false);
    }

    Option tabber;

    void InitMenu()
    {
        // Create tab selection option
        tabber = new Option("", "Drivers", "Tracks"); // , "Drive!"

        // DRIVER tab
        Tab driversTab = new Tab();

        Option selectPlayer = new Option("Player: ", "P1", "P2");

        Tab P1 = new Tab();
        Tab P2 = new Tab();

        string[] controls = InputManager.e.GetControlSchemeNames();
        string[] vehicles = GameManager.e.GetVehicleNames();

        Option P1_controls = new Option("Controls: ", controls);
        P1_controls.Select = SelectPlayerControlScheme;
        Option P2_controls = new Option("Controls: ", controls);
        P2_controls.Select = SelectPlayerControlScheme;

        Option P1_vehicles = new Option("Vehicles: ", vehicles);
        P1_vehicles.Select = SelectPlayerVehicle;
        Option P2_vehicles = new Option("Vehicles: ", vehicles);
        P2_vehicles.Select = SelectPlayerVehicle;

        P1.options.Add(P1_controls);
        P1.options.Add(P1_vehicles);

        P2.options.Add(P2_controls);
        P2.options.Add(P2_vehicles);

        selectPlayer.tabs = new Tab[] { P1, P2 };
        selectPlayer.Select = SelectPlayerControlScheme;

        selectPlayer.Select = SelectPlayer;

        driversTab.options.Add(selectPlayer);

        // TRACKS tab
        Tab tracksTab = new Tab();

        Option selectTrack = new Option("", TrackManager.e.GetLayoutNames());
        selectTrack.Select = SelectTrack;
        tracksTab.options.Add(selectTrack);

        Option driveTrack = new Option("Drive");
        driveTrack.Confirm = DriveTrack;
        tracksTab.options.Add(driveTrack);

        // DRIVE option is currently in Tracks
        // DRIVE! tab, should be empty, maybe overview?
        // Tab driveNowTab = new Tab();
        // driveNowTab.options.Add(tabber);

        // Now add tabs to tabber option
        tabber.tabs = new Tab[] { driversTab, tracksTab };
    }

    Text[] _texts;

    Text[] UITexts
    {
        get
        {
            if (_texts != null) return _texts;

            _texts = new Text[textTransforms.Count];

            for (int i = 0; i < _texts.Length; i++)
                _texts[i] = textTransforms[i].GetComponent<Text>();

            return _texts;
        }

    }

    Text UIText { get { return UITexts[selectedIndex]; } }

    void Refresh()
    {
        for (int i = 0; i < UITexts.Length; i++)
        {
            if (i < Options.Length)
                UITexts[i].text = Options[i].ToString();
            else UITexts[i].text = "";

            // make all texts normal scale, and dewedge them
            UITexts[i].text = Dewedge(UITexts[i].text);
            UITexts[i].transform.localScale = Vector3.one;


        }

        // make this text bigger and wedge it
        UIText.text = Wedge(UIText.text);
        UIText.transform.localScale = Vector3.one * 1.3f;
    }

    int selectedIndex = 0;

    void Update()
    {
        UpdateInput();
    }

    void UpdateInput()
    {
        // TODO: Replace with smarter input for controllers too!

        if (Input.GetKeyDown(KeyCode.Escape))
            ShowMenu(!menu.activeSelf);

        if (!menu.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            VerticalSelect(1);
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            VerticalSelect(-1);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            HorizontalSelect(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            HorizontalSelect(1);

        if (Input.GetKeyDown(KeyCode.Return))
            ConfirmSelect();
    }

    Transform prevSelected;

    int prevSelectedOption;

    Option[] Options
    {
        get
        {
            return tabber.GetTree();
        }
    }

    Option CurrentOption { get { return Options[selectedIndex]; } }

    void VerticalSelect(int i)
    {
        selectedIndex = Wrap(selectedIndex + i, Options.Length);

        UITexts[prevSelectedOption].text = Dewedge(UITexts[prevSelectedOption].text);

        prevSelectedOption = selectedIndex;

        Refresh();
    }

    void HorizontalSelect(int i)
    {
        Option option = CurrentOption;

        option.SelectBy(i);

        Refresh();
    }

    public enum ControllerType { None, XboX, PS };
    public ControllerType[] controllerTypes = new ControllerType[4];

    void ConfirmSelect()
    {
        if (currentOption.Confirm != null)
        {
            currentOption.Confirm();

            // Play ConfirmClip
        }
    }

    void ChangeMode()
    {
        mode = Wrap(mode + 1, 3);
    }

    void ChangeModeOption(int by)
    {
        modeOption = Wrap(modeOption + by, 10);
    }

    int modeOption;

    string GetModeText()
    {
        string modeName = "";
        string optionName = "";

        switch (mode)
        {
            case 0:
                modeName = "Practice";
                optionName = "";
                break;

            case 1:
                modeName = "Points";
                optionName = "to " + modeOption;

                break;
            case 2:
                modeName = "Time";
                optionName = "" + modeOption + "mins";

                break;
            default:
                break;
        }

        return "Mode: " + modeName + " " + optionName;
    }

    int Wrap(int i, int length)
    {
        if (i > length - 1)
            i = 0;

        if (i < 0)
            i = length - 1;

        return i;
    }

    string ControllerToString(ControllerType ct)
    {
        switch (ct)
        {
            case ControllerType.None:
                return "None";
            case ControllerType.XboX:
                return "XboX";
            case ControllerType.PS:
                return "PS";
        }

        return "";
    }

    string Wedge(string str)
    {
        return "< " + str + " >";
    }

    string Dewedge(string str)
    {
        if (str.StartsWith("<") && str.EndsWith(">"))
            return str.Substring(2, str.Length - 4);

        return str;
    }

    public int mode = 0; // practice, 

    void ShowMenu(bool show)
    {
        // Play ConfirmClip

        menu.gameObject.SetActive(show);

        if (show)
        {
            Refresh();
            VerticalSelect(0);
        }
    }


    #region Custom Actions

    void TurnPlayerOn()
    {
        Debug.Log("Turned player on!");
    }

    void ListPlayerOptions(int i)
    {

    }

    void SelectTrack(int i)
    {
        TrackManager.e.track = TrackManager.e.AllLayouts[i];
    }



    void DriveTrack()
    {
        GameManager.e.InitSession();
        ShowMenu(false);
        GameManager.e.EndMainMenu();
        GameObject.Find("SkyCamera").GetComponent<Camera>().enabled = true; // Not so nice
        VerticalSelect(0);
    }

    int selectedPlayer;

    void SelectPlayer(int i)
    {
        selectedPlayer = i;
    }

    void SelectPlayerControlScheme(int i)
    {
        int index = currentOption.currentIndex;

        GameManager.e.playerDatas[selectedPlayer].controlScheme = InputManager.e.controlSchemes[index];
    }

    void SelectPlayerVehicle(int i)
    {
        int index = currentOption.currentIndex;

        GameManager.e.playerDatas[selectedPlayer].vehicle = GameManager.e.vehicles[index];
    }

    #endregion
}

