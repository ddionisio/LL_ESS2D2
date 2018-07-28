using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeckWidget : MonoBehaviour {
    public GameObject cardTemplate;
    public int cardCount;
    public RectTransform cardGroupRoot;
    public CardDragWidget cardDrag;

    private CardWidget[] mCards;

    private float mDeployBorderY;

    void OnEnable() {
        var cardDeck = GameController.instance.cardDeck;

        int deckCardCount = cardDeck.cards.Length;

        for(int i = 0; i < deckCardCount; i++) {
            if(cardDeck.cards[i].curState != CardState.Hidden)
                cardDeck.cards[i].curState = CardState.Disabled; //enable when cycle begins

            mCards[i].gameObject.SetActive(true);

            mCards[i].Init(cardDeck.cards[i], mDeployBorderY);
        }

        for(int i = deckCardCount; i < mCards.Length; i++) {
            mCards[i].gameObject.SetActive(false);
        }

        cardDrag.gameObject.SetActive(false);

        GameController.instance.weatherCycle.cycleBeginCallback += OnCycleBegin;
        GameController.instance.weatherCycle.cycleEndCallback += OnCycleEnd;
    }

    void OnDisable() {
        if(GameController.isInstantiated && GameController.instance.weatherCycle) {
            GameController.instance.weatherCycle.cycleBeginCallback -= OnCycleBegin;
            GameController.instance.weatherCycle.cycleEndCallback -= OnCycleEnd;
        }
    }

    void Awake() {
        //initialize cards
        mCards = new CardWidget[cardCount];

        for(int i = 0; i < cardCount; i++) {
            var newGO = Instantiate(cardTemplate);
            newGO.transform.SetParent(cardGroupRoot);

            mCards[i] = newGO.GetComponent<CardWidget>();
        }

        cardTemplate.SetActive(false);

        var max = transform.localToWorldMatrix.MultiplyPoint3x4(cardGroupRoot.rect.max);

        mDeployBorderY = max.y;
    }

    void OnCycleBegin() {
        var cardDeck = GameController.instance.cardDeck;
        for(int i = 0; i < cardDeck.cards.Length; i++) {
            if(cardDeck.cards[i].curState != CardState.Hidden)
                cardDeck.cards[i].curState = CardState.Active;
        }
    }

    void OnCycleEnd() {
        var cardDeck = GameController.instance.cardDeck;
        for(int i = 0; i < cardDeck.cards.Length; i++) {
            if(cardDeck.cards[i].curState != CardState.Hidden)
                cardDeck.cards[i].curState = CardState.Disabled;
        }
    }
}
