using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalWeatherInfo : M8.UIModal.Controller, M8.UIModal.Interface.IPush {
    public const string parmWeatherData = "dat";

    [Header("Display")]
    public Image image;
    public Text titleLabel;
    public Text descLabel;
    public Text temperatureLabel;
    public Text humidityLabel;
    public Text windSpeedLabel;
    public Text precipitationLabel;

    private string mWeatherTitleRef;
    private string mWeatherDescRef;

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        if(aActive) {
            LoLManager.instance.SpeakTextQueue(mWeatherTitleRef, mWeatherTitleRef, 0);
            LoLManager.instance.SpeakTextQueue(mWeatherDescRef, mWeatherTitleRef, 1);
        }
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {

    }
}
