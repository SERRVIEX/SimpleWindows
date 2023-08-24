namespace SimpleWindow
{
    using UnityEngine;

    using SimpleWindow.Internal;

    public abstract class Window : MonoBehaviour
    {
        public WindowController WindowController;
        public abstract string Title { get; protected set; }
        public abstract Sprite Icon { get; protected set; }

        // Methods

        protected virtual void Awake()
        {
            name = GetType().ToString();
        }

        protected virtual void OnValidate()
        {
            name = GetType().ToString();
        }
    }
}