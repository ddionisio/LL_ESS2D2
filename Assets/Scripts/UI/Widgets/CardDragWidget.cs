using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDragWidget : MonoBehaviour {
    public GameObject cardBaseGO;

    [Header("Display")]    
    public Image cardImage;
    public Image cardIconImage;

    public Text cardNameLabel;

    public void Init(CardData card) {
        cardImage.sprite = card.image;
        cardImage.SetNativeSize();

        cardIconImage.sprite = card.icon;
        cardIconImage.SetNativeSize();

        if(cardNameLabel) cardNameLabel.text = M8.Localize.Get(card.title);
    }

    public void SetShow(bool show) {
        if(cardBaseGO.activeSelf != show) {
            cardBaseGO.SetActive(show);

            //animation
        }
    }
}
