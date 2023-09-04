namespace SimpleWindow.Serialization
{
    using System;
    using System.Collections.Generic;

    using SimpleWindow.Internal;

    [Serializable]
    public class WindowData
    {
        public bool IsFloating { get;private set; }
        public List<TabData> Tabs { get; private set; }

        public float NormalizedSizeX { get; private set; }
        public float NormalizedSizeY { get; private set; }

        public float NormalizedPositionX { get; private set; }
        public float NormalizedPositionY { get; private set; }

        public LayoutType LayoutType { get; private set; }
        public float CenterNormalizedPosition { get; private set; }

        public WindowData Parent { get; private set; }
        public List<WindowData> Children { get; private set; }

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
                    var normalizedSize = controller.GetNormalizedSize();
                    NormalizedSizeX = normalizedSize.x;
                    NormalizedSizeY = normalizedSize.y;

                    var normalizedPosition = controller.GetNormalizedPosition();
                    NormalizedPositionX = normalizedPosition.x;
                    NormalizedPositionY = normalizedPosition.y;
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
                    var normalizedSize = controller.GetNormalizedSize();
                    NormalizedSizeX = normalizedSize.x;
                    NormalizedSizeY = normalizedSize.y;

                    var normalizedPosition = controller.GetNormalizedPosition();
                    NormalizedPositionX = normalizedPosition.x;
                    NormalizedPositionY = normalizedPosition.y;
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
}