namespace SimpleWindow
{
    using System;
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.UI;

    public sealed class WindowsManager : MonoBehaviour
    {
        public static WindowsManager Instance { get; private set; }

        public static RectTransform RectTransform => Instance._rectTransform;
        [SerializeField] private RectTransform _rectTransform;

        public static RectTransform Underlayer => Instance._underlayer;
        [SerializeField] private RectTransform _underlayer;

        public static RectTransform Overlayer => Instance._overlayer;
        [SerializeField] private RectTransform _overlayer;

        public static RectOffset Margin { get; private set; }

        public static Camera Camera => Instance._camera;
        [SerializeField] private Camera _camera;

        public static CanvasScaler CanvasScaler => Instance._canvasScaler;
        [SerializeField] private CanvasScaler _canvasScaler;

        public static float AspectRatio => CanvasScaler.referenceResolution.x / Screen.width;

        [SerializeField] private WindowController _windowPrefab;
        [SerializeField] private List<Window> _windows;

        private List<WindowController> _controllers = new List<WindowController>();

        // Methods

        private void Awake()
        {
            Instance = this;

            int padding = 0;

            Margin = new RectOffset(
                                    left: -(int)RectTransform.rect.width / 2 + padding,
                                    right: (int)RectTransform.rect.width / 2 - padding,
                                    top: (int)RectTransform.rect.height / 2 - padding - 40, // 40 MenuBar
                                    bottom: -(int)RectTransform.rect.height / 2 + padding);

            name = GetType().ToString();
        }

        private void OnValidate() => name = GetType().ToString();

        /// <summary>
        /// Get mouse position in root rect transform.
        /// </summary>
        public static Vector3 GetMousePosition() => Input.mousePosition - new Vector3(RectTransform.rect.width / 2f, RectTransform.rect.height / 2f);

        public static T CreateWindowController<T>() where T : Window
        {
            return Instance.CreateWindowControllerImpl<T>();
        }

        private T CreateWindowControllerImpl<T>() where T : Window
        {
            for (int i = 0; i < _windows.Count; i++)
            {
                Window item = _windows[i];
                if (item.GetType() == typeof(T))
                {
                    bool isFloating = GetStaticWindowCount() > 0;
                    WindowController controller = CreateWindow(Vector3.zero, isFloating);
                    Window instance = Instantiate(item, controller.Content);
                    controller.Link(instance);

                    return (T)instance;
                }
            }

            throw new Exception($"Window '{typeof(T)}' wasn't found in the manager.");
        }

        /// <summary>
        /// Create new window for dragging item.
        /// </summary>
        public static WindowController CreateWindow(TabView item)
        {
            bool isFloating = GetStaticWindowCount() > 0;
            WindowController controller = CreateWindow(item.transform.localPosition, isFloating);
            item.Window.transform.SetParent(controller.Content);
            item.Window.transform.localPosition = Vector3.zero;
            controller.Link(item);

            MarkDirty();

            return controller;
        }

        /// <summary>
        /// Create new window at specified position.
        /// </summary>
        private static WindowController CreateWindow(Vector3 localPosition, bool isFloating)
        {
            return Instance.CreateWindowImpl(localPosition, isFloating);
        }

        private WindowController CreateWindowImpl(Vector3 localPosition, bool isFloating)
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

        public static void DestroyController(WindowController window)
        {
            Instance._controllers.Remove(window);
            Destroy(window.gameObject);

            MarkDirty();
        }
       

        /// <summary>
        /// Remove specified instance type.
        /// </summary>
        public static void Destroy<T>() where T : Window
        {
            for (int i = 0; i < Instance._controllers.Count; i++)
                if (Instance._controllers[i].Header.FindControllerOfType<T>(out TabView tabView))
                    Instance._controllers[i].Header.DestroyTab(tabView);

            MarkDirty();
        }

        public static bool IsPointerInsideTheWindows()
        {
            for (int i = 0; i < Instance._controllers.Count; i++)
                if (RectTransformUtility.RectangleContainsScreenPoint(Instance._controllers[i].RectTransform, Input.mousePosition, Instance._camera))
                    return true;

            return false;
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