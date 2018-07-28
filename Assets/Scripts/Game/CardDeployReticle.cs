using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardDeployReticle : MonoBehaviour {
    [Header("Display")]
    public SpriteRenderer iconSprite;
    
    public virtual void Show(CardData card) {
        gameObject.SetActive(true);

        if(iconSprite) iconSprite.sprite = card.icon;

    }

    public void Hide() {
        gameObject.SetActive(false);
    }
    
    protected virtual void Awake() {        
        gameObject.SetActive(false);
    }
}
