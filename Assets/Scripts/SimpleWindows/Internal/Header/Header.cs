namespace SimpleWindow.Internal
{
    using System.Linq;
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(ScrollRect))]
    public sealed class Header : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// The object on which the cursor is located.
        /// </summary>
        public static Header Focused { get; private set; }

        /// <summary>
        /// Reference to the window.
        /// </summary>
        [field: SerializeField] public WindowController Controller { get; set; }

        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] public ScrollRect ScrollRect { get; private set; }
        [SerializeField] private TabView _tabPrefab;

        public List<TabView> Tabs = new List<TabView>();

        // Constructors

        private Header() { }

        // Methods

        private void Awake() => OnValidate();

        public void AddTab(TabView item)
        {
            if (item.Header != null && item.Header != this)
                item.Header.RemoveTab(item);

            item.Header = this;
            if (!Tabs.Contains(item))
                Tabs.Add(item);

            OrderBySiblingIndex();
            item.transform.SetParent(ScrollRect.content);
        }

        public void RemoveTab(TabView item)
        {
            Header previousRoot = item.Header;

            item.Header = null;
            Tabs.Remove(item);
            OrderBySiblingIndex();
            item.transform.SetParent(WindowsManager.RectTransform);

            bool wasActive = item.Active;

            item.OnEndDragHandler.AddListener(() =>
            {
                if (wasActive && previousRoot != item.Header)
                    previousRoot.Select(0);

                item.OnEndDragHandler.RemoveAllListeners();

                if (Controller.Parent != null && Controller.Parent.Children.Count == 0)
                {
                    if (Tabs.Count == 0)
                        WindowsManager.Destroy(Controller);
                }
                else
                {
                    if (Tabs.Count == 0)
                        WindowsManager.Destroy(Controller);
                }
            });
        }

        public void AddWindow(Window window)
        {
            TabView tabView = Instantiate(_tabPrefab, ScrollRect.content);
            tabView.Link(window);
            tabView.Header = this;

            window.transform.SetParent(Controller.Content);
            Tabs.Add(tabView);

            Select(Tabs[Tabs.Count - 1]);
        }

        public void Select(TabView item)
        {
            int index = Tabs.IndexOf(item);
            Select(index);
        }

        public void Select(int index)
        {
            if (Tabs.Count == 0)
                return;

            for (int i = 0; i < Tabs.Count; i++)
                Tabs[i].Active = false;

            Tabs[index].Active = true;

            WindowsManager.MarkDirty();
        }

        public Window GetWindow(WindowController controller)
        {
            for (int i = 0; i < Tabs.Count; i++)
                if (Tabs[i].Window.Controller == controller)
                    return Tabs[i].Window;

            return null;
        }

        public void OrderBySiblingIndex() => Tabs.OrderBy(item => item.transform.GetSiblingIndex());

        public void OnPointerEnter(PointerEventData eventData)
        {
            Focused = this;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Focused = null;
        }

        private void OnValidate()
        {
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();

            RectTransform.SetAnchor(Anchor.StretchTop);
            RectTransform.SetPivot(Pivot.TopCenter);

            if (ScrollRect == null)
                ScrollRect = GetComponent<ScrollRect>();

            name = GetType().Name;
        }
    }
}