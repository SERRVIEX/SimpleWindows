namespace SimpleWindow
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public sealed class Move : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private WindowController _reference;

        private Vector3 _localPosition;
        private Vector2 _delta;

        // Methods

        private void Start()
        {
            ClampToViewport();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _localPosition = _reference.RectTransform.localPosition;
            _delta = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _delta += eventData.delta * (1920f / Screen.width);

            Vector3 localPosition = _localPosition + new Vector3(_delta.x, _delta.y, 0);

            if(Input.GetKey(KeyCode.LeftShift))
            {
                localPosition.x = Mathf.RoundToInt(localPosition.x / 25) * 25;
                localPosition.y = Mathf.RoundToInt(localPosition.y / 25) * 25;
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                localPosition.x = Mathf.RoundToInt(localPosition.x / 10) * 10;
                localPosition.y = Mathf.RoundToInt(localPosition.y / 10) * 10;
            }

            _reference.RectTransform.localPosition = localPosition;

            ClampToViewport();

            Vector2Int halfScreen = new Vector2Int(Screen.width, Screen.height) / 4;

            if(_reference.RectTransform.localPosition.x < -halfScreen.x && _reference.RectTransform.localPosition.y < -halfScreen.y)
                _reference.RectTransform.SetAnchor(Anchor.BottomLeft);
            
            else if (_reference.RectTransform.localPosition.x < -halfScreen.x && _reference.RectTransform.localPosition.y > halfScreen.y)
                _reference.RectTransform.SetAnchor(Anchor.TopLeft);

            else if (_reference.RectTransform.localPosition.x > halfScreen.x && _reference.RectTransform.localPosition.y < -halfScreen.y)
                _reference.RectTransform.SetAnchor(Anchor.BottomRight);

            else if (_reference.RectTransform.localPosition.x > halfScreen.x && _reference.RectTransform.localPosition.y > halfScreen.y)
                _reference.RectTransform.SetAnchor(Anchor.TopRight);

            else if (_reference.RectTransform.localPosition.x < -halfScreen.x)
                _reference.RectTransform.SetAnchor(Anchor.MiddleLeft);

            else if (_reference.RectTransform.localPosition.x > halfScreen.x)
                _reference.RectTransform.SetAnchor(Anchor.MiddleRight);

            else if (_reference.RectTransform.localPosition.y < -halfScreen.y)
                _reference.RectTransform.SetAnchor(Anchor.BottomCenter);

            else if (_reference.RectTransform.localPosition.y > halfScreen.y)
                _reference.RectTransform.SetAnchor(Anchor.TopCenter);

            else
                _reference.RectTransform.SetAnchor(Anchor.MiddleCenter);
        }

        private void ClampToViewport()
        {
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
    }
}