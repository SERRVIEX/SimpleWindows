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
    public sealed class WindowController : MonoBehaviour, IPointerMoveHandler, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>
        /// Min window size.
        /// </summary>
        public Vector3 MinSize = new Vector3(300, 150);

        /// <summary>
        /// The window is selected automatically when the mouse is inside it.
        /// </summary>
        public static WindowController Selected { get; private set; }

        [field: SerializeField] public RectTransform RectTransform { get; private set; }

        [HideInInspector] public List<WindowController> Children = new List<WindowController>();
        public WindowController this[int index] => Children[index];

        /// <summary>
        /// On which axis will align the child windows.
        /// </summary>
        public LayoutType Layout { get; private set; }

        [field: SerializeField] public Header Header { get; private set; }
        [field: SerializeField] public Actions Actions { get; private set; }
        [field: SerializeField] internal BoundsController BoundsController { get; private set; }

        public WindowController Parent { get; private set; }

        [field: SerializeField] public Image Background { get; private set; }
        [field: SerializeField] public LayoutElement LayoutElement { get; private set; }
        [field: SerializeField] public RectTransform Content { get; private set; }
        [field: SerializeField] public RectTransform FrontLayer { get; private set; }

        /// <summary>
        /// Set the middle position between the childs (0..99%).
        /// </summary>
        public float CenterNormalizedPosition { get; private set; }

        /// <summary>
        /// Detect if the pointer is near the window border.
        /// </summary>
        private bool _isPointerOnBorder;

        /// <summary>
        /// Which border is closer to the mouse.
        /// </summary>
        public Border ClosestBorder { get; private set; }

        private Vector3 _startPointerPosition;
        private float _startSplitLinePosition;

        [HideInInspector] public bool IsFloating;

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
            {
                BoundsController.transform.SetAsLastSibling();
                BoundsController.gameObject.SetActive(Parent == null && IsFloating);
            }

            SetTheName(this, 0);
        }

        private void SetTheName(WindowController controller, int deep)
        {
            if(controller.Parent != null)
            {
                SetTheName(controller.Parent, deep + 1);
                return;
            }

            name = "Window" + deep + $" ({GetTabName()})";
        }

        private string GetTabName()
        {
            string[] r = null;

            if (Header != null)
            {
                r = new string[Header.Tabs.Count];
                for (int i = 0; i < r.Length; i++)
                {
                    r[i] = Header.Tabs[i].Window.Label;
                }
            }

            return string.Join(", ", r);
        }

        public bool IsRoot() => IsRoot(this, 0);
        
        private bool IsRoot(WindowController window, int deep)
        {
            // If the parent wasn't found. 
            if (window.Parent == null)
            {
                // If the parent of verifying window was a root group (1 recursion) then return true.
                return deep == 0;
            }

            deep++;
            return IsRoot(window.Parent, deep);
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
            controller.transform.localPosition = Vector3.zero;
            controller.transform.localScale = Vector3.one;
            controller.Parent = this;
            controller.IsFloating = IsFloating;

            window.Controller = controller;

            // Clone this window as a child and the original convert into a group.
            WindowController current = MakeAsChild();

            switch (ClosestBorder)
            {
                // Set new window to the left of this window.
                case Border.Left:
                    Children.Add(controller);
                    Children.Add(current);
                    CreateHorizontalGroupLayout();
                    break;

                // Set new window to the right of this window.
                case Border.Right:
                    Children.Add(current);
                    Children.Add(controller);
                    CreateHorizontalGroupLayout();
                    break;

                // Set new window to the top of this window.
                case Border.Top:
                    Children.Add(controller);
                    Children.Add(current);
                    CreateVerticaGroupLayout();
                    break;

                // Set new window to the bottom of this window.
                case Border.Bottom:
                    Children.Add(current);
                    Children.Add(controller);
                    CreateVerticaGroupLayout();
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
        /// Make a child clone of the current window.
        /// </summary>
        private WindowController MakeAsChild()
        {
            // Create new window.
            WindowController controller = new GameObject("Window").AddComponent<WindowController>();
            controller.transform.SetParent(transform);
            controller.transform.localPosition = Vector3.zero;
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
            controller.Header.transform.SetParent(controller.Content);

            for (int i = 0; i < controller.Header.Tabs.Count; i++)
                controller.Header.Tabs[i].Window.Controller = controller;

            controller.Actions = Actions;
            controller.Actions.RectTransform.SetParent(Header.transform);
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
        /// Add a horizontal layout to the group to show the childs correct.
        /// Call this function when the new window is near to left or right border.
        /// </summary>
        protected void CreateHorizontalGroupLayout()
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
                child.LayoutElement.preferredWidth = RectTransform.rect.width * CenterNormalizedPosition / 100 * WindowsManager.AspectRatioFactor;
                child.LayoutElement.preferredHeight = 0;
            }

            Layout = LayoutType.Horizontal;
        }

        /// <summary>
        /// Add a vertical layout to the group to show the childs correct.
        /// Call this function when the new window is near to top or bottom border.
        /// </summary>
        protected void CreateVerticaGroupLayout()
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
                child.LayoutElement.preferredHeight = RectTransform.rect.height * CenterNormalizedPosition / 100 * WindowsManager.AspectRatioFactor;
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
                Header.transform.SetParent(Content);

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
                CreateHorizontalGroupLayout();
            else 
                CreateVerticaGroupLayout();

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
                    child.LayoutElement.preferredWidth = size[i] * WindowsManager.AspectRatioFactor;
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
                    child.LayoutElement.preferredHeight = size[i] * WindowsManager.AspectRatioFactor;
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Body must be performed only in the deepest child that never has children.
            if (Children.Count == 0)
                Selected = this;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Body must be performed only in the deepest child that
            // never has children and if the pointer is on its borders.
            if (Children.Count == 0 && _isPointerOnBorder)
            {
                WindowController draggable = GetDraggableController(this);
                if (draggable != null)
                    draggable._isDragging = true;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Body must be performed only in the deepest child that
            // never has children and if the pointer is on its borders.
            if (Children.Count == 0 && _isPointerOnBorder)
            {
                WindowController controller = GetDraggableController(this);
                if (controller != null)
                    controller.OnBeginDrag();
            }
        }

        /// <summary>
        /// Return the passed controller if it can be dragged or
        /// loop through its parents and search for draggable controller.
        /// </summary>
        /// <param name="controller">Target controller.</param>
        /// <returns>A draggable controller or null.</returns>
        private WindowController GetDraggableController(WindowController controller)
        {
            if(controller != null)
            {
                if(controller._isPointerOnBorder && controller._canBeDragged)
                    return controller;

                return controller.GetDraggableController(controller.Parent);
            }

            return null;
        }

        private void OnBeginDrag()
        {
            _startPointerPosition = Input.mousePosition;
            _startSplitLinePosition = Parent.CenterNormalizedPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            WindowController controller = GetDraggingController(this);
            if (controller != null)
                controller.OnDrag();
        }

        /// <summary>
        /// Return a controller that is in the dragging state.
        /// </summary>
        /// <param name="controller">Target controller.</param>
        /// <returns>A controller in a dragging state or a null.</returns>
        private WindowController GetDraggingController(WindowController controller)
        {
            if (controller != null)
            {
                if (controller._isDragging)
                    return controller;

                return controller.GetDraggingController(controller.Parent);
            }

            return null;
        }

        private void OnDrag()
        {
            float diff = Parent.Layout == LayoutType.Horizontal ? Input.mousePosition.x - _startPointerPosition.x : _startPointerPosition.y - Input.mousePosition.y;
            float max = Parent.Layout == LayoutType.Horizontal ? Parent.RectTransform.rect.width : Parent.RectTransform.rect.height;

            float start = max * _startSplitLinePosition / 100;
            float value = Mathf.Clamp(start + diff, 0, max);
            float percent = value / max * 100;

            Parent.CenterNormalizedPosition = Mathf.Clamp(percent, 10f, 90f);
            UpdateLayouts(Parent);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (Children.Count != 0)
                return;
         
            OnEndDrag();
            WindowsManager.MarkDirty();
        }

        private void OnEndDrag()
        {
            _isDragging = false;
            if (Parent != null)
                Parent.OnEndDrag();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            // Stop the method in the drag state.
            if (_isDragging)
                return;

            // Body must be performed only in the deepest child that never has children.
            if (Children.Count == 0)          
                ProcessPointerMove(eventData);
        }

        private void ProcessPointerMove(PointerEventData eventData)
        {
            if (Parent != null)
                Parent.ProcessPointerMove(eventData);

            Vector2 localPosition = eventData.PointerDataToRelativePos(RectTransform);

            _isPointerOnBorder = false;
            ClosestBorder = Border.None;
            _canBeDragged = false;

            if (Parent == null)
            {
                if (localPosition.x < Constants.BorderDetectionThreshold)
                {
                    _isPointerOnBorder = true;
                    ClosestBorder = Border.Left;
                }
                else if (localPosition.x > RectTransform.rect.width - Constants.BorderDetectionThreshold)
                {
                    _isPointerOnBorder = true;
                    ClosestBorder = Border.Right;
                }
                else if (localPosition.y < Constants.BorderDetectionThreshold)
                {
                    _isPointerOnBorder = true;
                    ClosestBorder = Border.Bottom;
                }
                else if (localPosition.y > RectTransform.rect.height - Constants.BorderDetectionThreshold)
                {
                    _isPointerOnBorder = true;
                    ClosestBorder = Border.Top;
                }
            }
            else
            {
                if (localPosition.x < Constants.BorderDetectionThreshold)
                {
                    _isPointerOnBorder = true;
                    ClosestBorder = Border.Left;
                }
                else if (localPosition.x > RectTransform.rect.width - Constants.BorderDetectionThreshold)
                {
                    _isPointerOnBorder = true;
                    ClosestBorder = Border.Right;
                }
                else if (localPosition.y < Constants.BorderDetectionThreshold)
                {
                    _isPointerOnBorder = true;
                    ClosestBorder = Border.Bottom;
                }
                else if (localPosition.y > RectTransform.rect.height - Constants.BorderDetectionThreshold)
                {
                    _isPointerOnBorder = true;
                    ClosestBorder = Border.Top;
                }

                _canBeDragged = true;

                if (Parent.Layout == LayoutType.Horizontal)
                {
                    if (Parent[1] == this && localPosition.x < Constants.BorderDetectionThreshold ||
                        Parent[0] == this && localPosition.x > RectTransform.rect.width - Constants.BorderDetectionThreshold)
                        return;
                }
                else if (Parent.Layout == LayoutType.Vertical)
                {
                    if (Parent[0] == this && localPosition.y < Constants.BorderDetectionThreshold ||
                        Parent[1] == this && localPosition.y > RectTransform.rect.height - Constants.BorderDetectionThreshold)
                        return;
                }
            }

            _canBeDragged = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //_isDragging = false;
            Selected = null;
            _isPointerOnBorder = false;
            ClosestBorder = Border.None;

            if(Parent != null)
                Parent.OnPointerExit(eventData);
        }

        public void LoadCenterNormalizedPosition(float value)
        {
            CenterNormalizedPosition = value;
        }

        public Vector2 GetNormalizedSize()
        {
            Vector2 size = new Vector2
            {
                x = (float)RectTransform.rect.width / WindowsManager.RectTransform.rect.width,
                y = (float)RectTransform.rect.height / WindowsManager.RectTransform.rect.height
            };

            return size;
        }

        public void LoadNormalizedSize(Vector2 value)
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

        public void LoadNormalizedPosition(Vector2 position)
        {
            RectTransform.localPosition = new Vector3(position.x * WindowsManager.RectTransform.rect.width, position.y * WindowsManager.RectTransform.rect.height, 0);
        }

        public void LoadChildren(List<WindowController> children)
        {
            Children.AddRange(children);
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Parent = this;
                children[i].Process();
            }

            CreateActions(children[0].Actions);
        }

        public void LoadLayoutGroup(LayoutType type)
        {
            Layout = type;

            HorizontalOrVerticalLayoutGroup layout;
            if (type == LayoutType.Horizontal)
                layout = gameObject.AddComponent<HorizontalLayoutGroup>();

            else
                layout = gameObject.AddComponent<VerticalLayoutGroup>();

            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childScaleWidth = true;
            layout.childScaleHeight = true;
           
            UpdateLayouts(this);
        }

        private void OnValidate()
        {
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();

            if (Background == null)
                Background = GetComponent<Image>();

            Background.raycastPadding = Vector4.one * Constants.BorderDetectionThreshold;

            if (LayoutElement == null)
                LayoutElement = GetComponent<LayoutElement>();
        }
    }
}