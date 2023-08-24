namespace SimpleWindow.Internal
{
    using UnityEngine;

    public class Actions : MonoBehaviour
    {
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] public MoveAction Move { get; private set; }
        [field: SerializeField] public CloseAction Close { get; private set; }

        // Constructors

        private Actions() { }

        // Methods

        public void SetWindow(WindowController window)
        {
            Move.SetWindow(window);
            Close.SetWindow(window);
        }

        public void SetFloatActiveActions(bool value)
        {
            Move.gameObject.SetActive(value);
            Close.gameObject.SetActive(value);
        }

        private void OnValidate()
        {
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();
        }
    }
}