namespace SimpleWindow
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public sealed class WindowLayout : MonoBehaviour
    {
        public LayoutType Layout { get; private set; }
        public float Size;

        public Image Background { get; private set; }

        // Methods

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            if(Background == null)
                Background = GetComponent<Image>();
        }

        public void Update()
        {

        }
    }
}