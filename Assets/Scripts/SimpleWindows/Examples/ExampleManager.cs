namespace SimpleWindow.Examples
{
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.UI;

    using TMPro;

    public class ExampleManager : MonoBehaviour
    {
        public WindowsManager Manager;
        public List<Window> Windows = new List<Window>();

        public Transform LayoutsParent;

        // Methods

        private void Start()
        {
            var save = LayoutsParent.GetChild(0).GetComponent<Button>();
            save.onClick.AddListener(() =>
            {
                string layout = "Layout " + WindowsManager.GetLayouts().Count;
                CreateLayoutButton(layout);
                WindowsManager.SaveLayout(layout);
            });

            var layouts = WindowsManager.GetLayouts();
            foreach (var layout in layouts)
                CreateLayoutButton(layout);
        }

        private void CreateLayoutButton(string layoutName)
        {
            var obj = Instantiate(LayoutsParent.GetChild(0), LayoutsParent).gameObject;
            obj.name = layoutName;
            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = layoutName;
            var button = obj.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                WindowsManager.LoadLayout(layoutName);
            });
        }

        public void AddHierarchy()
        {
            WindowsManager.CreateWindow<ExampleHierarchyWindow>();
        }

        public void AddInspector()
        {
            WindowsManager.CreateWindow<ExampleInspectorWindow>();
        }
    }
}