namespace SimpleWindow.Examples
{
    using System.Collections.Generic;

    using UnityEngine;

    public class ExampleManager : MonoBehaviour
    {
        public WindowsManager Manager;
        public List<Window> Windows = new List<Window>();

        public RectTransform Parent;

        // Methods

        public void AddHierarchy()
        {
            WindowsManager.CreateWindowController<ExampleHierarchyWindow>();
        }

        public void AddInspector()
        {
            WindowsManager.CreateWindowController<ExampleInspectorWindow>();
        }
    }
}