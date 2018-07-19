using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : GameModeController<GameController> {
    [Header("Controls")]
    public CardDeck cardDeck;
    public WeatherCycle weatherCycle;
    public Motherbase motherbase;

    [Header("Level Info")]
    public GameBounds2D levelBounds;
    public LayerMask levelGroundLayerMask;
    
    public LevelGroundPosition GetGroundPosition(Vector2 pos) {
        return LevelGroundPosition.FromPointInBounds(levelBounds.rect, pos, levelGroundLayerMask);
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();


    }
}
