namespace SimpleWindow.Serialization
{
    using System;

    using System.Collections.Generic;

    [Serializable]
    public class ManagerData
    {
        public string CurrentLayout;
        public List<string> Layouts = new List<string>();

        // Constructors

        public ManagerData(LayoutData currentLayout, List<LayoutData> layouts)
        {
            if (currentLayout != null)
                CurrentLayout = currentLayout.Name;

            for (int i = 0; i < layouts.Count; i++)
                Layouts.Add(layouts[i].Name);
        }
    }
}