using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardState {
    Active,
    Disabled,
    Hidden
}

[CreateAssetMenu(fileName = "cardDeck", menuName = "Game/Card Deck")]
public class CardDeckData : ScriptableObject {
    [System.Serializable]
    public class Item {
        public CardData card;

        public bool startHidden;
        public int startCount;
    }

    public Item[] cards;
}
