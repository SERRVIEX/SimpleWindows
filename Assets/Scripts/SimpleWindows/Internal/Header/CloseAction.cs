namespace SimpleWindow.Internal
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public class CloseAction : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private WindowController _reference;

        // Constructors

        private CloseAction() { }

        // Methods

        private void Awake()
        {
            _button.onClick.AddListener(() =>
            {
                WindowsManager.Destroy(_reference);
            });
        }

        public void SetWindow(WindowController controller)
        {
            _reference = controller;
        }

        private void OnValidate()
        {
            if(_button == null)
                _button = GetComponent<Button>();
        }
    }
}