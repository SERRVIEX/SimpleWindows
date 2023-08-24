namespace SimpleWindow.Examples
{
    using System.Collections.Generic;

    using UnityEngine;

    public class ExampleManager : MonoBehaviour
    {
        public WindowsManager Manager;
        public List<Window> Windows = new List<Window>();

        // Methods

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