namespace SimpleWindow.Serialization
{
    using System;

    using System.Collections.Generic;

    using SimpleWindow.Internal;

    [Serializable]
    public class LayoutData
    {
        public string Name { get; private set; }
        public List<WindowData> Windows { get; private set; }

        // Constructors

        public LayoutData(string name, List<WindowController> windows)
        {
            Name = name;
            Windows = new();

            for (int i = 0; i < windows.Count; i++)
                Windows.Add(new WindowData(windows[i]));
        }
    }
}