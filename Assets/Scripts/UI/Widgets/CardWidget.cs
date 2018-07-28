using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [Header("Display")]
    public Image cardImage;
    public Image cardFill;
    
    public Image cardIconImage;
    public Image cardIconFill;
    
    public Text cardCountLabel;
    public Text cardNameLabel;

    public M8.UI.Graphics.ColorGroup colorGroup;
    public Color disableColor = new Color(0.75f, 0.75f, 0.75f, 0.75f);

    public CardDragWidget cardDrag;

    private CardDeckController.CardItem mCardItem;

    private Graphic mBaseGraphic;

    private bool mIsDragging;

    public void Init(CardDeckController.CardItem cardItem) {
        Deinit(); //fail-safe

        mCardItem = cardItem;

        var card = mCardItem.card;

        cardImage.sprite = card.image;
        cardImage.SetNativeSize();

        cardIconImage.sprite = card.icon;
        cardIconImage.SetNativeSize();

        if(cardNameLabel) cardNameLabel.text = M8.Localize.Get(card.title);

        cardCountLabel.text = mCardItem.curCount.ToString();

        ApplyCurrentFill();

        mCardItem.stateChangeCallback += OnCardUpdateState;
        mCardItem.countUpdateCallback += OnCardUpdateCount;

        gameObject.SetActive(mCardItem.curState != CardState.Hidden);
    }

    public void Deinit() {
        if(mCardItem != null) {
            mCardItem.stateChangeCallback -= OnCardUpdateState;
            mCardItem.countUpdateCallback -= OnCardUpdateCount;

            mCardItem = null;
        }

        if(colorGroup)
            colorGroup.Revert();

        StopDragging();
    }

    void OnApplicationFocus(bool focus) {
        if(!focus)
            StopDragging();
    }

    void OnDestroy() {
        Deinit();
    }

    void Awake() {
        mBaseGraphic = GetComponent<Graphic>();

        colorGroup.Init();
    }

    void Update() {
        if(mCardItem != null && mCardItem.curState == CardState.Active) {
            ApplyCurrentFill();
        }
    }

    private void OnCardUpdateState() {
        gameObject.SetActive(mCardItem.curState != CardState.Hidden);

        if(mCardItem.curState == CardState.Active) {
            ApplyCurrentFill();
        }
        else if(mCardItem.curState == CardState.Disabled) {
            ApplyCardActive(false);

            cardFill.gameObject.SetActive(true);
            cardFill.fillAmount = 1.0f;
        }
    }

    private void OnCardUpdateCount() {
        cardCountLabel.text = mCardItem.curCount.ToString();
    }

    private void ApplyCurrentFill() {
        if(mCardItem.curCount > 0) {
            ApplyCardActive(true);

            cardFill.gameObject.SetActive(false);

            cardIconFill.fillAmount = 1.0f - mCardItem.cooldownScale;
        }
        else {
            ApplyCardActive(false);

            if(mCardItem.cooldownScale > 0f) {
                cardFill.gameObject.SetActive(true);
                cardFill.fillAmount = mCardItem.cooldownScale;
            }
            else
                cardFill.gameObject.SetActive(false);

            cardIconFill.fillAmount = 0f;
        }
    }

    private void ApplyCardActive(bool active) {
        if(mBaseGraphic.raycastTarget != active) {
            mBaseGraphic.raycastTarget = active;

            if(active)
                colorGroup.Revert();
            else {
                colorGroup.ApplyColor(disableColor);

                StopDragging();
            }
        }
    }

    private void StopDragging() {
        if(mIsDragging) {
            mIsDragging = false;

            cardDrag.gameObject.SetActive(false);            
        }
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        //apply info to cardDrag, activate
        mIsDragging = true;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;

        //update position, show reticle when above border
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        StopDragging();
    }
}
