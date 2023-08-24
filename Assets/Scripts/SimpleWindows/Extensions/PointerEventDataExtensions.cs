namespace SimpleWindow
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public static class PointerEventDataExtensions
    {
        public static Vector2 PointerDataToRelativePos(this PointerEventData eventData, RectTransform rectTransform)
        {
            Vector2 result;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, Camera.main, out result);
            result += rectTransform.sizeDelta / 2;
            return result;
        }
    }
}