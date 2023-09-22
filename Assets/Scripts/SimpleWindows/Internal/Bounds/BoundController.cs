namespace SimpleWindow.Internal
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    internal sealed class BoundController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        internal enum Position
        {
            Left,
            Right,
            Top,
            Bottom
        }

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _image;
        [SerializeField] private WindowController _reference;
        [SerializeField] private Position _position;

        private Vector2 _size;
        private Vector2 _anchoredPosition;
        private Vector2 _delta;

        // Constructors

        private BoundController() { }

        // Methods

        public void SetWindow(WindowController controller)
        {
            _reference = controller;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(TabView.Dragging == null)
                _image.color = new Color32(255, 255, 255, 64);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _image.color = Color.clear;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _size = new Vector2(_reference.RectTransform.rect.width, _reference.RectTransform.rect.height);
            _anchoredPosition = _reference.RectTransform.anchoredPosition;
            _delta = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _delta += eventData.delta * WindowsManager.AspectRatioFactor;

            switch (_position)
            {
                case Position.Left:
                    if (_size.x - _delta.x < _reference.MinSize.x)
                        return;

                    _reference.RectTransform.anchoredPosition = _anchoredPosition + new Vector2(_delta.x / 2f, 0);
                    _reference.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Clamp(_size.x - _delta.x, _reference.MinSize.x, int.MaxValue));

                    break;

                case Position.Right:
                    if (_size.x + _delta.x < _reference.MinSize.x)
                        return;

                    _reference.RectTransform.anchoredPosition = _anchoredPosition + new Vector2(_delta.x / 2f, 0);
                    _reference.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Clamp( _size.x + _delta.x, _reference.MinSize.x, int.MaxValue));

                    break;
                case Position.Top:
                    if (_size.y + _delta.y < _reference.MinSize.y)
                        return;

                    _reference.RectTransform.anchoredPosition = _anchoredPosition + new Vector2(0, _delta.y / 2f);
                    _reference.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Clamp(_size.y + _delta.y, _reference.MinSize.y, int.MaxValue));
                   
                    break;

                case Position.Bottom:
                    if (_size.y - _delta.y < _reference.MinSize.y)
                        return;

                    _reference.RectTransform.anchoredPosition = _anchoredPosition + new Vector2(0, _delta.y / 2f);
                    _reference.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Clamp(_size.y - _delta.y, _reference.MinSize.y, int.MaxValue));

                    break;
            }

            Vector3 localPosition = _reference.RectTransform.localPosition;
            localPosition.x = Mathf.Clamp(localPosition.x, WindowsManager.Margin.left + _reference.RectTransform.rect.width / 2f, WindowsManager.Margin.right - _reference.RectTransform.rect.width / 2f);
            localPosition.y = Mathf.Clamp(localPosition.y, WindowsManager.Margin.bottom + _reference.RectTransform.rect.height / 2f, WindowsManager.Margin.top - _reference.RectTransform.rect.height / 2f);
            localPosition.z = 0;
            _reference.RectTransform.localPosition = localPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            WindowsManager.MarkDirty();
        }

        private void OnValidate()
        {
            name = _position.ToString();

            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            if (_image == null)
                _image = GetComponent<Image>();

            switch (_position)
            {
                case Position.Left:
                    _rectTransform.SetAnchor(Anchor.StretchLeft);
                    _rectTransform.SetPivot(Pivot.MiddleLeft);
                    _rectTransform.SetWidth(4);
                    _rectTransform.anchoredPosition = new Vector2(0, 0);
                    _rectTransform.SetTop(10);
                    _rectTransform.SetBottom(10);
                    break;

                case Position.Right:
                    _rectTransform.SetAnchor(Anchor.StretchRight);
                    _rectTransform.SetPivot(Pivot.MiddleRight);
                    _rectTransform.SetWidth(4);
                    _rectTransform.anchoredPosition = new Vector2(0, 0);
                    _rectTransform.SetTop(10);
                    _rectTransform.SetBottom(10);
                    break;

                case Position.Top:
                    _rectTransform.SetAnchor(Anchor.StretchTop);
                    _rectTransform.SetPivot(Pivot.TopCenter);
                    _rectTransform.SetHeight(4);
                    _rectTransform.anchoredPosition = new Vector2(0, 0);
                    _rectTransform.SetLeft(10);
                    _rectTransform.SetRight(10);
                    break;

                case Position.Bottom:
                    _rectTransform.SetAnchor(Anchor.StretchBottom);
                    _rectTransform.SetPivot(Pivot.BottomCenter);
                    _rectTransform.SetHeight(4);
                    _rectTransform.anchoredPosition = new Vector2(0, 0);
                    _rectTransform.SetLeft(10);
                    _rectTransform.SetRight(10);
                    break;
            }
        }
    }
}