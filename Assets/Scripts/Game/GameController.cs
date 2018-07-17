using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : GameModeController<GameController> {
    [Header("Data")]
    public CardDeck cardDeck;

    protected override void OnInstanceInit() {
        base.OnInstanceInit();


    }
}
