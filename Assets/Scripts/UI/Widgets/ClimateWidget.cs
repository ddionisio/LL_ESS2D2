using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClimateWidget : MonoBehaviour {
    [Header("Data")]
    public string modalDesc = "description"; //modal to open for description

    [SerializeField]
    ClimateData _climate;

    [Header("Display")]
    public Image image;
    public Text titleLabel;

    private M8.GenericParams mModalDescParms = new M8.GenericParams();

    public ClimateData climate {
        get { return _climate; }
        set {
            if(_climate != value) {
                _climate = value;
                ApplyCurrentClimate();
            }
        }
    }

    public void OpenDescription() {
        if(!_climate)
            return;

        mModalDescParms[ModalInfo.parmImage] = _climate.image;
        mModalDescParms[ModalInfo.parmTitleTextRef] = _climate.titleTextRef;
        mModalDescParms[ModalInfo.parmDescTextRef] = _climate.descTextRef;

        M8.UIModal.Manager.instance.ModalOpen(modalDesc, mModalDescParms);
    }

    void Awake() {        
        ApplyCurrentClimate();
    }

    private void ApplyCurrentClimate() {
        if(_climate) {
            if(image) image.sprite = _climate.image;
            if(titleLabel) titleLabel.text = M8.Localize.Get(_climate.titleTextRef);
        }
    }
}
