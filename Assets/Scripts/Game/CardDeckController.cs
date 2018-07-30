using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeckController : MonoBehaviour {
    public const string poolGroupRef = "cardUnitPool";

    public CardDeckData deckData;
    
    public class CardItem {
        public CardData card { get; private set; }

        public event System.Action stateChangeCallback;
        public event System.Action countUpdateCallback;

        public CardState curState {
            get {
                return mCurState;
            }

            set {
                if(mCurState != value) {
                    mCurState = value;

                    if(stateChangeCallback != null)
                        stateChangeCallback();
                }
            }
        }

        public int curCount {
            get { return mCurCount; }
            set {
                int newVal = value >= 0 ? value : 0;

                if(mCurCount != newVal) {
                    mCurCount = newVal;

                    mCurCooldown = 0f; //visual purpose

                    if(countUpdateCallback != null)
                        countUpdateCallback();
                }
            }
        }

        /// <summary>
        /// [0, 1], starts at 1
        /// </summary>
        public float cooldownScale {
            get {
                return mCurCooldown / card.cooldownDuration;
            }
        }

        public CardItem(M8.PoolController unitPool, CardDeckData.Item cardInfo) {
            card = cardInfo.card;
            mCurState = cardInfo.startState;
            mCurCount = cardInfo.startCount;
            mCurCooldown = card.cooldownDuration;
            mPendingCount = 0;

            mPool = unitPool;

            mPool.AddType(card.unitPrefab, mCurCount, mCurCount);
        }

        public Unit SpawnUnit(Vector2 targetPos) {
            if(curCount <= 0) {
                Debug.LogWarning("Trying to spawn unit: " + card.unitPrefab.name + " but there is no count.");
                return null;
            }

            curCount--;

            var parms = new M8.GenericParams();
            parms[UnitSpawnParams.target] = targetPos;
            parms[UnitSpawnParams.card] = this;

            return mPool.Spawn<Unit>(card.unitPrefab.name, "", null, parms);
        }

        public void IncrementPendingCount() {
            mPendingCount++;
            mCurCooldown = 0f;
        }
                
        public void Update(float deltaTime) {
            if(mPendingCount > 0) {
                mCurCooldown = Mathf.Clamp(mCurCooldown + deltaTime, 0f, card.cooldownDuration);
                if(mCurCooldown == card.cooldownDuration) {
                    mPendingCount--;
                    curCount++;
                }
            }
        }

        private CardState mCurState;
        private int mCurCount;
        private float mCurCooldown;

        private int mPendingCount;

        private M8.PoolController mPool;
    }

    public CardItem[] cards { get; private set; }

    private M8.PoolController mUnitPool;

    public CardItem GetCardItem(Unit unit) {
        for(int i = 0; i < cards.Length; i++) {
            string templateName = unit.spawnType;
            if(cards[i].card.unitPrefab.name == templateName)
                return cards[i];
        }

        return null;
    }

    void OnDestroy() {
        if(mUnitPool)
            mUnitPool.ReleaseAll();
    }

    void Awake() {
        mUnitPool = M8.PoolController.CreatePool(poolGroupRef);

        cards = new CardItem[deckData.cards.Length];

        for(int i = 0; i < cards.Length; i++) {
            cards[i] = new CardItem(mUnitPool, deckData.cards[i]);
        }
    }

    void Update() {
        for(int i = 0; i < cards.Length; i++)
            cards[i].Update(Time.deltaTime);
    }
}
