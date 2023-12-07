using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Vector3 GetRandomSpawnPoint(Transform target)
    {
        var pos = UnityEngine.Random.insideUnitSphere * 5 + target.position;
        pos.y = 1f;

        return pos;
    }

    public static void SetRenderLayerChildren(Transform transform, int layerNumber)
    {
        foreach (Transform trans in transform.GetComponentInChildren<Transform>(true))
        {
            trans.gameObject.layer = layerNumber;
        }
    }
}
