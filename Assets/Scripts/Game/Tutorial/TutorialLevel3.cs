using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevel3 : MonoBehaviour {
    [Header("Data")]
    [M8.TagSelector]
    public string cardDeckTag;
    [M8.TagSelector]
    public string dragGuideTag;

    public Transform dragToWindbreakerPoint;

    [Header("Cards")]
    public CardData cardCollector;
    public CardData cardWindbreaker;

    [Header("Unit Templates")]
    public GameObject collectorPrefab;
    public GameObject windPrefab;

    [Header("Signal")]
    public SignalCardWidget signalCardDragBegin;
    public SignalCardWidget signalCardDragEnd;
    public SignalCardWidgetUnit signalCardSpawned;
    public SignalUnit signalCycleSpawnerSpawned;
    public M8.SignalEntity signalUnitSpawnerSpawned;

    private CardDeckWidget mCardDeck;
    private DragToGuideWidget mDragGuide;

    private CardWidget mCardWidgetTarget;
    //private Unit mCardUnitSpawned;
    private Unit mCycleUnitSpawned;
    private M8.EntityBase mUnitSpawnerSpawned;

    private bool mIsWaitingCardTargetSpawn;

    private M8.GenericParams mCardDescParms = new M8.GenericParams();

    void OnDestroy() {
        if(signalCardDragBegin) signalCardDragBegin.callback -= OnCardDragBegin;
        if(signalCardDragEnd) signalCardDragEnd.callback -= OnCardDragEnd;
        if(signalCardSpawned) signalCardSpawned.callback -= OnCardDragSpawned;

        if(signalCycleSpawnerSpawned) signalCycleSpawnerSpawned.callback -= OnCycleSpawn;

        if(signalUnitSpawnerSpawned) signalUnitSpawnerSpawned.callback -= OnUnitSpawnerSpawned;

        if(GameController.isInstantiated) {
            GameController.instance.prepareCycleCallback -= OnPrepareCycle;
            GameController.instance.weatherCycle.weatherBeginCallback -= OnWeatherBegin;
        }
    }

    void Awake() {
        if(signalCardDragBegin) signalCardDragBegin.callback += OnCardDragBegin;
        if(signalCardDragEnd) signalCardDragEnd.callback += OnCardDragEnd;
        if(signalCardSpawned) signalCardSpawned.callback += OnCardDragSpawned;

        if(signalCycleSpawnerSpawned) signalCycleSpawnerSpawned.callback += OnCycleSpawn;

        if(signalUnitSpawnerSpawned) signalUnitSpawnerSpawned.callback += OnUnitSpawnerSpawned;

        GameController.instance.prepareCycleCallback += OnPrepareCycle;
        GameController.instance.weatherCycle.weatherBeginCallback += OnWeatherBegin;
    }

    void OnPrepareCycle() {
        if(!mCardDeck)
            mCardDeck = GameObject.FindGameObjectWithTag(cardDeckTag).GetComponent<CardDeckWidget>();

        if(!mDragGuide)
            mDragGuide = GameObject.FindGameObjectWithTag(dragGuideTag).GetComponent<DragToGuideWidget>();
    }

    void OnWeatherBegin() {
        var curCycleInd = GameController.instance.weatherCycle.curCycleIndex;
        var curWeatherInd = GameController.instance.weatherCycle.curWeatherIndex;

        StopAllCoroutines();

        mCardWidgetTarget = null;
        //mCardUnitSpawned = null;
        mCycleUnitSpawned = null;
        mUnitSpawnerSpawned = null;

        mIsWaitingCardTargetSpawn = false;

        if(curCycleInd == 0) {
            if(curWeatherInd == 0) {
                StartCoroutine(DoCollector());
            }
        }
        else if(curCycleInd == 1) {
            if(curWeatherInd == 0) {

            }
            else if(curWeatherInd == 1) {
                StartCoroutine(DoWindbreaker());
            }
        }
    }

    void OnCardDragBegin(CardWidget cardWidget) {

    }

    void OnCardDragEnd(CardWidget cardWidget) {

    }

    void OnCardDragSpawned(CardWidget cardWidget, Unit unit) {
        if(mIsWaitingCardTargetSpawn) {
            if(cardWidget == mCardWidgetTarget) {
                mIsWaitingCardTargetSpawn = false;
            }
        }

        //mCardUnitSpawned = unit;
    }

    void OnCycleSpawn(Unit unit) {
        mCycleUnitSpawned = unit;
    }

    void OnUnitSpawnerSpawned(M8.EntityBase ent) {
        mUnitSpawnerSpawned = ent;
    }

    IEnumerator DoCollector() {
        //wait for a collector to spawn
        do {
            yield return null;
        } while(!mUnitSpawnerSpawned || mUnitSpawnerSpawned.spawnType != collectorPrefab.name || mUnitSpawnerSpawned.state == UnitStates.instance.spawning);

        yield return new WaitForSeconds(0.5f); //wait a bit

        //show card modal
        const string modalCardDesc = "cardDescription";
        mCardDescParms[ModalCardDetail.parmCardRef] = cardCollector;
        M8.UIModal.Manager.instance.ModalOpen(modalCardDesc, mCardDescParms);

        while(M8.UIModal.Manager.instance.isBusy || M8.UIModal.Manager.instance.activeCount > 0)
            yield return null;
        //
    }

    IEnumerator DoWindbreaker() {
        do {
            yield return null;
        } while(!mCycleUnitSpawned || mCycleUnitSpawned.spawnType != windPrefab.name || mCycleUnitSpawned.state == UnitStates.instance.spawning);

        //show windbreaker card
        GameController.instance.cardDeck.ShowCard(cardWindbreaker.name);

        yield return new WaitForSeconds(0.3f); //wait for card animation

        //show card modal
        const string modalCardDesc = "cardDescription";
        mCardDescParms[ModalCardDetail.parmCardRef] = cardWindbreaker;
        M8.UIModal.Manager.instance.ModalOpen(modalCardDesc, mCardDescParms);

        while(M8.UIModal.Manager.instance.isBusy || M8.UIModal.Manager.instance.activeCount > 0)
            yield return null;
        //
        
        //show drag display and wait for player to deploy
        mCardWidgetTarget = mCardDeck.GetCardWidget(cardWindbreaker);

        mIsWaitingCardTargetSpawn = true;

        const string modalDragInstruction = "dragInstructions";
        M8.UIModal.Manager.instance.ModalOpen(modalDragInstruction);

        //set drag destination towards point
        var gameCam = M8.Camera2D.main.unityCamera;

        Vector2 destScreenPos = gameCam.WorldToScreenPoint(dragToWindbreakerPoint.position);
        //

        mDragGuide.Show(true, mCardWidgetTarget.transform.position, destScreenPos);
        //

        //wait for unit to be spawned
        while(mIsWaitingCardTargetSpawn)
            yield return null;

        M8.UIModal.Manager.instance.ModalCloseUpTo(modalDragInstruction, true);
        mDragGuide.Hide();
    }
}
