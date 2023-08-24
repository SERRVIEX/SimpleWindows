namespace SimpleWindow
{
    using System;
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.UI;

    using SimpleWindow.Internal;

    public sealed class WindowsManager : MonoBehaviour
    {
        public static WindowsManager Instance { get; private set; }

        /// <summary>
        /// Aspect ratio factor between canvas and screen resolution.
        /// </summary>
        public static float AspectRatioFactor { get; private set; }

        public static Camera Camera => Instance._camera;
        [SerializeField] private Camera _camera;

        public static RectTransform RectTransform => Instance._rectTransform;
        [SerializeField] private RectTransform _rectTransform;

        public static CanvasScaler CanvasScaler => Instance._canvasScaler;
        [SerializeField] private CanvasScaler _canvasScaler;

        public static RectTransform Underlayer => Instance._underlayer;
        [SerializeField, Space(10)] private RectTransform _underlayer;

        public static RectTransform Overlayer => Instance._overlayer;
        [SerializeField] private RectTransform _overlayer;

        public static RectOffset Margin { get; private set; }
        [SerializeField, Space(10)] private RectOffset _padding;

        /// <summary>
        /// Window template.
        /// </summary>
        [SerializeField, Space(10)] private WindowController _windowPrefab;

        /// <summary>
        /// All available windows that can be created.
        /// </summary>
        [SerializeField] private List<Window> _windows;

        /// <summary>
        /// All active window controllers.
        /// </summary>
        private List<WindowController> _controllers = new List<WindowController>();

        // Constructors

        private WindowsManager() { }

        // Methods

        private void Awake()
        {
            Instance = this;

            Margin = new RectOffset(left: -(int)RectTransform.rect.width / 2 + _padding.left,
                                    right: (int)RectTransform.rect.width / 2 - _padding.right,
                                    top: (int)RectTransform.rect.height / 2 - _padding.top,
                                    bottom: -(int)RectTransform.rect.height / 2 + _padding.bottom);

            AspectRatioFactor = CanvasScaler.referenceResolution.x / Screen.width;
        }

        private void OnValidate()
        {
            name = GetType().Name;

            // Configure the underlayer.
            _underlayer.SetAnchor(Anchor.Stretch);
            _underlayer.SetPivot(Pivot.MiddleCenter);
            _underlayer.SetOffset(_padding.left, _padding.right, _padding.top, _padding.bottom);

            // Configure the overlayer.
            _overlayer.SetAnchor(Anchor.Stretch);
            _overlayer.SetPivot(Pivot.MiddleCenter);
            _overlayer.SetOffset(0, 0, 0, 0);
        }

        /// <summary>
        /// Create a new window.
        /// </summary>
        /// <typeparam name="T">Type of window.</typeparam>
        public static T CreateWindow<T>() where T : Window
        {
            return Instance.CreateWindowImpl<T>();
        }

        private T CreateWindowImpl<T>() where T : Window
        {
            for (int i = 0; i < _windows.Count; i++)
            {
                Window item = _windows[i];
                if (item.GetType() == typeof(T))
                {
                    bool isFloating = GetStaticWindowCount() > 0;
                    WindowController controller = CreateWindowController(Vector3.zero, isFloating);
                    Window instance = Instantiate(item, controller.Content);
                    controller.Link(instance);

                    return (T)instance;
                }
            }

            throw new Exception($"Window '{typeof(T)}' wasn't found in the manager.");
        }

        /// <summary>
        /// Create new window controller for dragging item.
        /// </summary>
        public static WindowController CreateWindowController(TabView item)
        {
            bool isFloating = GetStaticWindowCount() > 0;
            WindowController controller = CreateWindowController(item.transform.localPosition, isFloating);
            item.Window.transform.SetParent(controller.Content);
            item.Window.transform.localPosition = Vector3.zero;
            controller.Link(item);

            MarkDirty();

            return controller;
        }

        /// <summary>
        /// Create new window controller at specified position.
        /// </summary>
        private static WindowController CreateWindowController(Vector3 localPosition, bool isFloating)
        {
            return Instance.CreateWindowControllerImpl(localPosition, isFloating);
        }

        private WindowController CreateWindowControllerImpl(Vector3 localPosition, bool isFloating)
        {
            WindowController controller = Instantiate(_windowPrefab, isFloating ? Overlayer : Underlayer);
            controller.RectTransform.SetPivot(Pivot.MiddleCenter);
            controller.RectTransform.SetAnchor(Anchor.MiddleCenter);

            controller.IsRoot = true;
            controller.IsFloating = isFloating;

            if (isFloating)
            {
                controller.RectTransform.SetSize(400, 400);
                localPosition.x = Mathf.Clamp(localPosition.x, Margin.left + controller.RectTransform.rect.width / 2f, Margin.right - controller.RectTransform.rect.width / 2f);
                localPosition.y = Mathf.Clamp(localPosition.y, Margin.bottom + controller.RectTransform.rect.height / 2f, Margin.top - controller.RectTransform.rect.height / 2f);
                localPosition.z = 0;

                controller.RectTransform.localPosition = localPosition;
            }

            _controllers.Add(controller);

            MarkDirty();

            return controller;
        }

        public static void Destroy(WindowController controller)
        {
            Instance.DestroyImpl(controller);
        }

        private void DestroyImpl(WindowController controller)
        {
            if (controller.Parent != null)
                controller.Parent.Detach(controller);

            RemoveRecursive(controller);

            Instance._controllers.Remove(controller);
            DestroyImmediate(controller.gameObject);

            MarkDirty();
        }

        private void RemoveRecursive(WindowController controller)
        {
            for (int i = 0; i < controller.Children.Count; i++)
            {
                _controllers.Remove(controller.Children[i]);
                RemoveRecursive(controller.Children[i]);
            }
        }

        public static void Destroy(TabView tabView)
        {
            if(tabView.Header.Tabs.Count == 0)
                Destroy(tabView.Header.Controller);
            else
            {
                tabView.Header.RemoveTab(tabView);
                Destroy(tabView.Window.gameObject);
            }
        }

        public static bool IsPointerInsideTheWindows()
        {
            for (int i = 0; i < Instance._controllers.Count; i++)
                if (RectTransformUtility.RectangleContainsScreenPoint(Instance._controllers[i].RectTransform, Input.mousePosition, Instance._camera))
                    return true;

            return false;
        }

        public static void SetBoundsControllersActive(bool value)
        {
            Instance.SetBoundsControllersActiveImpl(value);
        }

        private void SetBoundsControllersActiveImpl(bool value)
        {
            for (int i = 0; i < _controllers.Count; i++)
                _controllers[i].SetBoundsControllersActive(value);
        }

        public static int GetWindowCount()
        {
            return Instance._controllers.Count;
        }

        public static int GetStaticWindowCount()
        {
            return Instance.GetStaticWindowCountImpl();
        }

        private int GetStaticWindowCountImpl()
        {
            int count = 0;
            for (int i = 0; i < _controllers.Count; i++)
                if (!_controllers[i].IsFloating)
                    count++;
            return count;
        }

        public static void MarkDirty() { }
    }
}