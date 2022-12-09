using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevel4 : MonoBehaviour {
    [Header("Data")]
    [M8.TagSelector]
    public string cardDeckTag;
    [M8.TagSelector]
    public string dragGuideTag;

    [Header("Cards")]
    public CardData cardSunfly;

    [Header("Unit Templates")]
    public GameObject frostbitePrefab;

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
                StartCoroutine(DoSunfly());
            }
            else if(curWeatherInd == 1) {

            }
            else if(curWeatherInd == 2) {
                GameController.instance.cardDeck.ShowCard(3);
            }
        }
    }

    IEnumerator DoSunfly() {
        //wait for frostbite to spawn
        while(!mCycleUnitSpawned || mCycleUnitSpawned.spawnType != frostbitePrefab.name)
            yield return null;

        var frostbite = mCycleUnitSpawned;
        var frostbiteColl = frostbite.GetComponent<Collider2D>(); //trust that its collider's offset is reasonably at mid-air

        yield return new WaitForSeconds(0.5f); //wait a bit

        //show sunfly card
        GameController.instance.cardDeck.ShowCard(cardSunfly.name);

        yield return new WaitForSeconds(0.3f); //wait for card animation

        //show card modal
        const string modalCardDesc = "cardDescription";
        mCardDescParms[ModalCardDetail.parmCardRef] = cardSunfly;
        M8.UIModal.Manager.instance.ModalOpen(modalCardDesc, mCardDescParms);

        while(M8.UIModal.Manager.instance.isBusy || M8.UIModal.Manager.instance.activeCount > 0)
            yield return null;
        //

        //show drag display and wait for player to deploy
        mCardWidgetTarget = mCardDeck.GetCardWidget(cardSunfly);

        mIsWaitingCardTargetSpawn = true;

        const string modalDragInstruction = "dragInstructions";
        M8.UIModal.Manager.instance.ModalOpen(modalDragInstruction);

        //set drag destination towards weed
        var gameCam = M8.Camera2D.main.unityCamera;

        Vector2 frostbiteScreenPos = gameCam.WorldToScreenPoint(frostbite.transform.TransformPoint(frostbiteColl.offset));
        //

        mDragGuide.Show(true, mCardWidgetTarget.transform.position, frostbiteScreenPos);
        //

        //wait for unit to be spawned
        while(mIsWaitingCardTargetSpawn)
            yield return null;

        M8.UIModal.Manager.instance.ModalCloseUpTo(modalDragInstruction, true);
        mDragGuide.Hide();

        //show the other cards
        GameController.instance.cardDeck.ShowCard(1);
        GameController.instance.cardDeck.ShowCard(2);        
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
