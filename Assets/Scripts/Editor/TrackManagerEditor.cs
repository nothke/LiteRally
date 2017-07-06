using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrackManager))]
public class TrackManagerEditor : Editor
{
    TrackManager tm;

    private void OnEnable()
    {
        tm = target as TrackManager;

        layoutNames = tm.GetLayoutNames();
    }

    int selectedTrack = 0;

    public string[] layoutNames;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("LOAD:", EditorStyles.boldLabel);

        selectedTrack = EditorGUILayout.Popup("All Layouts:", selectedTrack, layoutNames);

        if (GUILayout.Button("Load This Track from File"))
            tm.DeserializeTrack(tm.AllLayouts[selectedTrack].trackName, tm.AllLayouts[selectedTrack].layoutName);

        if (GUILayout.Button("Create Track in Scene"))
            tm.CreateTrack();

        if (GUILayout.Button("Refresh List"))
            layoutNames = tm.GetLayoutNames();

        EditorGUILayout.LabelField("SAVE:", EditorStyles.boldLabel);

        if (GUILayout.Button("Convert Scene to Track"))
            tm.SerializeSceneTrack();

        if (GUILayout.Button("Save Track to File"))
            tm.SerializeToFile();

        EditorGUILayout.LabelField("Cleanup:", EditorStyles.boldLabel);

        if (GUILayout.Button("Cleanup Scene"))
            tm.CleanupScene();

        if (GUILayout.Button("Cleanup Track"))
            tm.ClearTrack();
    }
}
