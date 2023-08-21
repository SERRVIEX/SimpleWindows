using UnityEngine;
using UnityEngine.UI;

public static class BoundsExtensions
{
    public static Rect BoundsToRect(this Bounds bounds, CanvasScaler canvasScaler, Camera camera)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector2[] extentPoints = new Vector2[8]
        {
            camera.WorldToScreenPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z)),
            camera.WorldToScreenPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z)),
            camera.WorldToScreenPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z)),
            camera.WorldToScreenPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z)),
            camera.WorldToScreenPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z)),
            camera.WorldToScreenPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z)),
            camera.WorldToScreenPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z)),
            camera.WorldToScreenPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z))
        };

        Vector2 min = extentPoints[0];
        Vector2 max = extentPoints[0];

        // Find the bottom left and top right coordinates.
        foreach (Vector2 item in extentPoints)
        {
            min = new Vector2(Mathf.Min(min.x, item.x), Mathf.Min(min.y, item.y));
            max = new Vector2(Mathf.Max(max.x, item.x), Mathf.Max(max.y, item.y));
        }

        Vector2 half = new Vector2(Screen.width / 2f, Screen.height / 2f);
        min -= half;
        max -= half;

        float aspectRatio = canvasScaler.referenceResolution.x / Screen.width;
        min *= aspectRatio;
        max *= aspectRatio;

        float width = max.x - min.x;
        float height = max.y - min.y;

        return new Rect(min.x + width / 2, min.y + height / 2, width, height);
    }
}