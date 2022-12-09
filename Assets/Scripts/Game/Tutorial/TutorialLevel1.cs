using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevel1 : MonoBehaviour {
    [Header("Data")]
    [M8.TagSelector]
    public string cardDeckTag;
    [M8.TagSelector]
    public string dragGuideTag;

    [Header("Cards")]
    public CardData cardGardener;
    public CardData cardMallet;
    public CardData cardSpearman;

    [Header("Unit Templates")]
    public GameObject weedPrefab;
    public GameObject molePrefab;
    public GameObject insectPrefab;
    
    [Header("Signal")]
    public SignalCardWidget signalCardDragBegin;
    public SignalCardWidget signalCardDragEnd;
    public SignalCardWidgetUnit signalCardSpawned;
    public SignalUnit signalCycleSpawnerSpawned;

    private CardDeckWidget mCardDeck;
    private DragToGuideWidget mDragGuide;

    private CardWidget mCardWidgetTarget;
    //private Unit mCardUnitSpawned;
    private Unit mCycleUnitSpawned;

    private bool mIsWaitingCardTargetSpawn;

    private M8.GenericParams mCardDescParms = new M8.GenericParams();

    void OnDestroy() {
        if(signalCardDragBegin) signalCardDragBegin.callback -= OnCardDragBegin;
        if(signalCardDragEnd) signalCardDragEnd.callback -= OnCardDragEnd;
        if(signalCardSpawned) signalCardSpawned.callback -= OnCardDragSpawned;

        if(signalCycleSpawnerSpawned) signalCycleSpawnerSpawned.callback -= OnCycleSpawn;

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

        mIsWaitingCardTargetSpawn = false;

        if(curCycleInd == 0) {
            if(curWeatherInd == 0) {
                StartCoroutine(DoGardener());
            }
            else if(curWeatherInd == 1) {
                StartCoroutine(DoMallet());
            }
        }
        else if(curCycleInd == 1) {
            if(curWeatherInd == 1) {
                StartCoroutine(DoSpearman());
            }
        }
    }

    IEnumerator DoGardener() {
        //wait for weed to spawn
        while(!mCycleUnitSpawned || mCycleUnitSpawned.spawnType != weedPrefab.name)
            yield return null;

        var weedUnit = mCycleUnitSpawned;

        yield return new WaitForSeconds(0.5f); //wait a bit

        //show gardener card
        GameController.instance.cardDeck.ShowCard(cardGardener.name);

        yield return new WaitForSeconds(0.3f); //wait for card animation

        //show card modal
        const string modalCardDesc = "cardDescription";
        mCardDescParms[ModalCardDetail.parmCardRef] = cardGardener;
        M8.UIModal.Manager.instance.ModalOpen(modalCardDesc, mCardDescParms);

        while(M8.UIModal.Manager.instance.isBusy || M8.UIModal.Manager.instance.activeCount > 0)
            yield return null;
        //

        //show drag display and wait for player to deploy
        mCardWidgetTarget = mCardDeck.GetCardWidget(cardGardener);

        mIsWaitingCardTargetSpawn = true;
        
        const string modalDragInstruction = "dragInstructions";
        M8.UIModal.Manager.instance.ModalOpen(modalDragInstruction);
                
        //set drag destination towards weed
        var gameCam = M8.Camera2D.main.unityCamera;

        Vector2 weedScreenPos = gameCam.WorldToScreenPoint(weedUnit.transform.position);
        //

        mDragGuide.Show(true, mCardWidgetTarget.transform.position, weedScreenPos);
        //

        //wait for unit to be spawned
        while(mIsWaitingCardTargetSpawn)
            yield return null;

        M8.UIModal.Manager.instance.ModalCloseUpTo(modalDragInstruction, true);
        mDragGuide.Hide();
    }

    IEnumerator DoMallet() {
        while(!mCycleUnitSpawned || mCycleUnitSpawned.spawnType != molePrefab.name)
            yield return null;

        //var moleUnit = mCycleUnitSpawned;

        yield return new WaitForSeconds(1.5f); //wait a bit

        //show mallet card
        GameController.instance.cardDeck.ShowCard(cardMallet.name);

        yield return new WaitForSeconds(0.3f); //wait for card animation

        //show card modal
        const string modalCardDesc = "cardDescription";
        mCardDescParms[ModalCardDetail.parmCardRef] = cardMallet;
        M8.UIModal.Manager.instance.ModalOpen(modalCardDesc, mCardDescParms);

        //while(M8.UIModal.Manager.instance.isBusy || M8.UIModal.Manager.instance.activeCount > 0)
            //yield return null;
        //
    }

    IEnumerator DoSpearman() {
        while(!mCycleUnitSpawned || mCycleUnitSpawned.spawnType != insectPrefab.name)
            yield return null;

        //var moleUnit = mCycleUnitSpawned;

        yield return new WaitForSeconds(1f); //wait a bit

        //show mallet card
        GameController.instance.cardDeck.ShowCard(cardSpearman.name);

        yield return new WaitForSeconds(0.3f); //wait for card animation

        //show card modal
        const string modalCardDesc = "cardDescription";
        mCardDescParms[ModalCardDetail.parmCardRef] = cardSpearman;
        M8.UIModal.Manager.instance.ModalOpen(modalCardDesc, mCardDescParms);

        //while(M8.UIModal.Manager.instance.isBusy || M8.UIModal.Manager.instance.activeCount > 0)
        //yield return null;
        //
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
}
