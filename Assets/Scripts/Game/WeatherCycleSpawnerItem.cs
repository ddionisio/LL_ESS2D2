using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherCycleSpawnerItem : MonoBehaviour {
    public enum SpawnType {
        Location,
        FlowerBudding, //spawn under budding flower, sets unitTarget in params
        FlowerBuddingUnmarked, //spawn under unmarked budding flower, sets unitTarget in params (will not spawn if all flowers are marked)

        TargetFlowerUnmarked, //target nearest flower unmarked relative to position, if no target found, skip spawn
        TargetFlowerBloomedUnmarked, //target nearest bloomed flower unmarked relative to position, if no target found, skip spawn

        FlowerRandom, //any active flower, also sets it as unitTarget
    }

    [Header("Edit")]
    public Color editColor = Color.white;

    [Header("Info")]
    public SpawnType type;
    public GameObject prefab;
    public GameObject showOnSpawnGO;
    public float delay; //delay relative to the last spawn, or at the start
    public bool forceDespawnOnCycleEnd; //if true, will always despawn on end of cycle (use for weather spawn items)

    [Header("Telemetry")]    
    public float dirAngle;

    public Vector2 dir {
        get {
            return M8.MathUtil.RotateAngle(Vector2.up, dirAngle);
        }
    }

    public Vector2 position {
        get {
            return transform.position;
        }
    }

    public Unit.DespawnCycleType cycleEndType { get; set; }

    public Unit Spawn(M8.PoolController pool) {
        if(showOnSpawnGO)
            showOnSpawnGO.SetActive(true);

        var parms = new M8.GenericParams();

        if(forceDespawnOnCycleEnd)
            parms[UnitSpawnParams.despawnCycleType] = Unit.DespawnCycleType.Cycle;
        else if(cycleEndType != Unit.DespawnCycleType.None)
            parms[UnitSpawnParams.despawnCycleType] = cycleEndType;

        switch(type) {
            case SpawnType.Location:
                parms[UnitSpawnParams.position] = position;
                parms[UnitSpawnParams.dir] = dir;
                break;

            case SpawnType.FlowerBudding:
            case SpawnType.FlowerBuddingUnmarked: {
                    var flowersQuery = GameController.instance.motherbase.GetFlowersBudding(type == SpawnType.FlowerBuddingUnmarked);
                    if(flowersQuery.Count == 0)
                        return null;

                    var flower = flowersQuery[Random.Range(0, flowersQuery.Count)];
                    parms[UnitSpawnParams.position] = flower.position;
                    parms[UnitSpawnParams.dir] = flower.up;
                    parms[UnitSpawnParams.unitTarget] = flower;
                }
                break;

            case SpawnType.TargetFlowerUnmarked: {
                    var flower = GameController.instance.motherbase.GetNearestFlower(position.x, true);
                    if(!flower)
                        return null;

                    parms[UnitSpawnParams.position] = position;
                    parms[UnitSpawnParams.dir] = dir;
                    parms[UnitSpawnParams.unitTarget] = flower;
                }
                break;

            case SpawnType.TargetFlowerBloomedUnmarked: {
                    var flower = GameController.instance.motherbase.GetNearestFlowerBloomed(position.x, true);
                    if(!flower)
                        return null;

                    parms[UnitSpawnParams.position] = position;
                    parms[UnitSpawnParams.dir] = dir;
                    parms[UnitSpawnParams.unitTarget] = flower;
                }
                break;

            case SpawnType.FlowerRandom: {
                    var flower = GameController.instance.motherbase.GetRandomFlower(false);
                    if(!flower)
                        return null;

                    parms[UnitSpawnParams.position] = flower.position;
                    parms[UnitSpawnParams.dir] = dir;
                    parms[UnitSpawnParams.unitTarget] = flower;
                }
                break;
        }

        return pool.Spawn<Unit>(prefab.name, "", null, parms);
    }

    void Awake() {
        if(showOnSpawnGO)
            showOnSpawnGO.SetActive(false);
    }

    void OnDrawGizmos() {
        const float radius = 0.33f;
        const float arrowLen = 1.0f;

        var pointColor = new Color(editColor.r, editColor.g, editColor.b, editColor.a * 0.75f);
        var arrowColor = editColor;

        Gizmos.color = pointColor;
        Gizmos.DrawSphere(position, radius);

        var end = position + dir * arrowLen;

        //display arrow for certain spawn types
        switch(type) {
            case SpawnType.Location:
            case SpawnType.FlowerRandom:
                Gizmos.color = arrowColor;
                M8.Gizmo.ArrowLine2D(position, end);
                break;
        }
    }
}
