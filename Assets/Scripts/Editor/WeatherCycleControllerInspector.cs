using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeatherCycleController))]
public class WeatherCycleControllerInspector : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        M8.EditorExt.Utility.DrawSeparator();
                
        var dat = target as WeatherCycleController;

        var lastEnabled = GUI.enabled;
        GUI.enabled = Application.isPlaying && dat.isCycleRunning;

        if(GUILayout.Button("Force End Cycle")) {
            dat.ForceCompleteCycle();
        }

        GUI.enabled = lastEnabled;
    }
}
