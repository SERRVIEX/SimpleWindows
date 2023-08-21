using UnityEngine;
using UnityEngine.UI;

public static class ScrollRectExtensions
{
    /// <summary>
    /// Normalize a distance to be used in verticalNormalizedPosition or horizontalNormalizedPosition.
    /// </summary>
    /// <param name="axis">Scroll axis, 0 = horizontal, 1 = vertical</param>
    /// <param name="distance">The distance in the scroll rect's view's coordiante space</param>
    /// <returns>The normalized scoll distance</returns>
    public static float NormalizeScrollDistance(this ScrollRect scrollRect, int axis, float distance)
    {
        // Based on code in ScrollRect's internal SetNormalizedPosition method.
        var viewport = scrollRect.viewport;
        var viewRect = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
        var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

        var content = scrollRect.content;
        var contentBounds = content != null ? content.TransformBoundsTo(viewRect) : new Bounds();

        var hiddenLength = contentBounds.size[axis] - viewBounds.size[axis];
        return distance / hiddenLength;
    }

    /// <summary>
    /// Scroll the target element to the vertical center of the scroll rect's viewport.
    /// Assumes the target element is part of the scroll rect's contents.
    /// </summary>
    public static void ScrollToCenter(this ScrollRect scrollRect, RectTransform target, RectTransform.Axis axis = RectTransform.Axis.Vertical)
    {
        // The scroll rect's view's space is used to calculate scroll position.
        var view = scrollRect.viewport ?? scrollRect.GetComponent<RectTransform>();

        // Calculate the scroll offset in the view's space.
        var viewRect = view.rect;
        var elementBounds = target.TransformBoundsTo(view);

        // Normalize and apply the calculated offset.
        if (axis == RectTransform.Axis.Vertical)
        {
            var offset = viewRect.center.y - elementBounds.center.y;
            var scrollPos = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset);
            scrollRect.verticalNormalizedPosition = Mathf.Clamp(scrollPos, 0, 1);
        }
        else
        {
            var offset = viewRect.center.x - elementBounds.center.x;
            var scrollPos = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(0, offset);
            scrollRect.horizontalNormalizedPosition = Mathf.Clamp(scrollPos, 0, 1);
        }
    }

    public static float ScrollToCenterNormalizedValue(this ScrollRect scrollRect, RectTransform target, RectTransform.Axis axis = RectTransform.Axis.Vertical)
    {
        // The scroll rect's view's space is used to calculate scroll position.
        var view = scrollRect.viewport ?? scrollRect.GetComponent<RectTransform>();

        // Calculate the scroll offset in the view's space.
        var viewRect = view.rect;
        var elementBounds = target.TransformBoundsTo(view);

        // Normalize and apply the calculated offset.
        if (axis == RectTransform.Axis.Vertical)
        {
            var offset = viewRect.center.y - elementBounds.center.y;
            var scrollPos = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset);
            return Mathf.Clamp(scrollPos, 0, 1);
        }
        else
        {
            var offset = viewRect.center.x - elementBounds.center.x;
            var scrollPos = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(0, offset);
            return Mathf.Clamp(scrollPos, 0, 1);
        }
    }
}
