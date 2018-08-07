using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClimateZoneWidget : MonoBehaviour {
    [Header("Data")]
    public string modalDesc = "description"; //modal to open for description

    [SerializeField]
    ClimateZoneData _climateZone;

    [Header("Display")]
    public Image image;
    public Text titleLabel;

    public ClimateZoneData climateZone {
        get { return _climateZone; }
        set {
            if(_climateZone != value) {
                _climateZone = value;
                ApplyCurrentClimateZone();
            }
        }
    }

    private M8.GenericParams mModalDescParms = new M8.GenericParams();

    public void OpenDescription() {
        if(!_climateZone)
            return;

        mModalDescParms[ModalInfo.parmImage] = _climateZone.image;
        mModalDescParms[ModalInfo.parmTitleTextRef] = _climateZone.titleTextRef;
        mModalDescParms[ModalInfo.parmDescTextRef] = _climateZone.descTextRef;

        M8.UIModal.Manager.instance.ModalOpen(modalDesc, mModalDescParms);
    }

    void Awake() {
        ApplyCurrentClimateZone();
    }

    private void ApplyCurrentClimateZone() {
        if(_climateZone) {
            if(image) image.sprite = _climateZone.image;
            if(titleLabel) titleLabel.text = M8.Localize.Get(_climateZone.titleTextRef);
        }
    }
}
