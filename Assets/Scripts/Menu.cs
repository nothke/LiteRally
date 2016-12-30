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

    //public string[] currentOptions;

    //Tab currentTab;
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
        public Tab[] tabs; // if added will switch tabs;

        public string preface;
        public string[] subOptions;

        public int currentIndex;

        public Tab CurrentTab { get { return tabs[currentIndex]; } }
        public string CurrentOption { get { return subOptions[currentIndex]; } }

        // Delegates
        public delegate void ConfirmHandler(); // a delegate for confirming
        public ConfirmHandler Confirm;
        public delegate void SelectHandler(int i); // a delegate for selecting
        public SelectHandler Select;
        public delegate void BecomeSelectedHandler();
        public BecomeSelectedHandler BecomeSelected;
        public delegate void EndSelectedHandler();
        public EndSelectedHandler EndSelected;

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

            return tabs[currentIndex];
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
        // Create tab option
        tabber = new Option("", "Drivers", "Tracks"); // , "Drive!"

        // DRIVER tab
        Tab driversTab = new Tab();
        //driversTab.options.Add(tabber);

        Option selectPlayer = new Option("Player: ", "P1", "P2");

        Tab P1 = new Tab();
        Tab P2 = new Tab();

        string[] controls = { "WASD", "Arrows", "XboX 1", "XboX 2" };
        string[] vehicles = { "Skewt", "Formulite" };

        Option P1_controls = new Option("Controls: ", controls);
        Option P2_controls = new Option("Controls: ", controls);

        Option P1_vehicles = new Option("Vehicles: ", vehicles);
        Option P2_vehicles = new Option("Vehicles: ", vehicles);

        P1.options.Add(P1_controls);
        P1.options.Add(P1_vehicles);

        P2.options.Add(P2_controls);
        P2.options.Add(P2_vehicles);

        selectPlayer.tabs = new Tab[] { P1, P2 };

        //selectPlayer.Confirm = TurnPlayerOn;
        driversTab.options.Add(selectPlayer);

        //Option controller = new Option("Controls: ", "WASD", "Arrows", "XboX 1", "XboX 2");
        //driversTab.options.Add(controller);

        //Option vehicle = new Option("Vehicle: ", "Skewt", "Formulite");
        //driversTab.options.Add(vehicle);

        // TRACKS tab
        Tab tracksTab = new Tab();
        //tracksTab.options.Add(tabber);

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

        //currentTab = driversTab;
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

    void Refresh()
    {
        //options = tabber.CurrentTab.GetTree();

        for (int i = 0; i < UITexts.Length; i++)
        {
            if (i < Options.Length)
                UITexts[i].text = Options[i].ToString();
            else UITexts[i].text = "";

            /*
            if (i < currentTab.options.Count)
            {
                if (i != 0 && currentTab.options[i].HasTabs) // if it has tabs, draw options below
                {



                    continue;
                }

                // write options
                UITexts[i].text = currentTab.options[i].ToString();


            }
            else // else clear text
                UITexts[i].text = "";*/
        }
    }

    int selectedIndex = 0;

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
    //string prevText;

    Text selectedText;

    int prevSelectedOption;

    //Option[] options;
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
        //options = tabber.CurrentTab.GetTree(); // currentTab.GetTree();

        // if there is just one item, do nothing
        //if (tabber.CurrentTab.options.Count == 1)
        //return;

        selectedIndex = Wrap(selectedIndex + i, Options.Length);

        Transform selectedT = textTransforms[selectedIndex];

        if (prevSelectedOption != selectedIndex)
        {
            UITexts[prevSelectedOption].text = Dewedge(UITexts[prevSelectedOption].text);
        }

        selectedText = UITexts[selectedIndex];
        //prevText = selectedText.text;
        selectedText.text = Wedge(selectedText.text);
        //selectedText.text = "< " + selectedText.text + " >";

        selectedT.localScale = Vector3.one * 1.3f;

        if (prevSelected)
            prevSelected.localScale = Vector3.one;

        prevSelected = selectedT;

        prevSelectedOption = selectedIndex;

        Refresh();
    }

    public enum ControllerType { None, XboX, PS };
    public ControllerType[] controllerTypes = new ControllerType[4];

    void HorizontalSelect(int i)
    {
        //if (selectedItem == 0)
        //Option option = currentTab.options[selectedIndex];
        Option option = CurrentOption;

        option.SelectBy(i);

        if (option.HasTabs)
        {
            //   currentTab = option.GetCurrentTab();

            // if (currentTab.options.Count == 0)
            //   throw new System.Exception("Tab should not be empty. Did you forget to add a tab selection option?");
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
        {
            VerticalSelect(0);
            Refresh();
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
        VerticalSelect(0);
    }

    #endregion
}

