namespace SimpleWindow.Serialization
{
    using System;

    using SimpleWindow.Internal;

    [Serializable]
    public class TabData
    {
        private string _typeName;
        public bool Active { get; private set; }

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
}