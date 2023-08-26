namespace SimpleWindow.Internal
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;

    using TMPro;

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

        [HideInInspector] public UnityEvent OnEndDragHandler = new UnityEvent();

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                if(_active)
                {
                    _image.color = new Color32(65, 65, 65, 255);
                    _title.color = new Color32(239, 239, 239, 255);
                }
                else
                {
                    _image.color = new Color32(49, 49, 49, 255);
                    _title.color = new Color32(176, 176, 176, 255);
                }

                Window.gameObject.SetActive(_active);
            }
        }

        private bool _active;

        [SerializeField] private Image _image;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _title;

        private Header _headerTemp;
        private int _tabCountTemp;

        // Constructors

        private TabView() { }

        // Methods

        private void Awake() => name = GetType().Name;

        public void Link(Window window)
        {
            Window = window;
            _title.text = Window.Title;
            _icon.sprite = window.Icon;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Dragging != null && Dragging != this)
                Focused = this;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            WindowsManager.SetBoundsControllersActive(false);

            _tabCountTemp = Window.Controller.Header.Tabs.Count;

            // Moving tab is not allowed when 1 static window left.
            if (WindowsManager.GetStaticWindowCount() == 1 && !Window.Controller.IsFloating && _tabCountTemp == 1)
                return;

            Dragging = this;
            _image.raycastTarget = false;
            _canvasGroup.alpha = 0.25f;

            _headerTemp = Header;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Header.Select(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Moving tab is not allowed when 1 static window left.
            if (WindowsManager.GetStaticWindowCount() == 1 && !Window.Controller.IsFloating && _tabCountTemp == 1)
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
            WindowsManager.SetBoundsControllersActive(true);

            // Moving tab is not allowed when 1 static window left.
            if (WindowsManager.GetStaticWindowCount() == 1 && !Window.Controller.IsFloating && _tabCountTemp == 1)
                return;

            if (Header == null)
            {
                // If the pointer is inside of the one window.
                // If selected window is not the same as the tab controller window.
                // If the pointer is close to one of the window borders
                // then split it into two windows from old and new one.
                if (WindowController.Selected != null && WindowController.Selected != Window.Controller && WindowController.Selected.ClosestBorder != Border.None)
                {
                    // Don't allow to attach to the child of the same parent (problems).
                    if (WindowController.Selected.Parent == null || WindowController.Selected.Parent != null && WindowController.Selected.Parent != Window.Controller.Parent)
                    {
                        Window.transform.SetParent(WindowsManager.RectTransform);

                        if (Window.Controller == null || _headerTemp.Tabs.Count > 0)
                            Window.Controller = WindowsManager.CreateWindowController(this);

                        if (Window.Controller.Parent != null)
                            Window.Controller.Parent.Detach(Window.Controller);

                        WindowController.Selected.Attach(Window.Controller, Window);

                        Window.Controller.Header.AddTab(this);
                        Window.Controller.Header.Select(this);

                        Window.transform.SetParent(Header.Controller.Content);
                        Window.transform.localPosition = Vector3.zero;
                    }
                    else
                        CreateFloatWindow();
                }
                else
                {
                    if (WindowController.Selected != null &&
                        WindowController.Selected == Window.Controller &&
                        WindowController.Selected.ClosestBorder != Border.None &&
                        WindowController.Selected.Header.Tabs.Count >= 1)
                    {
                        Window.Controller = WindowsManager.CreateWindowController(this);
                        WindowController.Selected.Attach(Window.Controller, Window);

                        Window.Controller.Header.Select(this);

                        Window.transform.SetParent(Header.Controller.Content);
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
                WindowController controller = Window.Controller;
                if (controller.Parent != null)
                    controller.Parent.Detach(controller);

                Window.Controller = Header.Controller;
                Window.transform.SetParent(Header.Controller.Content);

                Header.Select(this);

                if (controller != null && Window.Controller != controller && _headerTemp.Tabs.Count == 0)
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

            WindowController controller = Window.Controller;
            if (controller.Parent != null)
                controller.Parent.Detach(controller);

            // Create floater window.
            WindowsManager.CreateWindowController(this);

            if(Window.Controller != controller && _headerTemp.Tabs.Count == 0)
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