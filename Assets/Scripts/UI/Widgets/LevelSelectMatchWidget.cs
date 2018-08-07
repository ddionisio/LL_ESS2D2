using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectMatchWidget : MonoBehaviour {
    public ClimateWidget climateWidget;
    public ClimateZoneWidget climateZoneWidget;

    void OnEnable() {
        //grab current info via GameData
        var climateMatch = GameData.instance.curLevelData.climateMatch;

        climateWidget.climate = climateMatch;
        climateZoneWidget.climateZone = climateMatch.zone;
    }
}
