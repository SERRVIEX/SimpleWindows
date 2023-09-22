namespace SimpleWindow
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using System.Runtime.Serialization.Formatters.Binary;

    using UnityEngine;
    using UnityEngine.UI;

    using SimpleWindow.Internal;
    using SimpleWindow.Serialization;

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

        private LayoutData _currentLayout;
        private List<LayoutData> _layouts = new List<LayoutData>();

        public static string Path
        {
            get
            {
                if (string.IsNullOrEmpty(_path))
                    _path = $"{Application.persistentDataPath}/Windows";

                return _path;
            }
        }

        public static string _path;

        private bool _markedDirty;
        private static bool _isLoading;

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

        private void Update()
        {
            if(_markedDirty)
                Save();
        }

        /// <summary>
        /// Create a window with the specified type.
        /// </summary>
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
                    bool isFloating = GetUnderlayerWindowCount() > 0;
                    WindowController controller = CreateWindowController(Vector3.zero, isFloating);
                    Window instance = Instantiate(item, controller.Content);
                    controller.Link(instance);

                    MarkDirty();

                    return (T)instance;
                }
            }

            throw new Exception($"Window '{typeof(T)}' wasn't found in the manager.");
        }

        /// <summary>
        /// Create a window with the specified type.
        /// </summary>
        public static Window CreateWindow(Type type)
        {
            return Instance.CreateWindowImpl(type);
        }

        private Window CreateWindowImpl(Type type)
        {
            for (int i = 0; i < _windows.Count; i++)
            {
                Window item = _windows[i];
                if (item.GetType() == type)
                {
                    bool isFloating = GetUnderlayerWindowCount() > 0;
                    WindowController controller = CreateWindowController(Vector3.zero, isFloating);
                    Window instance = Instantiate(item, controller.Content);
                    controller.Link(instance);

                    MarkDirty();

                    return instance;
                }
            }

            throw new Exception($"Window '{type}' wasn't found in the manager.");
        }

        private static Window CreateWindow(Type type, WindowController controller)
        {
            return Instance.CreateWindowImpl(type, controller);
        }

        private Window CreateWindowImpl(Type type, WindowController controller)
        {
            for (int i = 0; i < _windows.Count; i++)
            {
                Window item = _windows[i];
                if (item.GetType() == type)
                {
                    Window instance = Instantiate(item, controller.Content);
                    controller.Link(instance);

                    return instance;
                }
            }

            throw new Exception($"Window '{type}' wasn't found in the manager.");
        }

        /// <summary>
        /// Create new window controller for dragging item.
        /// </summary>
        public static WindowController CreateWindowController(TabView item)
        {
            bool isFloating = GetUnderlayerWindowCount() > 0;
            WindowController controller = CreateWindowController(item.transform.localPosition, isFloating);
            item.Window.transform.SetParent(controller.Content);
            item.Window.transform.localPosition = Vector3.zero;
            controller.Link(item);

            MarkDirty();

            return controller;
        }

        private static WindowController CreateWindowController(Vector3 localPosition, bool isFloating)
        {
            return Instance.CreateWindowControllerImpl(localPosition, isFloating);
        }

        private WindowController CreateWindowControllerImpl(Vector3 localPosition, bool isFloating)
        {
            WindowController controller = Instantiate(_windowPrefab, isFloating ? Overlayer : Underlayer);
            controller.RectTransform.SetPivot(Pivot.MiddleCenter);
            controller.RectTransform.SetAnchor(Anchor.MiddleCenter);

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

            return controller;
        }

        /// <summary>
        /// Destroy a window controller.
        /// </summary>
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

            if (controller != null)
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

        /// <summary>
        /// Destroy a window controller by tab view.
        /// </summary>
        public static void Destroy(TabView tabView)
        {
            if (tabView.Header.Tabs.Count == 0)
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

        public static int GetUnderlayerWindowCount()
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

        public static string GetCurrentLayout()
        {
            return Instance._currentLayout.Name;
        }

        public static List<string> GetLayouts()
        {
            return Instance.GetLayoutsImpl();
        }

        public static int GetLayoutCount()
        {
            return Instance._layouts.Count;
        }

        private List<string> GetLayoutsImpl()
        {
            List<string> layouts = new List<string>();
            for (int i = 0; i < _layouts.Count; i++)
                layouts.Add(_layouts[i].Name);
            return layouts;
        }

        public static void RemoveLayout(string name)
        {
            Instance.RemoveLayoutImpl(name);
        }

        private void RemoveLayoutImpl(string name)
        {
            // Don't allow to remove the last layout.
            if (_layouts.Count <= 1)
                return;

            for (int i = 0; i < _layouts.Count; i++)
            {
                LayoutData layout = _layouts[i];
                if (layout.Name == name)
                {
                    _layouts.RemoveAt(i);

                    // If the removed layout is also selected then a new layout have to be loaded.
                    if (_currentLayout == layout)
                    {
                        _currentLayout = _layouts[0];
                        LoadLayout(_currentLayout);
                    }

                    return;
                }
            }
        }

        public static void MarkDirty()
        {
            if (_isLoading)
                return;

            Instance._markedDirty = true;
        }

        /// <summary>
        /// Restore default layouts.
        /// </summary>
        public static void Restore()
        {
            Instance.RestoreImpl();
        }

        private void RestoreImpl()
        {
            string streamingPath = $"{Application.streamingAssetsPath}/Windows";
            if (!Directory.Exists(streamingPath))
                return;

            string managerPath = $"{streamingPath}/Manager.data";
            if (!File.Exists(managerPath))
                return;

            BinaryFormatter managerBinaryFormatter = new BinaryFormatter();
            using FileStream managerFileStream = File.Open(managerPath, FileMode.Open);
            ManagerData managerData = (ManagerData)managerBinaryFormatter.Deserialize(managerFileStream);

            for (int i = 0; i < managerData.Layouts.Count; i++)
            {
                string originLayoutPath = $"{streamingPath}/Layouts/{managerData.Layouts[i]}.layout";
                string targetLayoutPath = $"{Path}/Layouts/{managerData.Layouts[i]}.layout";
                if (File.Exists(originLayoutPath))
                {
                    if (File.Exists(targetLayoutPath))
                        File.Delete(targetLayoutPath);

                    File.Copy(originLayoutPath, targetLayoutPath);

                    BinaryFormatter layoutBinaryFormatter = new BinaryFormatter();
                    using FileStream layoutFileStream = File.Open(targetLayoutPath, FileMode.Open);
                    LayoutData layoutData = (LayoutData)layoutBinaryFormatter.Deserialize(layoutFileStream);
                    if (layoutData != null && layoutData.Name == managerData.CurrentLayout)
                        LoadLayout(layoutData);
                }
            }
        }

        private void Save()
        {
            _markedDirty = false;

            Directory.CreateDirectory($"{Path}");
            Directory.CreateDirectory($"{Path}/Layouts");

            ManagerData managerData = new ManagerData(_currentLayout, _layouts);

            if (_currentLayout != null)
                SaveLayoutImpl(_currentLayout.Name);

            string managerPath = $"{Path}/Manager.data";
            var bf = new BinaryFormatter();
            using FileStream fs = File.Open(managerPath, FileMode.OpenOrCreate, FileAccess.Write);
            bf.Serialize(fs, managerData);
        }

        private void Load()
        {
            if (!Directory.Exists(Path))
            {
                string streamingPath = $"{Application.streamingAssetsPath}/Windows";
                if (!Directory.Exists(streamingPath))
                    return;

                DirectoryExtensions.Copy(streamingPath, Path);
            }

            Directory.CreateDirectory($"{Path}");
            Directory.CreateDirectory($"{Path}/Layouts");

            string managerPath = $"{Path}/Manager.data";
            if (!File.Exists(managerPath))
                return;

            _isLoading = true;

            BinaryFormatter managerBinaryFormatter = new BinaryFormatter();
            using FileStream managerFileStream = File.Open(managerPath, FileMode.Open);
            ManagerData managerData = (ManagerData)managerBinaryFormatter.Deserialize(managerFileStream);

            for (int i = 0; i < managerData.Layouts.Count; i++)
            {
                LayoutData layoutData = null;

                string layoutPath = $"{Path}/Layouts/{managerData.Layouts[i]}.layout";
                if (File.Exists(layoutPath))
                {
                    BinaryFormatter layoutBinaryFormatter = new BinaryFormatter();
                    using FileStream layoutFileStream = File.Open(layoutPath, FileMode.Open);
                    layoutData = (LayoutData)layoutBinaryFormatter.Deserialize(layoutFileStream);
                    _layouts.Add(layoutData);
                }
                else
                    continue;

                if (layoutData != null && layoutData.Name == managerData.CurrentLayout)
                    LoadLayout(layoutData);
            }

            _isLoading = false;
        }

        public static void SaveLayout(string name)
        {
            Instance.SaveLayoutImpl(name);
            Instance.Save();

            Debug.Log($"Layout '{name}' saved.");
        }

        private void SaveLayoutImpl(string name)
        {
            LayoutData layoutData = null;

            // If layout with the input name already exists then override it.
            for (int i = 0; i < _layouts.Count; i++)
            {
                if (_layouts[i].Name == name)
                {
                    layoutData = new LayoutData(name, _controllers.Where(item => item.Parent == null).ToList());
                    _layouts[i] = layoutData;
                    _currentLayout = layoutData;
                }
            }

            // Create new layout with the input name.
            if (layoutData == null)
            {
                layoutData = new LayoutData(name, _controllers.Where(item => item.Parent == null).ToList());
                _currentLayout = layoutData;
                _layouts.Add(layoutData);
            }

            string resultPath = $"{Path}/Layouts/{name}.layout";
            var bf = new BinaryFormatter();
            using FileStream fs = File.Open(resultPath, FileMode.OpenOrCreate, FileAccess.Write);
            bf.Serialize(fs, layoutData);
        }

        public static void LoadLayout(string name)
        {
            Instance.LoadLayoutImpl(name);
        }

        private void LoadLayoutImpl(string name)
        {
            // If a layout with the input name already exists then override it.
            for (int i = 0; i < _layouts.Count; i++)
            {
                if (_layouts[i].Name == name)
                {
                    LoadLayout(_layouts[i]);
                    return;
                }
            }
        }

        private void LoadLayout(LayoutData layoutData)
        {
            _currentLayout = layoutData;

            // Destroy the current layout.
            for (int i = 0; i < _controllers.Count; i++)
                if (_controllers[i] != null)
                    DestroyImmediate(_controllers[i].gameObject);

            _controllers.Clear();

            // Load the new layout.
            for (int i = 0; i < layoutData.Windows.Count; i++)
                LoadWindow(layoutData.Windows[i], null);
        }

        private WindowController LoadWindow(WindowData windowData, WindowController parent)
        {
            Transform transformParent;
            if (parent == null)
                transformParent = windowData.IsFloating ? Overlayer : Underlayer;
            else
                transformParent = parent.transform;

            WindowController controller = Instantiate(_windowPrefab, transformParent);
            controller.RectTransform.SetPivot(Pivot.MiddleCenter);
            controller.RectTransform.SetAnchor(Anchor.MiddleCenter);
            _controllers.Add(controller);

            controller.IsFloating = windowData.IsFloating;

            if (windowData.Children == null || windowData.Children.Count == 0)
            {
                // Create the root window from the first tabs.
                Window rootWindow = CreateWindow(windowData.Tabs[0].Type, controller);

                // Add the left tabs to the root window.
                for (int i = 1; i < windowData.Tabs.Count; i++)
                {
                    TabData tabData = windowData.Tabs[i];
                    CreateWindow(tabData.Type, controller);
                }

                for (int i = 0; i < windowData.Tabs.Count; i++)
                {
                    if (windowData.Tabs[i].Active)
                    {
                        rootWindow.Controller.Header.Select(i);
                        break;
                    }
                }

                // If the window is under layer then we have to set the center normalized position.
                if (!windowData.IsFloating)
                    rootWindow.Controller.LoadCenterNormalizedPosition(windowData.CenterNormalizedPosition);

                // If the window is floating when we have to set the size and the position.
                else
                {
                    rootWindow.Controller.LoadNormalizedSize(new Vector2(windowData.NormalizedSizeX, windowData.NormalizedSizeY));
                    rootWindow.Controller.LoadNormalizedPosition(new Vector2(windowData.NormalizedPositionX, windowData.NormalizedPositionY));
                }
            }
            else
            {
                Destroy(controller.Header.gameObject);
                Destroy(controller.Content.gameObject);
                Destroy(controller.FrontLayer.gameObject);

                List<WindowController> children = new List<WindowController>();
                for (int i = 0; i < windowData.Children.Count; i++)
                    children.Add(LoadWindow(windowData.Children[i], controller));

                // If the window is under layer then we have to set the center normalized position.
                if (!windowData.IsFloating)
                    controller.LoadCenterNormalizedPosition(windowData.CenterNormalizedPosition);

                // If the window is floating when we have to set the size and the position.
                else
                {
                    controller.LoadNormalizedSize(new Vector2(windowData.NormalizedSizeX, windowData.NormalizedSizeY));
                    controller.LoadNormalizedPosition(new Vector2(windowData.NormalizedPositionX, windowData.NormalizedPositionY));
                }

                controller.LoadChildren(children);
                controller.LoadLayoutGroup(windowData.LayoutType);
            }

            controller.Process();
            return controller;
        }

        public static void LoadLayouts()
        {
            Instance.Load();
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
    }
}