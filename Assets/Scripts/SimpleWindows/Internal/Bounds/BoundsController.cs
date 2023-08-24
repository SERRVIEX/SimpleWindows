namespace SimpleWindow.Internal
{
    using UnityEngine;

    internal class BoundsController : MonoBehaviour
    {
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }

        [SerializeField] private BoundController _left;
        [SerializeField] private BoundController _right;
        [SerializeField] private BoundController _top;
        [SerializeField] private BoundController _bottom;

        // Constructors

        private BoundsController() { }

        // Methods

        public void SetWindow(WindowController window)
        {
            _left.SetWindow(window);
            _right.SetWindow(window);
            _top.SetWindow(window);
            _bottom.SetWindow(window);
        }

        public void Set(BoundController.Position position, BoundController controller)
        {
            switch (position)
            {
                case BoundController.Position.Left:
                    _left = controller;
                    break;

                case BoundController.Position.Right:
                    _right = controller;
                    break;

                case BoundController.Position.Top:
                    _top = controller;
                    break;

                case BoundController.Position.Bottom:
                    _bottom = controller;
                    break;
            }
        }

        public BoundController Get(BoundController.Position position)
        {
            return position switch
            {
                BoundController.Position.Left => _left,
                BoundController.Position.Right => _right,
                BoundController.Position.Top => _top,
                BoundController.Position.Bottom => _bottom,
                _ => null,
            };
        }

        private void OnValidate()
        {
            if(RectTransform == null)
                RectTransform = GetComponent<RectTransform>();

            if(CanvasGroup == null)
                CanvasGroup = GetComponent<CanvasGroup>();

            Find(BoundController.Position.Left, ref _left);
            Find(BoundController.Position.Right, ref _right);
            Find(BoundController.Position.Top, ref _top);
            Find(BoundController.Position.Bottom, ref _bottom);
        }

        private void Find(BoundController.Position position, ref BoundController controller)
        {
            Transform child = transform.Find(position.ToString());
            if (child != null)
                if(child.GetComponent<BoundController>() != null)   
                    controller = child.GetComponent<BoundController>();
        }
    }
}