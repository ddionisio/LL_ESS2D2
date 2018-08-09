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

    [Header("Signal")]
    public SignalCardWidget signalCardDragStart;
    public SignalCardWidget signalCardDragEnd;
    public SignalCardWidgetUnit signalCardSpawned;

    public CardData card { get { return mCardItem.card; } }

    private CardDeckController.CardItem mCardItem;

    private Graphic mBaseGraphic;

    private float mDeployBorderY;

    private CardDeployReticle mDeployReticle;
    private bool mIsDragging;

    public void Init(CardDeckController.CardItem cardItem, float deployBorderY) {
        Deinit(); //fail-safe

        mCardItem = cardItem;

        var card = mCardItem.card;

        cardImage.sprite = card.image;
        cardImage.SetNativeSize();

        cardIconImage.sprite = card.icon;
        cardIconImage.SetNativeSize();

        if(cardNameLabel) cardNameLabel.text = M8.Localize.Get(card.title);

        cardCountLabel.text = mCardItem.curCount.ToString();

        OnCardUpdateState();

        mDeployBorderY = deployBorderY;

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

            cardFill.gameObject.SetActive(false);
        }
    }

    private void OnCardUpdateCount() {
        cardCountLabel.text = mCardItem.curCount.ToString();
    }

    private void ApplyCurrentFill() {
        if(mCardItem.curCount > 0) {
            ApplyCardActive(true);

            cardFill.gameObject.SetActive(false);

            cardIconFill.fillAmount = mCardItem.cooldownScale;
        }
        else {
            ApplyCardActive(false);

            if(mCardItem.cooldownScale > 0f) {
                cardFill.gameObject.SetActive(true);
                cardFill.fillAmount = 1.0f - mCardItem.cooldownScale;
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

    private void StartDragging() {
        if(!mIsDragging) {
            mIsDragging = true;

            cardDrag.gameObject.SetActive(true);

            cardDrag.Init(mCardItem.card);
            cardDrag.SetShow(true);

            if(signalCardDragStart)
                signalCardDragStart.Invoke(this);
        }

        if(!mDeployReticle) {
            mDeployReticle = GameController.instance.GetCardDeployReticle(mCardItem.card.targetReticleName);
            if(mDeployReticle)
                mDeployReticle.Init(mCardItem.card);
        }
    }

    private void StopDragging() {
        if(mIsDragging) {
            mIsDragging = false;

            cardDrag.gameObject.SetActive(false);

            if(signalCardDragEnd)
                signalCardDragEnd.Invoke(this);
        }

        if(mDeployReticle) {
            mDeployReticle.Hide();
            mDeployReticle = null;
        }
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if(mCardItem.curState != CardState.Active || mCardItem.curCount <= 0)
            return;

        //apply info to cardDrag, activate
        StartDragging();

        //update position
        cardDrag.transform.position = eventData.position;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!mIsDragging)
            return;

        //update position, show reticle when above border
        var curPos = eventData.position;

        if(curPos.y < mDeployBorderY) {
            cardDrag.SetShow(true);

            //reticle hide
            if(mDeployReticle) mDeployReticle.Hide();

            cardDrag.transform.position = curPos;
        }
        else {
            cardDrag.SetShow(false);

            //reticle show
            if(mDeployReticle) {
                mDeployReticle.Show();
                mDeployReticle.UpdatePosition(curPos);
            }
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(mIsDragging) {
            var curPos = eventData.position;

            //check if deployable, then deploy
            if(curPos.y > mDeployBorderY && (mDeployReticle == null || mDeployReticle.canDeploy)) {
                Vector2 targetPoint;

                if(mDeployReticle)
                    targetPoint = mDeployReticle.targetPosition;
                else {
                    //convert to world space
                    var gameCam = M8.Camera2D.main;
                    targetPoint = gameCam.unityCamera.ScreenToWorldPoint(curPos);
                }

                var unit = mCardItem.SpawnUnit(targetPoint);

                //spawn via motherbase
                GameController.instance.motherbase.SpawnQueueUnit(unit, targetPoint);

                if(signalCardSpawned)
                    signalCardSpawned.Invoke(this, unit);
            }
        }

        StopDragging();
    }
}
