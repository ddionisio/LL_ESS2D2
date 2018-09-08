using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalWeatherInfo : M8.UIModal.Controller, M8.UIModal.Interface.IPush {

    private string mWeatherTitleRef;
    private string mWeatherDescRef;

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        if(aActive) {

        }
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {

    }
}
