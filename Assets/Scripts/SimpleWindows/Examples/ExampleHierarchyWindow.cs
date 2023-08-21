namespace SimpleWindow.Examples
{
    using UnityEngine;

    public class ExampleHierarchyWindow : Window
    {
        public override string Title { get; protected set; }
        public override Sprite Icon { get; protected set; }

        // Methods

        protected override void Awake()
        {
            base.Awake();

            Title = "Hierarchy";
        }
    }
}