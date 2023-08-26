namespace SimpleWindow.Internal
{
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(LayoutElement))]
    public class WindowController : MonoBehaviour, IPointerMoveHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
    {
        /// <summary>
        /// The window is selected automatically when the mouse is inside it.
        /// </summary>
        public static WindowController Selected { get; protected set; }

        [field: SerializeField] public RectTransform RectTransform { get; protected set; }

        /// <summary>
        /// Is mouse inside the window rect.
        /// </summary>
        public bool IsPointerInside;

        /// <summary>
        /// Detect if the pointer is near the window border.
        /// </summary>
        protected bool IsPointerOnBorder;

        /// <summary>
        /// Which border is closer to the mouse.
        /// </summary>
        public Border ClosestBorder { get; protected set; }

        protected Vector3 StartPointerPosition;
        protected float StartSplitLinePosition;

        /// <summary>
        /// Min window size.
        /// </summary>
        public Vector3 MinSize => new Vector3(300, 150);

        public bool IsFloating
        {
            get => _isFloating;
            set => _isFloating = value;
        }

        public bool _isFloating;

        /// <summary>
        /// Here can be 2 or 0 childs.
        /// When this window is not split, it has no children.
        /// When the window is splitted then this one is clone and converted to a child, 
        /// and another child is came from arguments. 
        /// </summary>
        [SerializeField] public List<WindowController> Children = new List<WindowController>();
        public WindowController this[int index] => Children[index];

        /// <summary>
        /// On which axis will align the child windows.
        /// </summary>
        public LayoutType Layout { get; protected set; }

        [field: SerializeField] public Header Header { get; private set; }
        [field: SerializeField] public Actions Actions { get; private set; }
        [field: SerializeField] internal BoundsController BoundsController { get; private set; }

        [field: SerializeField] public WindowController Parent { get; private set; }

        [field: SerializeField] public Image Background { get; private set; }
        [field: SerializeField] public LayoutElement LayoutElement { get; private set; }
        [field: SerializeField] public RectTransform Content { get; private set; }
        [field: SerializeField] public RectTransform FrontLayer { get; private set; }

        /// <summary>
        /// Set the middle position between the childs (%).
        /// </summary>
        public float CenterNormalizedPosition;

        /// <summary>
        /// Determine if the border can be dragged.
        /// </summary>
        private bool _canBeDragged;

        /// <summary>
        /// Determine if it's dragging the border.
        /// </summary>
        private bool _isDragging;

        // Constructors

        private WindowController() { }

        // Methods

        private void Update()
        {
            if (Children.Count > 0)
                IsPointerInside = false;
        }

        protected void OnValidate()
        {
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();

            if (Background == null)
                Background = GetComponent<Image>();

            if (LayoutElement == null)
                LayoutElement = GetComponent<LayoutElement>();
        }

        public void Process()
        {
            Actions.SetFloatActiveActions(IsFloating);

            if (IsRoot())
                Actions.gameObject.SetActive(IsFloating);
            else
                Actions.gameObject.SetActive(false);

            if (FrontLayer != null)
                FrontLayer.SetAsLastSibling();

            if (BoundsController != null)
                BoundsController.transform.SetAsLastSibling();
        }

        public bool IsRoot()
        {
            return IsRootTest(this, 0);
        }

        private bool IsRootTest(WindowController window, int deep)
        {
            // If parent wasn't found. 
            if (window.Parent == null)
            {
                // If parent of verifying window was a root group (1 recursion) then return true.
                return deep == 0;
            }

            deep++;
            return IsRootTest(window.Parent, deep);
        }

        public void Link(Window item)
        {
            item.Controller = this;
            Header.AddWindow(item);
            Process();
        }

        public void Link(TabView item)
        {
            item.Window.Controller = this;
            Header.AddTab(item);
            Process();
        }

        /// <summary>
        /// Attach new window to this one.
        /// That will create a group with these current and new window.
        /// </summary>
        public void Attach(WindowController controller, Window window)
        {
            // Make new window as a child of this.
            controller.transform.SetParent(transform);
            controller.transform.localScale = Vector3.one;
            controller.Parent = this;
            window.Controller = controller;

            // Clone this window as a child and the original convert into a group.
            WindowController current = MakeAsChild();

            switch (ClosestBorder)
            {
                // Set new window to the left of this window.
                case Border.Left:
                    Children.Add(controller);
                    Children.Add(current);
                    ProcessHorizontal();
                    break;

                // Set new window to the right of this window.
                case Border.Right:
                    Children.Add(current);
                    Children.Add(controller);
                    ProcessHorizontal();
                    break;

                // Set new window to the top of this window.
                case Border.Top:
                    Children.Add(controller);
                    Children.Add(current);
                    ProcessVertical();
                    break;

                // Set new window to the bottom of this window.
                case Border.Bottom:
                    Children.Add(current);
                    Children.Add(controller);
                    ProcessVertical();
                    break;
            }

            // Order the children.
            for (int i = 0; i < Children.Count; i++)
                Children[i].transform.SetSiblingIndex(i);

            current.Process();
            controller.Process();

            controller.FrontLayer.SetAsLastSibling();
        }

        /// <summary>
        /// Attach new window to this one.
        /// That will create a group with these current and new window.
        /// </summary>
        public void Attach(WindowController controller, Window window, LayoutType layout)
        {
            // Make new window as a child of this.
            controller.transform.SetParent(transform);
            controller.transform.localScale = Vector3.one;
            controller.Parent = this;
            window.Controller = controller;

            // Clone this window as a child and the original convert into a group.
            WindowController current = MakeAsChild();

            if(layout == LayoutType.Horizontal)
            {
                Children.Add(current);
                Children.Add(controller);
                ProcessHorizontal();
            }
            else
            {
                Children.Add(current);
                Children.Add(controller);
                ProcessVertical();
            }

            // Order the children.
            for (int i = 0; i < Children.Count; i++)
                Children[i].transform.SetSiblingIndex(i);

            current.Process();
            controller.Process();

            controller.FrontLayer.SetAsLastSibling();
        }

        /// <summary>
        /// Make a child clone of the current window.
        /// </summary>
        private WindowController MakeAsChild()
        {
            // Create new window.
            WindowController controller = new GameObject("Window").AddComponent<WindowController>();
            controller.transform.SetParent(transform);
            controller.transform.localScale = Vector3.one;

            controller.Parent = this;
            controller.IsFloating = IsFloating;
            
            controller.Background.color = Background.color;

            // Grab the content to the new window.
            controller.Content = Content;
            controller.Content.transform.SetParent(controller.transform, false);
            controller.Content.localPosition = Vector3.zero;
            controller.Content.SetAnchor(Anchor.Stretch);
            controller.Content.SetOffset(0, 0, 0, 0);

            // Grab the header to the new window.
            controller.Header = Header;
            controller.Header.Controller = controller;

            for (int i = 0; i < controller.Header.Tabs.Count; i++)
                controller.Header.Tabs[i].Window.Controller = controller;

            controller.Actions = Actions;
            controller.Actions.RectTransform.SetParent(controller.transform);
            controller.Actions.SetWindow(controller);
            controller.Actions.gameObject.SetActive(false);

            // Grab the header to the new window.
            controller.BoundsController = BoundsController;
            controller.BoundsController.RectTransform.SetParent(controller.transform);
            controller.BoundsController.SetWindow(controller);
            controller.BoundsController.gameObject.SetActive(false);

            // Grab the front layer.
            controller.FrontLayer = FrontLayer;
            controller.FrontLayer.SetParent(controller.transform, false);

            Content = null;
            Header = null;

            CreateActions(controller.Actions);
            CreateBoundsController(controller.BoundsController);

            FrontLayer = null;

            return controller;
        }

        private void CreateActions(Actions original)
        {
            Actions = Instantiate(original, transform);
            Actions.GetComponent<LayoutElement>().ignoreLayout = true;
            Actions.RectTransform.SetAnchor(Anchor.TopRight);
            Actions.RectTransform.SetPivot(Pivot.TopRight);
            Actions.RectTransform.anchoredPosition = Vector3.zero;
            Actions.SetWindow(this);

            Actions.gameObject.SetActive(Parent == null);
        }

        private void CreateBoundsController(BoundsController original)
        {
            BoundsController = Instantiate(original, transform);
            BoundsController.RectTransform.SetAnchor(Anchor.Stretch);
            BoundsController.RectTransform.SetPivot(Pivot.MiddleCenter);
            BoundsController.RectTransform.SetOffset(0, 0, 0, 0);
            BoundsController.SetWindow(this);

            BoundsController.gameObject.SetActive(Parent == null);
        }

        /// <summary>
        /// Add a horizontal layout to group to show the childs correct.
        /// Call this function when the new window is near to left or right border.
        /// </summary>
        protected void ProcessHorizontal()
        {
            HorizontalLayoutGroup horizontalLayoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.childForceExpandWidth = true;
            horizontalLayoutGroup.childForceExpandHeight = true;
            horizontalLayoutGroup.childControlWidth = true;
            horizontalLayoutGroup.childControlHeight = true;
            horizontalLayoutGroup.childScaleWidth = true;
            horizontalLayoutGroup.childScaleHeight = true;

            CenterNormalizedPosition = 50;

            for (int i = 0; i < Children.Count; i++)
            {
                WindowController child = Children[i];
                child.LayoutElement.preferredWidth = RectTransform.rect.width * CenterNormalizedPosition / 100;
                child.LayoutElement.preferredHeight = 0;
            }

            Layout = LayoutType.Horizontal;
        }

        /// <summary>
        /// Add a vertical layout to group to show the childs correct.
        /// Call this function when the new window is near to top or bottom border.
        /// </summary>
        protected void ProcessVertical()
        {
            VerticalLayoutGroup verticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childForceExpandWidth = true;
            verticalLayoutGroup.childForceExpandHeight = true;
            verticalLayoutGroup.childControlWidth = true;
            verticalLayoutGroup.childControlHeight = true;
            verticalLayoutGroup.childScaleWidth = true;
            verticalLayoutGroup.childScaleHeight = true;

            CenterNormalizedPosition = 50;

            for (int i = 0; i < Children.Count; i++)
            {
                WindowController child = Children[i];
                child.LayoutElement.preferredWidth = 0;
                child.LayoutElement.preferredHeight = RectTransform.rect.height * CenterNormalizedPosition / 100;
            }

            Layout = LayoutType.Vertical;
        }

        /// <summary>
        /// Detach a window from this group.
        /// </summary>
        /// <param name="controller">Child window.</param>
        public void Detach(WindowController controller)
        {
            if (Children.Contains(controller))
            {
                if (controller.Header.Tabs.Count > 0)
                    return;

                WindowController origin;

                if (controller == Children[0])
                    origin = Children[1];
                
                else
                    origin = Children[0];

                MakeAsRoot(origin);
                origin.Process();
                Process();
            }
        }

        /// <summary>
        /// When a child is detached, th group is disolved and another one becomes a root.
        /// </summary>
        /// <param name="controller">New root window.</param>
        private void MakeAsRoot(WindowController controller)
        {
            // Clear the group children.
            Children.Clear();

            IsFloating = controller.IsFloating;
            Background.color = controller.Background.color;

            // If the child doesn't have children (isn't a group) then we must destroy the
            // layout from this group and grab the content from the child to the root.
            if (controller.Children.Count == 0)
            {
                if (Actions != null)
                    DestroyImmediate(Actions.gameObject);

                if (BoundsController == null)
                    DestroyImmediate(BoundsController.gameObject);

                if (GetComponent<LayoutGroup>() != null)
                    DestroyImmediate(GetComponent<LayoutGroup>());

                Content = controller.Content;
                Content.SetParent(transform);
                Content.SetOffset(0, 0, 0, 0);

                Header = controller.Header;
                Header.Controller = this;

                Actions = controller.Actions;
                Actions.SetWindow(this);
                Actions.transform.SetParent(Header.transform);
                Actions.RectTransform.anchoredPosition = new Vector2(0, 0);

                BoundsController = controller.BoundsController;
                BoundsController.SetWindow(this);
                BoundsController.transform.SetParent(transform);

                FrontLayer = controller.FrontLayer;
                FrontLayer.SetParent(transform);
                FrontLayer.SetOffset(0, 0, 0, 0);

                Window window = Header.GetWindow(controller);
                window.Controller = this;
            }
            // If the new root has children (it's a group) then we simply copy its children to this one.
            else
            {
                for (int i = 0; i < controller.Children.Count; i++)
                {
                    WindowController child = controller.Children[i];
                    child.transform.SetParent(transform);
                    child.transform.localScale = Vector3.one;
                    Children.Add(child);
                }

                UpdateParent(this);

                // Copy the layout from new root to this one.
                CopyLayout(controller);

                for (int i = 0; i < controller.Children.Count; i++)
                {
                    WindowController child = controller.Children[i];
                    child.Process();
                }
            }

            Destroy(controller.gameObject);
        }

        public void SetBoundsControllersActive(bool value)
        {
            if (BoundsController != null)
            {
                BoundsController.CanvasGroup.interactable = value;
                BoundsController.CanvasGroup.blocksRaycasts = value;
            }
        }

        /// <summary>
        /// Update parents after modifying the groups.
        /// </summary>
        private void UpdateParent(WindowController window)
        {
            for (int i = 0; i < window.Children.Count; i++)
            {
                WindowController child = window.Children[i];
                child.Parent = window;
                UpdateParent(child);
            }
        }

        /// <summary>
        /// Copy the layout from the target window with its properties.
        /// </summary>
        /// <param name="window">Target window.</param>
        private void CopyLayout(WindowController window)
        {
            // If this is a group, then it has a layout group, so we need to destroy it.
            LayoutGroup oldLayoutGroup = GetComponent<LayoutGroup>();
            if (oldLayoutGroup != null)
                DestroyImmediate(oldLayoutGroup);

            // Copy the line position to keep the children sizes correctly.
            CenterNormalizedPosition = window.CenterNormalizedPosition;

            // Copy the new layout.
            LayoutGroup newLayoutGroup = window.GetComponent<LayoutGroup>();
            if (newLayoutGroup is HorizontalLayoutGroup)
                ProcessHorizontal();
            else 
                ProcessVertical();

            UpdateLayouts(this);
        }

        private void UpdateLayouts(WindowController controller)
        {
            float[] size = new float[2];

            if (controller.Layout == LayoutType.Horizontal)
            {
                size[0] = controller.RectTransform.rect.width * controller.CenterNormalizedPosition / 100;
                size[1] = controller.RectTransform.rect.width - size[0];

                for (int i = 0; i < controller.Children.Count; i++)
                {
                    WindowController child = controller.Children[i];
                    child.LayoutElement.preferredWidth = size[i];
                    child.LayoutElement.preferredHeight = 0;
                }
            }
            else
            {
                size[0] = controller.RectTransform.rect.height * controller.CenterNormalizedPosition / 100;
                size[1] = controller.RectTransform.rect.height - size[0];

                for (int i = 0; i < controller.Children.Count; i++)
                {
                    WindowController child = controller.Children[i];
                    child.LayoutElement.preferredWidth = 0;
                    child.LayoutElement.preferredHeight = size[i];
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsPointerOnBorder && _canBeDragged)
            {
                _isDragging = true;
                StartPointerPosition = Input.mousePosition;
                StartSplitLinePosition = Parent.CenterNormalizedPosition;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;

            float diff = Parent.Layout == LayoutType.Horizontal ? Input.mousePosition.x - StartPointerPosition.x : StartPointerPosition.y - Input.mousePosition.y;
            float max = Parent.Layout == LayoutType.Horizontal ? Parent.RectTransform.rect.width : Parent.RectTransform.rect.height;

            float start = max * StartSplitLinePosition / 100;
            float value = Mathf.Clamp(start + diff, 0, max);
            float percent = value / max * 100;
            Parent.CenterNormalizedPosition = Mathf.Clamp(percent, 10f, 90f);
            UpdateLayouts(Parent);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;
            _isDragging = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Children.Count == 0)
            {
                Selected = this;
                IsPointerInside = true;
            }
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (_isDragging)
                return;

            if (Children.Count == 2)
                return;

            ClosestBorder = Border.None;

            Vector2 localPosition = eventData.PointerDataToRelativePos(RectTransform);
            if (Parent == null)
            {
                if (localPosition.x < Constants.BorderDetectionThreshold)
                {
                    IsPointerOnBorder = true;
                    ClosestBorder = Border.Left;
                }
                else if (localPosition.x > RectTransform.rect.width - Constants.BorderDetectionThreshold)
                {
                    IsPointerOnBorder = true;
                    ClosestBorder = Border.Right;
                }
                else if (localPosition.y < Constants.BorderDetectionThreshold)
                {
                    IsPointerOnBorder = true;
                    ClosestBorder = Border.Bottom;
                }
                else if (localPosition.y > RectTransform.rect.height - Constants.BorderDetectionThreshold)
                {
                    IsPointerOnBorder = true;
                    ClosestBorder = Border.Top;
                }
            }
            else
            {
                if (Parent.Children.Count == 0)
                    return;

                if (localPosition.x < Constants.BorderDetectionThreshold)
                {
                    IsPointerOnBorder = true;
                    ClosestBorder = Border.Left;
                }
                else if (localPosition.x > RectTransform.rect.width - Constants.BorderDetectionThreshold)
                {
                    IsPointerOnBorder = true;
                    ClosestBorder = Border.Right;
                }
                else if (localPosition.y < Constants.BorderDetectionThreshold)
                {
                    IsPointerOnBorder = true;
                    ClosestBorder = Border.Bottom;
                }
                else if (localPosition.y > RectTransform.rect.height - Constants.BorderDetectionThreshold)
                {
                    IsPointerOnBorder = true;
                    ClosestBorder = Border.Top;
                }

                if (Parent.Layout == LayoutType.Horizontal)
                {
                    if (Parent[1] == this && localPosition.x < Constants.BorderDetectionThreshold)
                    {
                        _canBeDragged = true;
                        return;
                    }
                    else if (Parent[0] == this && localPosition.x > RectTransform.rect.width - Constants.BorderDetectionThreshold)
                    {
                        _canBeDragged = true;
                        return;
                    }
                }
                else if (Parent.Layout == LayoutType.Vertical)
                {
                    if (Parent[0] == this && localPosition.y < Constants.BorderDetectionThreshold)
                    {
                        _canBeDragged = true;
                        return;
                    }
                    else if (Parent[1] == this && localPosition.y > RectTransform.rect.height - Constants.BorderDetectionThreshold)
                    {
                        _canBeDragged = true;
                        return;
                    }
                }
            }

            _canBeDragged = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsPointerInside = false;
            IsPointerOnBorder = false;
            Selected = null;
        }

        public void SetCenterNormalizedPosition(float value)
        {
            CenterNormalizedPosition = value;
            if(Children.Count > 0)
                UpdateLayouts(this);
        }

        public Vector2 GetNormalizedSize()
        {
            Vector2 size = new Vector2
            {
                x = RectTransform.rect.width / WindowsManager.RectTransform.rect.width,
                y = RectTransform.rect.height / WindowsManager.RectTransform.rect.height
            };

            return size;
        }

        public void SetNormalizedSize(Vector2 value)
        {
            RectTransform.SetSize(new Vector2(value.x * WindowsManager.RectTransform.rect.width, value.y * WindowsManager.RectTransform.rect.height));
        }

        public Vector2 GetNormalizedPosition()
        {
            Vector2 position = new Vector2
            {
                x = RectTransform.localPosition.x / WindowsManager.RectTransform.rect.width,
                y = RectTransform.localPosition.y / WindowsManager.RectTransform.rect.height
            };

            return position;
        }

        public void SetNormalizedPosition(Vector2 position)
        {
            RectTransform.localPosition = new Vector3(position.x * WindowsManager.RectTransform.rect.width, position.y * WindowsManager.RectTransform.rect.height, 0);
        }
    }
}