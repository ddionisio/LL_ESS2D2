using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnitAllyFlower))]
public class UnitAllyFlowerInspector : Editor {
    public const string growthModID = "debug";

    private float mGrowthMod;

    void OnEnable() {
        if(Application.isPlaying) {
            var dat = target as UnitAllyFlower;
            mGrowthMod = dat.GetGrowthMod(growthModID);
        }
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        M8.EditorExt.Utility.DrawSeparator();

        var dat = target as UnitAllyFlower;

        var lastEnabled = GUI.enabled;
        GUI.enabled = Application.isPlaying;

        GUILayout.BeginHorizontal();

        mGrowthMod = EditorGUILayout.FloatField("Growth Mod", mGrowthMod);

        if(GUILayout.Button("Apply", GUILayout.Width(50f))) {
            dat.ApplyGrowthMod(growthModID, mGrowthMod);
        }

        GUILayout.EndHorizontal();

        GUI.enabled = lastEnabled;
    }
}
