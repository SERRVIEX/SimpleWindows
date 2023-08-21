using System.Collections.Generic;

using UnityEngine;

public static class RectTransformExtensions
{
    private struct AnchorPreset
    {
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;

        // Constructors

        public AnchorPreset(Vector2 anchorMin, Vector2 anchorMax)
        {
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
        }
    }

    private static Dictionary<Anchor, AnchorPreset> _anchorPresets = new Dictionary<Anchor, AnchorPreset>() 
    {
        { Anchor.TopLeft, new AnchorPreset(new Vector2(0, 1), new Vector2(0, 1)) },
        { Anchor.TopCenter, new AnchorPreset(new Vector2(.5f, 1),new Vector2(.5f, 1)) },
        { Anchor.TopRight, new AnchorPreset(new Vector2(1, 1),new Vector2(1, 1)) },

        { Anchor.MiddleLeft, new AnchorPreset(new Vector2(0, .5f),new Vector2(0, .5f)) },
        { Anchor.MiddleCenter, new AnchorPreset(new Vector2(.5f, .5f),new Vector2(.5f, .5f)) },
        { Anchor.MiddleRight, new AnchorPreset(new Vector2(1f, .5f),new Vector2(1f, .5f)) },

        { Anchor.BottomLeft, new AnchorPreset(new Vector2(0, 0),new Vector2(0, 0)) },
        { Anchor.BottomCenter, new AnchorPreset(new Vector2(0.5f, 0),new Vector2(0.5f, 0)) },
        { Anchor.BottomRight, new AnchorPreset(new Vector2(1, 0),new Vector2(1, 0)) },

        { Anchor.StretchTop, new AnchorPreset(new Vector2(0, 1),new Vector2(1, 1)) },
        { Anchor.StretchMiddle, new AnchorPreset(new Vector2(0, .5f),new Vector2(1, .5f)) },
        { Anchor.StretchBottom, new AnchorPreset(new Vector2(0, 0),new Vector2(1, 0)) },

        { Anchor.StretchLeft, new AnchorPreset(new Vector2(0, 0),new Vector2(0, 1)) },
        { Anchor.StretchCenter, new AnchorPreset(new Vector2(.5f, 0),new Vector2(.5f, 1)) },
        { Anchor.StretchRight, new AnchorPreset(new Vector2(1, 0), new Vector2(1, 1)) },

        { Anchor.Stretch, new AnchorPreset(new Vector2(0, 0), new Vector2(1, 1)) },
    };

    public static Dictionary<Pivot, Vector2> _pivotPresets = new Dictionary<Pivot, Vector2>()
    {
        { Pivot.TopLeft, new Vector2(0, 1) },
        { Pivot.TopCenter, new Vector2(.5f, 1) },
        { Pivot.TopRight,  new Vector2(1, 1) },

        { Pivot.MiddleLeft, new Vector2(0, .5f) },
        { Pivot.MiddleCenter,  new Vector2(.5f, .5f) },
        { Pivot.MiddleRight, new Vector2(1, .5f) },

        { Pivot.BottomLeft, new Vector2(0, 0) },
        { Pivot.BottomCenter, new Vector2(.5f, 0) },
        { Pivot.BottomRight, new Vector2(1, 0) },
    };

    public static void SetAnchor(this RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
    {
        Vector2 size = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        Vector3 position = rectTransform.position;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        rectTransform.SetSize(size);
        rectTransform.position = position;
    }

    public static void SetAnchor(this RectTransform rectTransform, Anchor anchor)
    {
        Vector2 size = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        Vector3 position = rectTransform.position;

        rectTransform.anchorMin = _anchorPresets[anchor].AnchorMin;
        rectTransform.anchorMax = _anchorPresets[anchor].AnchorMax;

        rectTransform.SetSize(size);
        rectTransform.position = position;
    }

    public static Anchor GetAnchor(RectTransform rectTransform)
    {
        foreach (var item in _anchorPresets)
            if(rectTransform.anchorMin == item.Value.AnchorMin && rectTransform.anchorMax == item.Value.AnchorMax)
                return item.Key;

        return Anchor.MiddleCenter;
    }

    public static bool IsAnchor(this RectTransform rectTransform, Anchor anchor) => GetAnchor(rectTransform) == anchor;

    public static void SetPivot(this RectTransform rectTransform, Vector2 pivot)
    {
        Vector2 size = rectTransform.rect.size;
        Vector2 deltaPivot = rectTransform.pivot - pivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
        rectTransform.pivot = pivot;
        rectTransform.localPosition -= deltaPosition;
    }

    public static void SetPivot(this RectTransform rectTransform, Pivot pivot)
    {
        Vector2 newPivot = _pivotPresets[pivot];
        Vector2 size = rectTransform.rect.size;
        Vector2 deltaPivot = rectTransform.pivot - newPivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
        rectTransform.pivot = newPivot;
        rectTransform.localPosition -= deltaPosition;
    }

    public static Pivot GetPivot(this RectTransform rectTransform)
    {
        foreach (var item in _pivotPresets)
            if(rectTransform.pivot == item.Value)
                return item.Key;

        return Pivot.MiddleCenter;
    }

    public static bool IsPivot(this RectTransform rectTransform, Pivot pivot) => GetPivot(rectTransform) == pivot;

    public static void SetWidth(this RectTransform rectTransform, float size) => rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);

    public static void SetHeight(this RectTransform rectTransform, float size) => rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);

    public static void SetSize(this RectTransform rectTransform, float width, float height)
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    public static Vector2 GetSize(this RectTransform rectTransform) => new Vector2(rectTransform.rect.width, rectTransform.rect.height);

    public static void SetSize(this RectTransform rectTransform, Vector2 size)
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    public static void SetSize(this RectTransform rectTransform, Vector3 size)
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    public static void SetLeft(this RectTransform rectTransform, float left) => rectTransform.offsetMin = new Vector2(left, rectTransform.offsetMin.y);
    
    public static void SetRight(this RectTransform rectTransform, float right) => rectTransform.offsetMax = new Vector2(-right, rectTransform.offsetMax.y);

    public static void SetTop(this RectTransform rectTransform, float top) => rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -top);

    public static void SetBottom(this RectTransform rectTransform, float bottom) => rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, bottom);

    public static void SetOffset(this RectTransform rectTransform, float left, float right, float top, float bottom)
    {
        rectTransform.offsetMin = new Vector2(left, rectTransform.offsetMin.y);
        rectTransform.offsetMax = new Vector2(-right, rectTransform.offsetMax.y);
        rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -top);
        rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, bottom);
    }

    public static RectOffset GetOffset(this RectTransform rectTransform)
    {
        return new RectOffset((int)rectTransform.offsetMin.x, (int)-rectTransform.offsetMax.x, (int)-rectTransform.offsetMax.y, (int)rectTransform.offsetMin.y);
    }

    public static bool Overlaps(this RectTransform source, RectTransform target)
    {
        Vector3 relativePos = source.InverseTransformPoint(target.transform.position);

        return !(relativePos.x < -(source.rect.width * source.pivot.x + target.rect.width * (1 - target.pivot.x)) ||
            relativePos.x > source.rect.width * (1 - source.pivot.x) + target.rect.width * target.pivot.x ||
            relativePos.y > source.rect.height * (1 - source.pivot.y) + target.rect.height * target.pivot.y ||
            relativePos.y < -(source.rect.height * source.pivot.y + target.rect.height * (1 - target.pivot.y)));
    }

    public static bool OverlapsAllCorners(this RectTransform source, RectTransform target)
    {
        Vector3 relativePos = source.InverseTransformPoint(target.transform.position);

        if (relativePos.x - target.rect.width * target.pivot.x < -source.rect.width / 2f) return false;
        if (relativePos.x + target.rect.width * (1 - target.pivot.x) > source.rect.width / 2f) return false;
        if (relativePos.y - target.rect.height * target.pivot.y < -source.rect.height / 2f) return false;
        if (relativePos.y + target.rect.height * (1 - target.pivot.y) > source.rect.height / 2f) return false;

        return true;
    }

    

    /// <summary>
    /// Get local position of each corner in local space.
    /// </summary>
    public static RectOffset GetLocalCorners(this RectTransform rectTransform)
    {
        float minX = rectTransform.localPosition.x - (rectTransform.rect.width * rectTransform.pivot.x);
        float maxX = rectTransform.localPosition.x + (rectTransform.rect.width * (1 - rectTransform.pivot.x));

        float minY = rectTransform.localPosition.y - (rectTransform.rect.height * (1 - rectTransform.pivot.y));
        float maxY = rectTransform.localPosition.y + (rectTransform.rect.height * rectTransform.pivot.y);

        RectOffset rectOffset = new RectOffset();
        rectOffset.left = (int)minX;
        rectOffset.right = (int)maxX;
        rectOffset.top = (int)maxY;
        rectOffset.bottom = (int)minY;

        return rectOffset;
    }

    public static RectOffset GetInsetsFromParentEdge(this RectTransform rectTransform)
    { 
        RectTransform parent = rectTransform.parent.GetComponent<RectTransform>();
        RectOffset localCorners = GetLocalCorners(rectTransform);

        RectOffset rectOffset = new RectOffset();
        rectOffset.left = (int)(localCorners.left + parent.rect.width / 2);
        rectOffset.right = (int)(parent.rect.width - (localCorners.right + parent.rect.width / 2));
        rectOffset.top = (int)(parent.rect.height - (localCorners.top + parent.rect.height / 2));
        rectOffset.bottom = (int)(localCorners.bottom + parent.rect.height / 2);

        return rectOffset;
    }

    /// <summary>
    /// Transform the bounds of the current rect transform to the space of another transform.
    /// </summary>
    public static Bounds TransformBoundsTo(this RectTransform source, Transform target)
    {
        Vector3[] corners = new Vector3[4];

        // Based on code in ScrollRect's internal GetBounds and InternalGetBounds methods.
        var bounds = new Bounds();
        if (source != null)
        {
            source.GetWorldCorners(corners);

            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var matrix = target.worldToLocalMatrix;
            for (int i = 0; i < 4; i++)
            {
                Vector3 v = matrix.MultiplyPoint3x4(corners[i]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
        }

        return bounds;
    }
}