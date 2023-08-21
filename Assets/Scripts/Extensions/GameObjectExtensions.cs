using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
    public static void SetLayerRecursively(this GameObject gameObject, int layer)
    {
        if (null == gameObject)
            return;

        gameObject.layer = layer;

        foreach (Transform child in gameObject.transform)
        {
            if (child == null)
                continue;

            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public static Bounds GetBoundsInChildren(this GameObject gameObject)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds();
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i].enabled)
                bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }
}
