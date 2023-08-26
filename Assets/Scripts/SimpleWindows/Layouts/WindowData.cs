namespace SimpleWindow.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using UnityEngine;

    using SimpleWindow.Internal;
    using UnityEngine.VFX;
    using System.IO;

    [DataContract]
    public class WindowData
    {
        [DataMember] public bool IsFloating { get;private set; }
        [DataMember] public List<TabData> Tabs { get; private set; }

        [DataMember] public Vector3 NormalizedSize { get; private set; }
        [DataMember] public Vector2 NormalizedPosition { get; private set; }

        [DataMember] public LayoutType LayoutType { get; private set; }
        [DataMember] public float CenterNormalizedPosition { get; private set; }

        [DataMember] public WindowData Parent { get; private set; }
        [DataMember] public List<WindowData> Children { get; private set; }

        // Constructors

        public WindowData(WindowController controller)
        {
            IsFloating = controller.IsFloating;
            
            // If the controller is not a group.
            if (controller.Children.Count == 0)
            {
                // If the controller is root and is floating then we need to
                // save the position and the size of this window.
                if (controller.IsRoot() && IsFloating)
                {
                    NormalizedSize = controller.GetNormalizedSize();
                    NormalizedPosition = controller.GetNormalizedPosition();
                }

                // Get the tabs that are linked to the window.
                Tabs = new List<TabData>();
                for (int i = 0; i < controller.Header.Tabs.Count; i++)
                    Tabs.Add(new TabData(controller.Header.Tabs[i]));
            }
            // If the controller is a group.
            else
            {
                // If the controller is root and is floating then we need to
                // save the position and the size of this window.
                if (controller.IsRoot() && IsFloating)
                {
                    NormalizedPosition = controller.GetNormalizedPosition();
                    NormalizedSize = controller.GetNormalizedSize();
                }

                LayoutType = controller.Layout;
                // Save the center normalized position of a group.
                CenterNormalizedPosition = controller.CenterNormalizedPosition;
            }

            // Collect data from the children.
            if (controller.Children.Count > 0)
            {
                Children = new List<WindowData>();
                for (int i = 0; i < controller.Children.Count; i++)
                {
                    var child = new WindowData(controller.Children[i]);
                    child.Parent = this;
                    Children.Add(child);
                }
            }
        }
    }

    [DataContract]
    public class TabData
    {
        [DataMember] private string _typeName;
        [DataMember] public bool Active { get; private set; }

        public Type Type
        {
            get => Type.GetType(_typeName);
            private set => _typeName = value.FullName;
        }

        // Constructors

        public TabData(TabView tab)
        {
            Type = tab.Window.GetType();
        }
    }

    [DataContract]
    public class LayoutData
    {
        [DataMember] public List<WindowData> Windows { get; private set; }

        // Constructors

        public LayoutData(List<WindowController> windows)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                Windows.Add(new WindowData(windows[i]));
            }
        }
    }
}