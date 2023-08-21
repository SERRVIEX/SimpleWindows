namespace SimpleWindow
{
    using System.Linq;
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using static UnityEditor.Progress;
    using TMPro.EditorUtilities;

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
        [field: SerializeField] public WindowController Window { get; set; }

        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] public ScrollRect ScrollRect { get; private set; }
        [SerializeField] private TabView _tabPrefab;

        public List<TabView> _tabs = new List<TabView>();
        public int TabCount => _tabs.Count;

        // Methods

        private void Awake() => OnValidate();

        public void AddTab(TabView item)
        {
            if (item.Header != null && item.Header != this)
                item.Header.RemoveTab(item);
            
            item.Header = this;
            if(!_tabs.Contains(item))
                _tabs.Add(item);

            OrderBySiblingIndex();
            item.transform.SetParent(ScrollRect.content);
        }

        public void RemoveTab(TabView item)
        {
            Header previousRoot = item.Header;

            item.Header = null;
            _tabs.Remove(item);
            OrderBySiblingIndex();
            item.transform.SetParent(WindowsManager.RectTransform);

            bool wasActive = item.Active;

            item.OnEndDragHandler.AddListener(() =>
            {
                if (wasActive)
                    previousRoot.Select(0);

                item.OnEndDragHandler.RemoveAllListeners();

                if (Window.Parent != null && Window.Parent.ChildCount == 0)
                {
                    if (_tabs.Count == 0)
                        WindowsManager.DestroyController(Window);
                }
                else
                {
                    if (_tabs.Count == 0)
                        WindowsManager.DestroyController(Window);
                }
            });
        }

        public void AddController(Window controller)
        {
            TabView tabView = Instantiate(_tabPrefab, ScrollRect.content);
            tabView.Link(controller);
            tabView.Header = this;

            controller.transform.SetParent(Window.Content);
            _tabs.Add(tabView);

            Select(_tabs[_tabs.Count - 1]);
        }

        public void Select(TabView item)
        {
            int index = _tabs.IndexOf(item);
            Select(index);
        }

        private void Select(int index)
        {
            if (_tabs.Count == 0)
                return;

            for (int i = 0; i < _tabs.Count; i++)
                _tabs[i].Active = false;

            _tabs[index].Active = true;

            WindowsManager.MarkDirty();
        }

        public void DestroyTab(TabView item)
        {
            _tabs.Remove(item);

            Destroy(item.Window.gameObject);
            Destroy(item.gameObject);

            if (_tabs.Count == 0)
                WindowsManager.DestroyController(Window);
        }

        public bool FindControllerOfType<T>(out TabView item) where T : Window
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                if (_tabs[i].Window.GetType() == typeof(T))
                {
                    item = _tabs[i];
                    return true;
                }
            }

            item = null;
            return false;
        }

        public Window GetWindow(WindowController controller)
        {
            for (int i = 0; i < _tabs.Count; i++)
                if (_tabs[i].Window.WindowController == controller)
                    return _tabs[i].Window;

            return null;
        }

        public void OrderBySiblingIndex() => _tabs.OrderBy(a => a.transform.GetSiblingIndex());

        public void OnPointerEnter(PointerEventData eventData)
        {
            Focused = this;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Focused = null;
        }

        public string GetName()
        {
            string value = string.Empty;

            for (int i = 0; i < _tabs.Count; i++)
            {
                if (_tabs[i].Window != null)
                    value += _tabs[i].Window.GetType().Name + " | ";
            }
            if(value.Length > 3)
            value = value.Substring(0, value.Length - 2);
            return value;
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