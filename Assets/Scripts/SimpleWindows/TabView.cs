namespace SimpleWindow
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;

    using TMPro;
    using static UnityEngine.RuleTile.TilingRuleOutput;

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public sealed class TabView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        /// <summary>
        /// The object that is currently being dragged.
        /// </summary>
        public static TabView Dragging { get; private set; }

        /// <summary>
        /// The object on which the cursor is located.
        /// </summary>
        public static TabView Focused { get; private set; }

        /// <summary>
        /// Reference to the header.
        /// </summary>
        [field: SerializeField] public Header Header { get; set; }

        /// <summary>
        /// Reference to the window.
        /// </summary>
        [field: SerializeField] public Window Window { get; private set; }

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                if(_active)
                {
                    _image.color = new Color32(125, 125, 125, 255);
                    _icon.color = new Color32(255, 255, 255, 255);
                    _title.color = new Color32(25, 25, 25, 255);
                }
                else
                {
                    _image.color = new Color32(100, 100, 100, 255);
                    _icon.color = new Color32(100, 100, 100, 255);
                    _title.color = new Color32(25, 25, 25, 255);
                }

                Window.gameObject.SetActive(_active);
            }
        }

        private bool _active;

        [HideInInspector] public UnityEvent OnEndDragHandler = new UnityEvent();

        [SerializeField] private Image _image;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _title;

        private Header _previousHeader;

        // Methods

        private void Awake() => name = GetType().Name;

        public void Link(Window controller)
        {
            Window = controller;
            _title.text = Window.Title;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Dragging != null && Dragging != this)
                Focused = this;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Moving tab is not allowed when 1 static window left.
            if (WindowsManager.GetStaticWindowCount() == 1 && !Window.WindowController.IsFloating && Window.WindowController.Header.Tabs.Count == 1)
                return;

            Dragging = this;
            _image.raycastTarget = false;
            _canvasGroup.alpha = 0.25f;

            _previousHeader = Header;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Header.Select(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Moving tab is not allowed when 1 static window left.
            if (WindowsManager.GetStaticWindowCount() == 1 && !Window.WindowController.IsFloating && Window.WindowController.Header.Tabs.Count == 1)
                return;

            // If this tab is not in the headers.
            if (Header.Focused == null)
            {
                // If old header is still linked.
                if (Header != null)
                {
                    // Unlink this tab from the header.
                    Header.RemoveTab(this);
                }

                RectTransformUtility.ScreenPointToLocalPointInRectangle(WindowsManager.RectTransform, Input.mousePosition, WindowsManager.Camera, out Vector2 localPosition);
                transform.localPosition = localPosition;
            }
            else
            {
                if(Header != Header.Focused)
                    Header.Focused.AddTab(this);

                if (Focused != null)
                {
                    transform.SetSiblingIndex(Focused.transform.GetSiblingIndex());
                    Header.OrderBySiblingIndex();
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Moving tab is not allowed when 1 static window left.
            if (WindowsManager.GetStaticWindowCount() == 1 && !Window.WindowController.IsFloating && Window.WindowController.Header.Tabs.Count == 1)
                return;

            if (Header == null)
            {
                // If the pointer is inside of the one window.
                // If selected window is not the same as the tab controller window.
                // If the pointer is close to one of the window borders
                // then split it into two windows from old and new one.
                if (WindowController.Selected != null && WindowController.Selected != Window.WindowController && WindowController.Selected.ClosestBorder != Border.None)
                {
                    // Don't allow to attach to the child of the same parent (problems).
                    if (WindowController.Selected.Parent == null || WindowController.Selected.Parent != null && WindowController.Selected.Parent != Window.WindowController.Parent)
                    {
                        Window.transform.SetParent(WindowsManager.RectTransform);

                        if (Window.WindowController == null || _previousHeader.Tabs.Count > 0)
                            Window.WindowController = WindowsManager.CreateWindow(this);

                        if (Window.WindowController.Parent != null)
                            Window.WindowController.Parent.Detach(Window.WindowController, Window);

                        WindowController.Selected.Attach(Window.WindowController, Window);

                        Window.WindowController.Header.AddTab(this);
                        Window.WindowController.Header.Select(this);

                        Window.transform.SetParent(Header.Window.Content);
                        Window.transform.localPosition = Vector3.zero;
                    }
                    else
                        CreateFloatWindow();
                }
                else
                {
                    if (WindowController.Selected != null &&
                        WindowController.Selected == Window.WindowController &&
                        WindowController.Selected.ClosestBorder != Border.None &&
                        WindowController.Selected.Header.Tabs.Count >= 1)
                    {
                        Window.WindowController = WindowsManager.CreateWindow(this);
                        WindowController.Selected.Attach(Window.WindowController, Window);

                        //Window.WindowController.Header.AddTab(this);
                        Window.WindowController.Header.Select(this);

                        Window.transform.SetParent(Header.Window.Content);
                        Window.transform.localPosition = Vector3.zero;
                    }
                    else
                    {
                        CreateFloatWindow();
                    }
                }
            }
            else
            {
                WindowController controller = Window.WindowController;
                if (controller.Parent != null)
                    controller.Parent.Detach(controller, Window);

                Window.WindowController = Header.Window;
                Window.transform.SetParent(Header.Window.Content);
                Header.Select(this);

                if (controller != null && Window.WindowController != controller && _previousHeader.Tabs.Count == 0)
                    Destroy(controller.gameObject);
            }

            Dragging = null;
            _image.raycastTarget = true;
            _canvasGroup.alpha = 1;

            OnEndDragHandler?.Invoke();
        }

        private void CreateFloatWindow()
        {
            // Change the parent of the controller to avoid destroying it in detaching process.
            Window.transform.SetParent(WindowsManager.RectTransform);

            WindowController controller = Window.WindowController;
            if (controller.Parent != null)
                controller.Parent.Detach(controller, Window);

            // Create floater window.
            WindowsManager.CreateWindow(this);

            if(Window.WindowController != controller && _previousHeader.Tabs.Count == 0)
                Destroy(controller.gameObject);

            Header.Select(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Focused = null;
        }

        private void OnValidate()
        {
            _image = GetComponent<Image>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        }
    }
}