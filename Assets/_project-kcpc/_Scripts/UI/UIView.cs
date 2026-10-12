using UnityEngine;
using UnityEngine.UI;

namespace ProjectKCPC.Scripts.UI
{
    public class UIView : UI
    {
        [SerializeField] private Button _closeButton;

        private GameUI _gameUI;

        private void Start()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        protected virtual void OnEnable()
            => _closeButton.onClick.AddListener(HandleCloseClick);

        protected virtual void OnDisable()
            => _closeButton.onClick.RemoveListener(HandleCloseClick);

        public void Initialize(GameUI gameUI)
            => _gameUI = gameUI;

        public void Open()
            => gameObject.SetActive(true);

        public void Close()
            => gameObject.SetActive(false);

        private void HandleCloseClick()
        {
            _gameUI.CloseView();
            Close();
        }
    }
}