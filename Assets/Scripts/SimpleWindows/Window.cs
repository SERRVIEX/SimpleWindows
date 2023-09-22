namespace SimpleWindow
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    using SimpleWindow.Internal;

    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(LayoutElement))]
    public abstract class Window : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerMoveHandler
    {
        [HideInInspector] public WindowController Controller;

        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] public string Label { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }

        [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
        [field: SerializeField] protected LayoutElement Layout { get; private set; }

        public float Width => RectTransform.rect.width;
        public float Height => RectTransform.rect.height;

        // Methods

        protected virtual void Awake()
        {
            if(string.IsNullOrEmpty(Label))
                Label = "Unknown";
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Controller.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Controller.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Controller.OnEndDrag(eventData);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            Controller.OnPointerMove(eventData);
        }

        protected virtual void OnValidate()
        {
            name = GetType().ToString();

            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();

            if (CanvasGroup == null)
                CanvasGroup = GetComponent<CanvasGroup>();

            if (Layout == null)
                Layout = GetComponent<LayoutElement>();

            Layout.preferredHeight = 9999;
        }
    }
}