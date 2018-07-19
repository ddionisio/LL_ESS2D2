using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LevelGroundPosition {
    public Vector2 position;
    public Vector2 normal;

    public static LevelGroundPosition FromPointInBounds(Rect bounds, Vector2 pos, LayerMask layerMask) {
        Vector2 sPos = new Vector2(pos.x, bounds.yMax);
        Vector2 dir = Vector2.down;

        var hit = Physics2D.Raycast(sPos, dir, bounds.height, layerMask);

        return hit.collider ? new LevelGroundPosition() { position=hit.point, normal=hit.normal } : new LevelGroundPosition() { position=pos, normal=Vector2.up };
    }
}
