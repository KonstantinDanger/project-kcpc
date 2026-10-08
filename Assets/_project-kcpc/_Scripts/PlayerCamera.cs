using ProjectKCPC.Scripts.Config;
using UnityEngine;

namespace ProjectKCPC.Scripts
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private Transform _cameraHolder;
        
        private CameraConfig _config;

        public Quaternion LookRotation => Quaternion.LookRotation(_cameraHolder.forward);
        public Vector3 Forward => _cameraHolder.forward;

        private float _verticalRotation;
        private float _horizontalRotation;

        private bool _isLocked;
        private bool _initialized;

        public void Initialize(bool hasAuthority, CameraConfig config)
        {
            if (_initialized)
                return;

            if (!hasAuthority)
            {
                _cameraHolder.gameObject.SetActive(false);
                _cameraHolder.GetComponentInChildren<Camera>().tag = StaticData.Constants.EmptyTag;

                return;
            }

            _config = config;

            ShowCursor(); //for testing ui
            //HideCursor();

            _initialized = true;
        }

        public void Rotate(Vector3 direction, float speed)
        {
            if (_isLocked)
                return;

            _horizontalRotation += direction.x * speed * Time.deltaTime;
            float verticalDelta = direction.y * speed * Time.deltaTime;

            _verticalRotation -= verticalDelta;
            _verticalRotation = Mathf.Clamp(_verticalRotation, _config.MinRotationAngle, _config.MaxRotationAngle);

            _cameraHolder.localRotation = Quaternion.Euler(_verticalRotation,_horizontalRotation, 0f);

        }

        [ContextMenu("Hide cursor")]
        public void HideCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _isLocked = false;
        }

        [ContextMenu("Show cursor")]
        public void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _isLocked = true;
        }
    }
}
