using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeployTargetDisplay : MonoBehaviour {
    public const string poolGroup = "cardDeployTarget";

    private M8.PoolDataController mPoolData;

    public static CardDeployTargetDisplay Spawn(GameObject prefab, Vector2 position) {
        var pool = M8.PoolController.CreatePool(poolGroup);
        if(!pool.IsFactoryTypeExists(prefab.name))
            pool.AddType(prefab, 8, 8);

        var spawn = pool.Spawn<CardDeployTargetDisplay>(prefab.name, "", null, null);

        spawn.transform.position = position;

        return spawn;
    }

    public void Release() {
        if(!mPoolData)
            mPoolData = GetComponent<M8.PoolDataController>();

        if(mPoolData)
            mPoolData.Release();
        else
            Destroy(gameObject);
    }
}
