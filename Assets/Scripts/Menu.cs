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

    public string[] currentOptions;

    Tab currentTab;
    Option currentOption { get { return currentTab.options[selectedItem]; } }

    public class Tab
    {
        public List<Option> options = new List<Option>();
    }

    public class Option
    {
        //public bool isSwitchable; // subOptions.Length alrady doubles for this
        public bool isConfirmable;

        public Tab[] tabs; // if added will switch tabs;

        public string preface;
        public string[] subOptions;

        public int currentOption;

        public delegate void ConfirmHandler(); // a delegate for confirming
        public ConfirmHandler Confirm;
        public delegate void SelectHandler(int i); // a delegate for selecting
        public SelectHandler Select;

        public override string ToString()
        {
            if (Length == 0)
                return preface;

            return preface + subOptions[currentOption];
        }

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

        public bool HasTabs { get { return tabs != null && tabs.Length > 0; } }

        public Tab GetCurrentTab()
        {
            Debug.Assert(HasTabs, "You are requesting tabs, but this option doesn't have them");

            return tabs[currentOption];
        }

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

        public void SelectBy(int by)
        {
            currentOption += by;

            if (currentOption >= Length)
                currentOption = 0;

            if (currentOption < 0)
                currentOption = Length - 1;

            if (Select != null)
                Select(currentOption);
        }
    }

    void Start()
    {
        foreach (Transform item in menu.transform)
            textTransforms.Add(item);

        InitMenu();

        ShowMenu(false);
    }

    void InitMenu()
    {
        // Create tab option
        Option tabber = new Option("", "Drivers", "Tracks", "Drive!");

        // DRIVER tab
        Tab driversTab = new Tab();
        driversTab.options.Add(tabber);

        Option selectPlayer = new Option();
        selectPlayer.subOptions = new string[] { "P1", "P2", "P3", "P4" };
        selectPlayer.Confirm = TurnPlayerOn;
        driversTab.options.Add(selectPlayer);

        Option controller = new Option("Controls: ", "WASD", "Arrows", "XboX 1", "XboX 2");
        driversTab.options.Add(controller);

        Option vehicle = new Option("Vehicle: ", "Skewt", "Formulite");
        driversTab.options.Add(vehicle);

        // TRACKS tab
        Tab tracksTab = new Tab();
        tracksTab.options.Add(tabber);

        Option selectTrack = new Option("", TrackManager.e.GetLayoutNames());
        selectTrack.Select = SelectTrack;
        tracksTab.options.Add(selectTrack);

        Option driveTrack = new Option("Drive");
        driveTrack.Confirm = DriveTrack;
        tracksTab.options.Add(driveTrack);

        /*
        // DRIVE! tab, should be empty, maybe overview?
        Tab driveNowTab = new Tab();
        driveNowTab.options.Add(tabber);*/

        // Now add tabs to tabber option
        tabber.tabs = new Tab[] { driversTab, tracksTab };

        currentTab = driversTab;
    }

    Text[] _texts;

    Text[] texts
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

    void Refresh()
    {
        for (int i = 0; i < texts.Length; i++)
        {
            if (i >= currentTab.options.Count)
                texts[i].text = "";
            else
                texts[i].text = currentTab.options[i].ToString();
        }
    }

    int selectedItem = 0;

    // Update is called once per frame
    void Update()
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
    string prevText;

    Text selectedText;

    int prevSelectedOption;

    void VerticalSelect(int i)
    {
        // if there is just one item, do nothing
        if (currentTab.options.Count == 1)
            return;

        selectedItem = Wrap(selectedItem + i, currentTab.options.Count);

        Transform selectedT = textTransforms[selectedItem];

        if (prevSelectedOption != selectedItem)
        {
            texts[prevSelectedOption].text = Dewedge(texts[prevSelectedOption].text);
        }

        selectedText = texts[selectedItem];
        prevText = selectedText.text;
        selectedText.text = Wedge(selectedText.text);
        //selectedText.text = "< " + selectedText.text + " >";

        selectedT.localScale = Vector3.one * 1.3f;

        if (prevSelected)
            prevSelected.localScale = Vector3.one;

        prevSelected = selectedT;

        Refresh();
    }

    public enum ControllerType { None, XboX, PS };
    public ControllerType[] controllerTypes = new ControllerType[4];

    void HorizontalSelect(int i)
    {
        Option option = currentTab.options[selectedItem];

        option.SelectBy(i);

        if (option.HasTabs)
        {
            currentTab = option.GetCurrentTab();

            if (currentTab.options.Count == 0)
                throw new System.Exception("Tab should not be empty. Did you forget to add a tab selection option?");
        }

        Refresh();
    }



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
            Refresh();
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
    }

    #endregion
}

