using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeck : MonoBehaviour {
    public const string poolGroupRef = "cardUnitPool";

    public enum CardState {
        Active,
        Disabled,
        Hidden
    }

    [System.Serializable]
    public class CardItem {
        public CardData card;

        public CardState startState;
        public int startCount;
        public int maxCount;

        public event System.Action countUpdateCallback;

        public CardState curState {
            get {
                //if current count is 0, disabled
                if(mCurCount <= 0)
                    return CardState.Disabled;

                return mCurState;
            }

            set {
                if(mCurState != value) {
                    mCurState = value;
                }
            }
        }

        public int curCount {
            get { return mCurCount; }
            set {
                int newVal = Mathf.Clamp(value, 0, maxCount);

                if(mCurCount != newVal) {
                    mCurCount = newVal;

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

        public Unit SpawnUnit(M8.GenericParams parms) {
            return mPool.Spawn<Unit>(card.unitPrefab.name, "", null, parms);
        }

        public void Init(M8.PoolController unitPool) {
            mCurState = startState;
            mCurCount = startCount;
            mCurCooldown = card.cooldownDuration;

            mPool = unitPool;

            mPool.AddType(card.unitPrefab, maxCount, maxCount);
        }

        public void Update(float deltaTime) {
            if(mCurCooldown < card.cooldownDuration) {
                mCurCooldown = Mathf.Clamp(mCurCooldown + deltaTime, 0f, card.cooldownDuration);
            }
        }

        private CardState mCurState;
        private int mCurCount;
        private float mCurCooldown;

        private M8.PoolController mPool;
    }

    public CardItem[] cards;

    private M8.PoolController mUnitPool;

    void OnDestroy() {
        if(mUnitPool)
            mUnitPool.ReleaseAll();
    }

    void Awake() {
        mUnitPool = M8.PoolController.CreatePool(poolGroupRef);

        for(int i = 0; i < cards.Length; i++) {
            cards[i].Init(mUnitPool);
        }
    }

    void Update() {
        for(int i = 0; i < cards.Length; i++)
            cards[i].Update(Time.deltaTime);
    }
}
