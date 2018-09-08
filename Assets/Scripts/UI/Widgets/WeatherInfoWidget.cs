using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeatherInfoWidget : MonoBehaviour {
    [Header("Data")]
    public string modalDescription = "weatherDescription";

    [Header("Display")]
    public Image image;
    public Text nameLabel;
    public Text temperatureLabel;
    public Text precipitationLabel;

    public string titleTextRef { get { return mTitleTextRef; } }

    private Sprite mImageSpr;
    private string mTitleTextRef;
    private string mDetailTextRef;

    private LoLSpeakTextClick mNameSpeakText;

    private M8.GenericParams mDescParms = new M8.GenericParams();

    public void OpenDescription() {
        mDescParms[ModalInfo.parmImage] = mImageSpr;
        mDescParms[ModalInfo.parmTitleTextRef] = mTitleTextRef;
        mDescParms[ModalInfo.parmDescTextRef] = mDetailTextRef;

        M8.UIModal.Manager.instance.ModalOpen(modalDescription, mDescParms);
    }

    public void Apply(WeatherData weather) {
        mImageSpr = weather.type.image;
        mTitleTextRef = weather.type.titleRef;
        mDetailTextRef = weather.type.detailRef;

        if(image) {
            image.sprite = mImageSpr;
            image.SetNativeSize();
        }

        if(nameLabel) nameLabel.text = M8.Localize.Get(mTitleTextRef);
        if(temperatureLabel) temperatureLabel.text = weather.temperatureText;
        if(precipitationLabel) precipitationLabel.text = weather.precipitationPercentText;

        if(mNameSpeakText) mNameSpeakText.key = mTitleTextRef;
    }

    void Awake() {
        if(nameLabel)
            mNameSpeakText = nameLabel.GetComponent<LoLSpeakTextClick>();
    }
}
