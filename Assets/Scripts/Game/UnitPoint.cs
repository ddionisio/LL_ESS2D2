using UnityEngine;

public struct UnitPoint {
    public Vector2 position;
    public Vector2 up;

    public static bool GetGroundPoint(Vector2 position, out UnitPoint point) {
        var levelRect = GameController.instance.levelBounds.rect;

        var checkPoint = new Vector2(position.x, levelRect.yMax);
        var checkDir = Vector2.down;

        var hit = Physics2D.Raycast(checkPoint, checkDir, levelRect.height, GameData.instance.groundLayerMask);
        if(hit.collider) {
            point = new UnitPoint() { position = hit.point, up = hit.normal };

            return true;
        }
        else {
            point = new UnitPoint() { position = position, up = Vector2.up };

            return false;
        }
    }
}
