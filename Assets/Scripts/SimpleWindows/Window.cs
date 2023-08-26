namespace SimpleWindow
{
    using UnityEngine;

    using SimpleWindow.Internal;

    public abstract class Window : MonoBehaviour
    {
        [HideInInspector] public WindowController Controller;
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }

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